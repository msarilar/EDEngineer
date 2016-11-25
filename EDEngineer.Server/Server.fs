module Server

open System
open System.Collections.Generic
open System.Linq

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Newtonsoft.Json
open NodaTime
open NodaTime.Text

open EDEngineer.Models.Barda.Collections
open EDEngineer.Models

type Format = Json | Xml | Csv

type Cmdr<'TGood, 'TBad> = 
  | Found of 'TGood
  | Parsed of 'TGood
  | KnownFormat of 'TGood
  | NotFound of 'TBad
  | BadString of 'TBad
  | UnknownFormat of 'TBad

type CmdrBuilder() =
  member this.Bind(v, f) =
    match v with
      | Found v -> f v
      | Parsed v -> f v
      | KnownFormat v -> f v
      | NotFound commander -> (sprintf "Commander %s not found (๑´╹‸╹`๑)" commander) |> NOT_FOUND
      | BadString s -> (sprintf "Couldn't parse time %s ヘ（。□°）ヘ" s) |> BAD_REQUEST
      | UnknownFormat s -> (sprintf "Unknown file format requested %s (╬ ꒪Д꒪)ノ" s) |> BAD_REQUEST
  member this.Return value = value

let cmdr = CmdrBuilder()

let start (token, port, state:Func<IDictionary<string, State>>) =

    let json = fun s -> JsonConvert.SerializeObject(s, Formatting.Indented)
    
    let ingredients = fun (state:State) -> state.Cargo
                                           |> List.ofSeq
                                           |> List.map (fun d -> d.Value.Data)

    let blueprints = fun (state:State) -> state.Blueprints
                                          |> List.ofSeq

    let referenceData = fun (state:Func<IDictionary<string, State>>) -> state.Invoke().First().Value

    let cargoExtractor = fun (commander:State, kind) -> commander.Cargo
                                                        |> List.ofSeq
                                                        |> List.filter 
                                                          (fun e -> match kind with
                                                                    | Some(Kind.Commodity) -> e.Value.Data.Kind = Kind.Commodity
                                                                    | Some(Kind.Data)      -> e.Value.Data.Kind = Kind.Data
                                                                    | Some(Kind.Material)  -> e.Value.Data.Kind = Kind.Material
                                                                    | _                    -> true)
                                                        |> List.map (fun e -> e.Value)
                                                        |> List.map (fun e -> e.Data.Name, e.Count)
                                                        |> dict

    let cargoRoute = fun(state, kind) -> (state, kind) |> cargoExtractor |> json |> OK

    let commanderRoute = fun commander ->
                             match state.Invoke().TryGetValue(commander) with
                             | (true, state) -> Found state
                             | (false, _)    -> NotFound commander

    let timeRoute = fun s -> match s with
                             | Some(t) -> match InstantPattern.GeneralPattern.Parse(t) with
                                          | e when e.Success = true -> Parsed e.Value
                                          | _                       -> BadString t
                             | None    -> Parsed Instant.MinValue

    let listOperations commander =
      request(fun request ->
        let timestampString = match request.queryParam "last" with
                              | Choice1Of2 s     -> Some(s)
                              | Choice2Of2 other -> None

        cmdr {
            let! timestamp = timeRoute timestampString
            let! state = commanderRoute commander
            return state.Operations
                   |> List.ofSeq
                   |> List.filter
                     (fun e -> match e.Timestamp with
                               | t when t >= timestamp -> true
                               | _                     -> false)
                   |> json 
                   |> OK
        }
      )
    
    let FormatExtractor = fun(extension) ->
      match extension with
      | "json" -> KnownFormat Json
      | "csv"  -> KnownFormat Csv
      | "xml"  -> KnownFormat Xml
      | f      -> UnknownFormat f

    let AcceptExtractor = fun(request:HttpRequest) ->
      let accept = request.headers.Where(fun (k, v) -> k = "accept").Select(fun (k, v) -> v).First()
      let accepts = accept.Split [|';'|]
      let format = accepts |> List.ofSeq |> List.fold(fun acc elem -> match acc with
                                                                      | Some(r) -> Some(r)
                                                                      | _       -> match elem with
                                                                                   | "text/json" -> Some(Json)
                                                                                   | "text/csv"  -> Some(Csv)
                                                                                   | "text/xml"  -> Some(Xml)
                                                                                   | _           -> None) None
      match format with
      | Some(f) -> KnownFormat f
      | _       -> KnownFormat Json

    let da = fun format -> format |> json |> OK

    let test da =
      request(fun request ->
          let format = AcceptExtractor request
          da format
      )

    let app =
      choose
        [ GET >=> choose
           [ path "/ingredients" >=> 
               request (fun _ -> referenceData state |> ingredients |> json |> OK)
             path "/blueprints"  >=> 
               request (fun _ -> referenceData state |> blueprints |> json |> OK)
             path "/commanders"  >=> 
               request (fun _ -> state.Invoke().Keys |> json |> OK)
               
             pathScan "/%s/cargo" (fun (commander) -> 
               cmdr {
                 let! state = commanderRoute commander
                 return cargoRoute(state, None)
             })
             pathScan "/%s/materials" (fun (commander) -> 
               cmdr {
                 let! state = commanderRoute commander
                 return cargoRoute(state, Some(Kind.Material))
             })
             pathScan "/%s/data" (fun (commander) -> 
               cmdr {
                 let! state = commanderRoute commander
                 return cargoRoute(state, Some(Kind.Data))
             })
             pathScan "/%s/commodities" (fun (commander) -> 
               cmdr {
                 let! state = commanderRoute commander
                 return cargoRoute(state, Some(Kind.Commodity))
             })

             pathScan "/%s/operations" listOperations
             
             path "/kek" >=> test da
             pathScan "/%s/kek.%s" (fun (commander, format) ->
                 cmdr {
                     let! f = FormatExtractor format
                     return da f
                 }
             )
             
             NOT_FOUND "Route not found ¯\_(ツ)_/¯" ]
        ]

    let localhost = Net.IPAddress.Parse("127.0.0.1")

    startWebServer { 
      defaultConfig with 
        cancellationToken = token
        bindings = [ HttpBinding.mk HTTP localhost port ] } app
    state