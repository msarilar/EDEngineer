module CommanderChart

open Newtonsoft.Json

open EDEngineer.Models.Utils
open EDEngineer.Models
open EDEngineer.Models.Operations

let chart commander logDirectory (settings:JsonSerializerSettings) =
    let logWatcher = new LogWatcher(logDirectory)

    logWatcher.RetrieveAllLogs().[commander]
    |> Seq.map (fun l -> JsonConvert.DeserializeObject<JournalEntry>(l, settings))
    |> Seq.filter (fun e -> e.Relevant = true && not (e.JournalOperation :? DumpOperation))
    |> Seq.iter (fun e -> e.JournalOperation.Mutate(state))


    ""