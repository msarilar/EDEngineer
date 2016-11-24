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

let start (token, port, state:Func<IDictionary<string, State>>) =

    let stateStr = sprintf "%A" state

    let json = fun s -> JsonConvert.SerializeObject(s, Formatting.Indented)
    
    let ingredientExtractor = fun (state:State) -> state.Cargo
                                                   |> List.ofSeq
                                                   |> List.map (fun d -> d.Value.Data)

    let blueprintExtractor = fun (state:State) -> state.Blueprints
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

    let cargoRoute = fun (commander, kind) -> 
                       request (fun r -> 
                         match state.Invoke().TryGetValue(commander) with
                         | (true, c) -> (c, kind) |> cargoExtractor |> json |> OK
                         | (false, _) -> NOT_FOUND "Commander not found (๑´╹‸╹`๑)")
    
    let app =
      choose
        [ GET >=> choose
           [ path "/ingredients" >=> 
               request (fun _ -> referenceData state |> ingredientExtractor |> json |> OK)
             path "/blueprints"  >=> 
               request (fun _ -> referenceData state |> blueprintExtractor |> json |> OK)
             path "/commanders"  >=> 
               request (fun _ -> state.Invoke().Keys |> json |> OK)

             pathScan "/%s/cargo"       (fun (commander) -> cargoRoute (commander, None))
             pathScan "/%s/data"        (fun (commander) -> cargoRoute (commander, Some(Kind.Data)))
             pathScan "/%s/materials"   (fun (commander) -> cargoRoute (commander, Some(Kind.Material)))
             pathScan "/%s/commodities" (fun (commander) -> cargoRoute (commander, Some(Kind.Commodity)))
               

             NOT_FOUND "Route not found ¯\_(ツ)_/¯" ]
        ]

    let localhost = Net.IPAddress.Parse("127.0.0.1")

    startWebServer { 
      defaultConfig with 
        cancellationToken = token
        bindings = [ HttpBinding.mk HTTP localhost port ] } app
    state