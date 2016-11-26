module Server

open System
open System.Collections.Generic
open System.Linq
open System.Data
open System.IO

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
type cargoType = { Kind: string; Name: string; Count: int }

let inline (|?) (a) b = if a = null then b else a  

type Cmdr<'TGood, 'TBad> = 
  | Found         of 'TGood
  | Parsed        of 'TGood
  | KnownFormat   of 'TGood
  | NotFound      of 'TBad
  | BadString     of 'TBad
  | UnknownFormat of 'TBad
  | RouteNotFound of 'TBad

type CmdrBuilder() =
  member this.Bind(v, f) =
    match v with
      | Found v             -> f v
      | Parsed v            -> f v
      | KnownFormat v       -> f v
      | NotFound commander  -> (sprintf "Commander %s not found (๑´╹‸╹`๑)" commander) |> NOT_FOUND
      | BadString s         -> (sprintf "Couldn't parse time %s ヘ（。□°）ヘ" s) |> BAD_REQUEST
      | UnknownFormat s     -> (sprintf "Unknown file format requested %s (╬ ꒪Д꒪)ノ" s) |> BAD_REQUEST
      | RouteNotFound _     -> "Route not found ¯\_(ツ)_/¯" |> NOT_FOUND
  member this.Return value = value

let cmdr = CmdrBuilder()

let start (token, port, state:Func<IDictionary<string, State>>) =

  let json = fun s -> JsonConvert.SerializeObject s
  let xml = fun s ->
    let xmlDoc = (json(s) |> sprintf "{ \"item\": %s }", "root") |> JsonConvert.DeserializeXmlNode
    xmlDoc.InnerXml
  let csv = fun s -> (json(s) |> sprintf "{ \"item\": %s }") |> JsonUtils.ToCsv
    
  let ingredients = fun (state:State) -> state.Cargo
                                         |> Seq.map (fun d -> d.Value.Data)

  let blueprints = fun (state:State) -> state.Blueprints

  let referenceData = fun (state:Func<IDictionary<string, State>>) -> state.Invoke().First().Value

  let cargoExtractor = fun (state:State, kind) -> state.Cargo
                                                  |> Seq.map (fun e -> e.Value)
                                                  |> Seq.filter 
                                                    (fun e -> match kind with
                                                              | Some(Kind.Commodity) -> e.Data.Kind = Kind.Commodity
                                                              | Some(Kind.Data)      -> e.Data.Kind = Kind.Data
                                                              | Some(Kind.Material)  -> e.Data.Kind = Kind.Material
                                                              | _                    -> true)
                                                  |> Seq.map (fun e -> { Kind = e.Data.Kind.ToString(); Name = e.Data.Name; Count = e.Count })
                                                  |> List.ofSeq

  let commanderRoute = fun commander ->
                         match state.Invoke().TryGetValue(commander) with
                         | (true, state) -> Found state
                         | (false, _)    -> NotFound commander


  let timeRoute = function
                  | Some(t) -> match InstantPattern.GeneralPattern.Parse(t) with
                               | e when e.Success = true -> Parsed e.Value
                               | _                       -> BadString t
                  | None    -> Parsed Instant.MinValue

  let AcceptExtractor = fun(request:HttpRequest) ->
    request.headers
    |> Seq.filter (fun (k, v) -> k = "accept")
    |> Seq.map (fun (k, v) -> v)
    |> Seq.head
    |> fun x -> x.Split [|';'|]
    |> Seq.fold(fun acc elem -> match acc with
                                 | Some(r) -> Some(r)
                                 | _       -> match elem with
                                              | "text/json" -> Some(Json)
                                              | "text/csv"  -> Some(Csv)
                                              | "text/xml"  -> Some(Xml)
                                              | _           -> None) None
    |> function
      | Some(f) -> f
      | _       -> Json

  let FormatExtractor (request: HttpRequest) =
    function
    | ".json"                   -> KnownFormat Json
    | ".csv"                    -> KnownFormat Csv
    | ".xml"                    -> KnownFormat Xml
    | s when s.StartsWith(".")  -> UnknownFormat s
    | s when s = ""             -> 
      let formatFromRequest = AcceptExtractor request
      KnownFormat formatFromRequest
    | f                         -> RouteNotFound f

  let FormatPicker =
    function
    | Xml   -> xml
    | Json  -> json
    | Csv   -> csv

  let MimeType = 
    function
    | Xml   -> Writers.setMimeType "text/xml; charset=utf-8"
    | Json  -> Writers.setMimeType "text/json; charset=utf-8"
    | Csv   -> Writers.setMimeType "text/csv; charset=utf-8"

  let app =
    choose
      [ 
        pathScan "/ingredients%s" (fun (format) -> 
          (request(fun request ->
            cmdr {
              let! f = FormatExtractor request format 
              return  referenceData state |> ingredients |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        pathScan "/blueprints%s" (fun (format) -> 
          (request(fun request ->
            cmdr {
              let! f = FormatExtractor request format
              return  referenceData state |> blueprints |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        pathScan "/commanders%s" (fun (format) -> 
          (request(fun request ->
            cmdr {
              let! f = FormatExtractor request format
              return  state.Invoke().Keys |> FormatPicker(f) |> OK >=> MimeType(f)
            })))
               
        pathScan "/%s/cargo%s" (fun (commander, format) -> 
          (request(fun request ->
            cmdr {
              let! s = commanderRoute commander
              let! f = FormatExtractor request format
              return (s, None) |> cargoExtractor |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        pathScan "/%s/materials%s" (fun (commander, format) -> 
          (request(fun request ->
            cmdr {
                let! s = commanderRoute commander
                let! f = FormatExtractor request format
                return (s, Some(Kind.Material)) |> cargoExtractor |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        pathScan "/%s/data%s" (fun (commander, format) -> 
          (request(fun request ->
            cmdr {
                let! s = commanderRoute commander
                let! f = FormatExtractor request format
                return (s, Some(Kind.Data)) |> cargoExtractor |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        pathScan "/%s/commodities%s" (fun (commander, format) -> 
          (request(fun request ->
            cmdr {
                let! s = commanderRoute commander
                let! f = FormatExtractor request format
                return (s, Some(Kind.Commodity)) |> cargoExtractor |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        pathScan "/%s/operations%s" (fun (commander, format) ->
          (request(fun request ->
            let timestampString = match request.queryParam "last" with
                                  | Choice1Of2 s     -> Some(s)
                                  | Choice2Of2 other -> None

            cmdr {
              let! state = commanderRoute commander
              let! f = FormatExtractor request format
              let! timestamp = timeRoute timestampString
              return state.Operations
                      |> Seq.filter
                        (fun e -> match e.Timestamp with
                                  | t when t >= timestamp -> true
                                  | _                     -> false)
                      |> FormatPicker(f) 
                      |> OK >=> MimeType(f)
            })))
             
        NOT_FOUND "Route not found ¯\_(ツ)_/¯" ]
      

  let localhost = Net.IPAddress.Parse("127.0.0.1")

  startWebServer { 
    defaultConfig with 
      cancellationToken = token
      bindings = [ HttpBinding.mk HTTP localhost port ] } app
  state