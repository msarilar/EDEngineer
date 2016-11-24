module Server

open EDEngineer.Models
open System
open System.Collections.Generic

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Newtonsoft.Json

open EDEngineer.Models.Barda.Collections

let start (state:Func<IDictionary<string, State>>) =

    let stateStr = sprintf "%A" state

    let commandersJson = fun s -> JsonConvert.SerializeObject s

    let app =
      choose
        [ GET >=> choose
           [ path "/commanders" >=> request (fun _ -> state.Invoke() |> commandersJson |> OK)
             path "/status" >=> OK "running"
             path "/state" >=> OK stateStr ]
        ]

    startWebServer defaultConfig app
    state