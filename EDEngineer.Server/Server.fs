module Server

open EDEngineer.Models

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

let start state:State =

    let stateStr = sprintf "%A" state

    let app =
      choose
        [ GET >=> choose
           [ path "/commanders" >=> OK ""
             path "/status" >=> OK "running"
             path "/state" >=> OK stateStr ]
        ]

    startWebServer defaultConfig app
    state