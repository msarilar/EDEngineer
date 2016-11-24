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

open EDEngineer.Models.Barda.Collections
open EDEngineer.Models

type Cmdr<'TSuccess, 'TError> = 
    | Found of 'TSuccess
    | NotFound of 'TError

type CmdrBuilder() =
    member this.Bind(v, f) =
        match v with
        | Found v -> f v
        | NotFound commander -> (sprintf "Commander %s not found (๑´╹‸╹`๑)" commander) |> NOT_FOUND
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
                                                                    | Some(Kind.Data) -> e.Value.Data.Kind = Kind.Data
                                                                    | Some(Kind.Material) -> e.Value.Data.Kind = Kind.Material
                                                                    | _ -> true)
                                                        |> List.map (fun e -> e.Value)
                                                        |> List.map (fun e -> e.Data.Name, e.Count)
                                                        |> dict

    let cargoRoute = fun(state, kind) -> (state, kind) |> cargoExtractor |> json |> OK

    let commanderRoute = fun commander ->
                             match state.Invoke().TryGetValue(commander) with
                             | (true, state) -> Found state
                             | (false, _)    -> NotFound commander
    
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
             
             NOT_FOUND "Route not found ¯\_(ツ)_/¯" ]
        ]

    let localhost = Net.IPAddress.Parse("127.0.0.1")

    startWebServer { 
      defaultConfig with 
        cancellationToken = token
        bindings = [ HttpBinding.mk HTTP localhost port ] } app
    state