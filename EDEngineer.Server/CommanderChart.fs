module CommanderChart

open EDEngineer.Models.Utils

let chart commander logFolder =
    let logWatcher = new LogWatcher(logFolder)
    ""