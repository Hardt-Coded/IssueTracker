namespace Infrastructure

open Microsoft.WindowsAzure.Storage
open CosmoStore.TableStorage
open Newtonsoft.Json.Linq



module EventStore =

    open System
    open FSharp.Control.Tasks.V2
    open Newtonsoft.Json
    open Domain.Common
    open Dtos.Common
    open CosmoStore
    open CosmoStore.TableStorage

    
    
    let connectionString = ""

    [<CLIMutable>]
    type EventEntity = {
        Id:string
        EventType:string
        Data:string
        Version:int
    }

    let private tableName = "IssueTrackerEventSource"


    let getEventStore connectionString =
        task {
            let account = CloudStorageAccount.Parse(connectionString)
            let authKey = account.Credentials.ExportBase64EncodedKey()
            let config = 
                if (connectionString.StartsWith("UseDevelopmentStorage")) then
                    TableStorage.Configuration.CreateDefaultForLocalEmulator ()
                else
                    TableStorage.Configuration.CreateDefault account.Credentials.AccountName authKey            
            let config = { config with TableName = tableName }            
            return TableStorage.EventStore.getEventStore config
        }
        
        


    let private toEventData aggregate (data:IEvent) =
        let jToken = JToken.FromObject(data)
        {
            EventWrite.Id = Guid.NewGuid()
            CorrelationId = None
            CausationId = None
            Name = data.EventType
            Data = jToken 
            Metadata = None
        }


    let storeEvents connectionString aggregate id (events:IEvent list) =
        task {
            let! eventStore = getEventStore connectionString
            let streamId = sprintf "%s-%s" aggregate id
                
            try 
                let batchedEvents =
                    events
                    |> List.map (fun i -> i |> toEventData aggregate)
                    |> List.chunkBySize 99

                for batch in batchedEvents do
                    let! x = eventStore.AppendEvents streamId ExpectedVersion.Any batch
                    ()

                return () |> Ok
            with
            | _ as e ->
                return e |> InfrastructureError |> Error
        }


    let private readAllEvents connectionString eventConverter aggregate id =
        task {
            let! eventStore = getEventStore connectionString
            let streamId = sprintf "%s-%s" aggregate id
            try
                let! result = EventsReadRange.AllEvents |> eventStore.GetEvents streamId
                match result with
                | [] ->
                    return None |> Ok
                | _ ->
                    let events =
                        result
                        |> Seq.map (eventConverter)
                        |> Seq.toList
                    return events |> Some |> Ok
            with
            | _ as e ->
                return e |> InfrastructureError |> Error
                
        }


    let private readEventsSpecificVersion connectionString eventConverter aggregate id version =
        task {
            let! eventStore = getEventStore connectionString
            let streamId = sprintf "%s-%s" aggregate id
            try
                let! result = EventsReadRange.FromVersion(version) |> eventStore.GetEvents streamId
                match result with
                | [] ->
                    return None |> Ok
                | _ ->
                    let events =
                        result
                        |> Seq.map (eventConverter)
                        |> Seq.toList
                    return events |> Some |> Ok
            with
            | _ as e ->
                return e |> InfrastructureError |> Error
                
        }

    let private readAllStreams connectionString aggregate =
        task {
            let! eventStore = getEventStore connectionString
            try
                let! streams = StreamsReadFilter.StartsWith(aggregate) |> eventStore.GetStreams
                return streams |> Ok
            with
            | _ as e ->
                return e |> InfrastructureError |> Error
        
        }
    
    
   

    module User =

        open Dtos.User.Events
        open System.Threading.Tasks
        


        let private eventConverter (event:EventRead<JToken,int64>) : Domain.User.Event * int64 =
            match event.Name with
            | x when x = nameof UserCreated ->
                let e = event.Data.ToObject<UserCreated>()
                e |> toDomain, event.Version
            | x when x = nameof  UserDeleted ->
                let e = event.Data.ToObject<UserDeleted>()
                e |> toDomain, event.Version
            | x when x = nameof  EMailChanged ->
                let e = event.Data.ToObject<EMailChanged>()
                e |> toDomain, event.Version
            | x when x = nameof  PasswordChanged ->
                let e = event.Data.ToObject<PasswordChanged>()
                e |> toDomain, event.Version
            | x when x = nameof  AddedToGroup ->
                let e = event.Data.ToObject<AddedToGroup>()
                e |> toDomain, event.Version
            | x when x = nameof  RemovedFromGroup ->
                let e = event.Data.ToObject<RemovedFromGroup>()
                e |> toDomain, event.Version
            | _ ->
                failwith "can not convert event"


        let readEvents connectionString aggregate id : Task<Result<(Domain.User.Event * int64) list option,Errors>> =
            readAllEvents connectionString eventConverter aggregate id


        let readEventsStartSpecificVersion connectionString aggregate id version : Task<Result<(Domain.User.Event * int64) list option,Errors>> =
            readEventsSpecificVersion connectionString eventConverter aggregate id version 


        //let readAllEventsFromUser connectionString aggregate =
        //    readAllEventsFromAggregate connectionString eventConverter aggregate
            
                    
                
        


         