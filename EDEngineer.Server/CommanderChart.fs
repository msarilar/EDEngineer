module CommanderChart

open Newtonsoft.Json

open EDEngineer.Models.Utils
open EDEngineer.Models
open EDEngineer.Models.Operations
open XPlot.Plotly
open System
open System.Collections.Generic

type Record = {
    Count: int;
    Timestamp: DateTime;
}

let random = new Random()

let randomRgb() =
    let r = random.Next(0, 255)
    let g = random.Next(0, 255)
    let b = random.Next(0, 255)
    sprintf "rgb(%i, %i, %i)" r g b

let chartData commander logDirectory (settings:JsonSerializerSettings) (languages:ILanguage) (l:LanguageInfo) =
    let logWatcher = new LogWatcher(logDirectory)

    let folder (map:Map<string, Record list>) (item:JournalEntry) =
        let microFolder (map:Map<string, Record list>) (kv:KeyValuePair<string, int>) =
            let key = languages.Translate(kv.Key, l)
            let newValue = 
                match map.TryFind key with
                | None -> [{ Count = kv.Value; Timestamp = item.Timestamp.ToDateTimeUtc() }]
                | Some values ->
                    let newRecord = { Count = kv.Value + values.Head.Count; Timestamp = item.Timestamp.ToDateTimeUtc() }
                    newRecord::values
            map.Add(key, newValue)

        item.JournalOperation.Changes
        |> Seq.fold microFolder map

    logWatcher.RetrieveAllLogs().[commander]
    |> Seq.map (fun l -> JsonConvert.DeserializeObject<JournalEntry>(l, settings))
    |> Seq.filter (fun e -> e.Relevant = true && e.JournalOperation.Changes <> null && not (e.JournalOperation :? DumpOperation)&& not (e.JournalOperation :? ManualChangeOperation))
    |> Seq.fold folder Map.empty<string, Record list>
    |> Map.toSeq

let chart commander logDirectory (settings:JsonSerializerSettings) (languages:ILanguage) (l:LanguageInfo) =
    let toBar name (values:seq<Record>) =
        let times = values |> Seq.map (fun v -> v.Timestamp) |> Seq.toArray
        let amounts = values |> Seq.map (fun v -> v.Count) |> Seq.toArray
        Bar(
            x = times,
            y = amounts,
            name = languages.Translate(name, l),
            marker = Marker(color = randomRgb))

    let toScatter name (values:seq<Record>) =
        let times = values |> Seq.map (fun v -> v.Timestamp) |> Seq.toArray
        let amounts = values |> Seq.map (fun v -> v.Count) |> Seq.toArray
        let color = randomRgb()
        Scatter(
            x = times,
            y = amounts,
            name = languages.Translate(name, l),
            marker =
                Marker(
                    color = color,
                    line =
                        Line(
                            color = color,
                            width = 0.5
                        )
                ))

    let data =
        chartData commander logDirectory settings languages l
        |> Seq.map (fun (k, values) -> toScatter k values)

    let styledLayout =
        Layout(
            title = "Cargo Progression",
            xaxis =
                Xaxis(
                    tickfont =
                        Font(
                            size = 14.,
                            color = "rgb(107, 107, 107)"
                        )
                ),
            yaxis =
                Yaxis(
                    titlefont =
                        Font(
                            size = 16.,
                            color = "rgb(107, 107, 107)"
                        ),
                    tickfont =
                        Font(
                            size = 14.,
                            color = "rgb(107, 107, 107)"
                        )
                ),
            legend =
                Legend(
                    x = 1250.,
                    y = 1.0,
                    bgcolor = "rgba(100, 100, 100, 0)",
                    bordercolor = "rgba(0, 0, 0)"
                ),
            barmode = "group",
            bargap = 0.15,
            bargroupgap = 0.1
        )

    let chart =
        data
        |> Chart.Plot
        |> Chart.WithLayout styledLayout
        |> Chart.WithWidth 1200
        |> Chart.WithHeight 800

    chart.GetHtml()