module Server

open System
open System.Collections.Generic
open System.Linq
open System.Data
open System.IO

open Suave
open Suave.CORS
open Suave.Filters
open Suave.Writers
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Newtonsoft.Json
open NodaTime
open NodaTime.Text

open EDEngineer.Models.Utils.Json
open EDEngineer.Models.State
open EDEngineer.Models

type Format = Json | Xml | Csv
type cargoType = { Kind: string; Name: string; Count: int; }
type ShoppingListItem = {
    Blueprint:Object;
    Count:int;
}

let inline (|?) (a) b = if a = null then b else a  

type Cmdr<'TGood, 'TBad> = 
  | Found            of 'TGood
  | Parsed           of 'TGood
  | KnownFormat      of 'TGood
  | Parameter        of 'TGood
  | NotFound         of 'TBad
  | MissingParameter of 'TBad
  | BadParameter     of 'TBad
  | BadString        of 'TBad
  | UnknownFormat    of 'TBad
  | RouteNotFound    of 'TBad
  | UnknownLanguage  of 'TBad

type CmdrBuilder() =
  member this.Bind(v, f) =
    match v with
      | Found v             -> f v
      | Parameter v         -> f v
      | Parsed v            -> f v
      | KnownFormat v       -> f v
      | NotFound commander  -> (sprintf "Commander %s not found" commander) |> NOT_FOUND
      | BadString s         -> (sprintf "Couldn't parse time %s" s) |> BAD_REQUEST
      | MissingParameter s  -> (sprintf "Couldn't find required parameter %s" s) |> BAD_REQUEST
      | BadParameter v      -> (sprintf "Parameter has an incorrect value %s" v) |> BAD_REQUEST
      | UnknownFormat s     -> (sprintf "Unknown file format requested %s" s) |> BAD_REQUEST
      | RouteNotFound _     -> "Route not found" |> NOT_FOUND
      | UnknownLanguage s   -> (sprintf "Unknown language requested %s" s) |> BAD_REQUEST
  member this.Return value = value

let cmdr = CmdrBuilder()

let start (token,
           port,
           translator:ILanguage,
           state:Func<IDictionary<string, State>>,
           shopppingLists:Func<IDictionary<string, List<Tuple<Blueprint, int>>>>,
           changeShoppingList:Action<string, Blueprint, int>) =
  
  let corsConfig = {
    defaultCORSConfig with
      allowedUris = InclusiveOption.Some [ "http://localhost:" + (port |> string) ]
  }
  let JsonConfig = fun lang -> 
    let settings = new JsonSerializerSettings (ContractResolver = new LocalizedContractResolver(translator, lang))
    settings.Converters.Add(new LocalizedJsonConverter(translator, lang))
    settings

  let json = fun (s, l) -> JsonConvert.SerializeObject(s, JsonConfig(l))
  let xml = fun (s, l) ->
    let xmlDoc = (json(s, l) |> sprintf "{ \"item\": %s }", "root") |> JsonConvert.DeserializeXmlNode
    xmlDoc.InnerXml
  let csv = fun (s, l) -> (json(s, l) |> sprintf "{ \"item\": %s }") |> JsonUtils.ToCsv
    
  let ingredients = fun (state:State) -> state.Cargo.Ingredients
                                         |> Seq.map (fun d -> d.Value.Data)

  let blueprints = fun (state:State) -> state.Blueprints

  let referenceData = fun (state:Func<IDictionary<string, State>>) -> state.Invoke().First().Value

  let cargoExtractor = fun (state:State, kind) -> state.Cargo.Ingredients
                                                  |> Seq.map (fun e -> e.Value)
                                                  |> Seq.filter 
                                                    (fun e -> match kind with
                                                              | Some(Kind.Commodity) -> e.Data.Kind = Kind.Commodity
                                                              | Some(Kind.Data)      -> e.Data.Kind = Kind.Data
                                                              | Some(Kind.Material)  -> e.Data.Kind = Kind.Material
                                                              | _                    -> true)
                                                  |> Seq.map (fun e -> { 
                                                                          Kind = e.Data.Kind.ToString();
                                                                          Name = e.Data.Name;
                                                                          Count = e.Count;
                                                                        })
                                                  |> List.ofSeq
  let toGuid str =
    match Guid.TryParse str with
    | (true, guid) -> Some guid
    | (_, _) -> None

  let toInt str =
    match Int32.TryParse str with
    | (true, intValue) -> Some intValue
    | (_, _) -> None

  let requestParameter =
    fun (request:HttpRequest) parameter transformation ->
      match request.fieldData parameter with
      | Choice1Of2 value ->
        match transformation value with
        | Some result -> Parameter result
        | None -> BadParameter value
      | Choice2Of2 _ -> MissingParameter parameter

  let stateRoute = fun commander ->
                     match state.Invoke().TryGetValue(Uri.UnescapeDataString(commander)) with
                     | (true, state) -> Found state
                     | (false, _)    -> NotFound commander

  let shoppingListRoute = fun commander ->
                            match shopppingLists.Invoke().TryGetValue(Uri.UnescapeDataString(commander)) with
                            | (true, state) -> Found state
                            | (false, _)    -> NotFound commander

  let timeRoute = function
                  | Some(t) -> match InstantPattern.General.Parse(t) with
                               | e when e.Success = true -> Parsed e.Value
                               | _                       -> BadString t
                  | None    -> Parsed Instant.MinValue

  let AcceptExtractor = fun(request:HttpRequest) ->
    let findFormat (x: string) =
        x.Split [|';'|]
        |> Seq.fold(fun acc elem -> match acc with
                                 | Some(r) -> Some(r)
                                 | _       -> match elem with
                                              | "text/json" -> Some(Json)
                                              | "text/csv"  -> Some(Csv)
                                              | "text/xml"  -> Some(Xml)
                                              | _           -> None) None
    request.headers
    |> Seq.filter (fun (k, v) -> k = "accept")
    |> Seq.map (fun (k, v) -> v)
    |> Seq.tryHead
    |> Option.bind findFormat
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
    
  let LanguageExtractor =
    function
    | Choice1Of2 s -> match translator.TryGetLangInfo(s) with
                      | (true, l)  -> Found l
                      | (false, l) -> UnknownLanguage s
    | Choice2Of2 _ -> Found translator.DefaultLanguage

  let app =
    choose
      [ 
        GET >=> pathScan "/languages%s" (fun (format) -> 
          (request(fun request ->
            cmdr {
              let! f = FormatExtractor request format
              let! l = LanguageExtractor <| request.queryParam "lang"
              return (translator.LanguageInfos, l) |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        GET >=> pathScan "/ingredients%s" (fun (format) -> 
          (request(fun request ->
            cmdr {
              let! f = FormatExtractor request format 
              let! l = LanguageExtractor <| request.queryParam "lang"
              return (referenceData state |> ingredients, l) |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        GET >=> pathScan "/blueprints%s" (fun (format) -> 
          (request(fun request ->
            cmdr {
              let! f = FormatExtractor request format
              let! l = LanguageExtractor <| request.queryParam "lang"
              return (referenceData state |> blueprints, l) |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        GET >=> pathScan "/commanders%s" (fun (format) -> 
          (request(fun request ->
            cmdr {
              let! f = FormatExtractor request format
              let! l = LanguageExtractor <| request.queryParam "lang"
              return (state.Invoke().Keys, l) |> FormatPicker(f) |> OK >=> MimeType(f)
            })))
               
        GET >=> pathScan "/%s/cargo%s" (fun (commander, format) -> 
          (request(fun request ->
            cmdr {
              let! s = stateRoute commander
              let! f = FormatExtractor request format
              let! l = LanguageExtractor <| request.queryParam "lang"
              return ((s, None) |> cargoExtractor, l) |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        GET >=> pathScan "/%s/materials%s" (fun (commander, format) -> 
          (request(fun request ->
            cmdr {
                let! s = stateRoute commander
                let! f = FormatExtractor request format
                let! l = LanguageExtractor <| request.queryParam "lang"
                return ((s, Some(Kind.Material)) |> cargoExtractor, l) |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        GET >=> pathScan "/%s/data%s" (fun (commander, format) -> 
          (request(fun request ->
            cmdr {
                let! s = stateRoute commander
                let! f = FormatExtractor request format
                let! l = LanguageExtractor <| request.queryParam "lang"
                return ((s, Some(Kind.Data)) |> cargoExtractor, l) |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        GET >=> pathScan "/%s/commodities%s" (fun (commander, format) -> 
          (request(fun request ->
            cmdr {
                let! s = stateRoute commander
                let! f = FormatExtractor request format
                let! l = LanguageExtractor <| request.queryParam "lang"
                return ((s, Some(Kind.Commodity)) |> cargoExtractor, l) |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        PATCH >=> pathScan "/%s/shopping-list" (fun commander ->
          (request(fun request ->
            cmdr {
               let! s = stateRoute commander

               let findBlueprint =
                 fun guid ->
                  let matchingBlueprints = s.Blueprints |> Seq.filter (fun b -> b.CoriolisGuid.HasValue && b.CoriolisGuid.Value = guid)
                  match matchingBlueprints |> List.ofSeq with
                  | [] -> None
                  | v::[] -> Some v
                  | v::_ -> Some v

               let! blueprint = requestParameter request "uuid" (toGuid >> Option.bind findBlueprint)
               let! size = requestParameter request "size" toInt

               match blueprint.ShoppingListCount + size with
               | diff when diff >= 0 ->
                 changeShoppingList.Invoke(commander, blueprint, size)
                 return NO_CONTENT
               | _ ->
                 let blueprintString = blueprint.ToString()
                 return (sprintf "Tried to change shopping list by %i but result would be negative (Blueprint %s)" size blueprintString) |> BAD_REQUEST
            })))

        GET >=> pathScan "/%s/shopping-list%s" (fun (commander, format) -> 
          let toShoppingListItem (blueprint: Blueprint, count: int) =
            { Blueprint = blueprint.ToSerializable(); Count = count }

          (request(fun request ->
            cmdr {
                let! s = shoppingListRoute commander
                let! f = FormatExtractor request format
                let! l = LanguageExtractor <| request.queryParam "lang"
                return (s |> Seq.map toShoppingListItem, l) |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        GET >=> pathScan "/%s/operations%s" (fun (commander, format) ->
          (request(fun request ->
            let timestampString = match request.queryParam "last" with
                                  | Choice1Of2 s     -> Some(s)
                                  | Choice2Of2 _ -> None

            cmdr {
              let! state = stateRoute commander
              let! f = FormatExtractor request format
              let! timestamp = timeRoute timestampString
              let! l = LanguageExtractor <| request.queryParam "lang"
              let operations = state.Operations
                               |> Seq.filter
                                 (fun e -> match e.Timestamp with
                                           | t when t >= timestamp -> true
                                           | _                     -> false)
              return (operations, l) |> FormatPicker(f) |> OK >=> MimeType(f)
            })))

        OPTIONS >=>
            fun context ->
                context |> (
                    OK "CORS approved"
                    >=> addHeader "Access-Control-Allow-Methods" "GET, PATCH, OPTIONS" )

        NOT_FOUND "Route not found" ]
           >=> addHeader "Access-Control-Allow-Origin" "*"
           >=> cors corsConfig

  startWebServer { 
    defaultConfig with 
      cancellationToken = token
      bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" port ] } app
  state