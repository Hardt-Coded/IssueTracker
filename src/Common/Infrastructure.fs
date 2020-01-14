

namespace Common

open Microsoft.WindowsAzure.Storage.Table
open System.Collections
open System.Collections.Generic

module Infrastructure =

    type DictionaryTableEntity () =
        inherit TableEntity () 
        let mutable _properties = 
            Dictionary<string,EntityProperty>() :> IDictionary<string,EntityProperty>

        override this.ReadEntity(properties,operationContext) =
            _properties <- properties

        override this.WriteEntity(operationContext) =
            _properties

        member this.GetProperties () = 
            _properties





    module EventStore =

        open System
        open FSharp.Control.Tasks.V2
        open Newtonsoft.Json.Linq
        open Domain
        open Dtos
        open CosmoStore
        open CosmoStore.TableStorage
        open System.Threading.Tasks
        open Microsoft.WindowsAzure.Storage
        open CosmoStore.TableStorage
        open Newtonsoft.Json.Linq


        // create eventStore
        let eventStore tableName connection =
            let lazyEventStore =
                lazy(
                    task {
                        let account = CloudStorageAccount.Parse(connection)
                        let authKey = account.Credentials.ExportBase64EncodedKey()
                        let config = 
                            if (connection.StartsWith("UseDevelopmentStorage")) then
                                TableStorage.Configuration.CreateDefaultForLocalEmulator ()
                            else
                                TableStorage.Configuration.CreateDefault account.Credentials.AccountName authKey            
                        let config = { config with TableName = tableName }            
                        return TableStorage.EventStore.getEventStore config
                    }    
                )

            lazyEventStore.Force



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


        let storeEvents (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate id (events:IEvent list) =
            task {
                let! eventStore = getEventStore ()
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
                    return 
                        e 
                        |> InfrastructureError 
                        |> List.singleton 
                        |> Error
            }


        let readAllEvents (getEventStore:unit -> Task<EventStore<JToken,int64>>) eventConverter aggregate id =
            task {
                let! eventStore = getEventStore ()
                let streamId = sprintf "%s-%s" aggregate id
                try
                    let! result = EventsReadRange.AllEvents |> eventStore.GetEvents streamId
                
                    let events =
                        result
                        |> Seq.map (eventConverter)
                        |> Seq.toList
                    return events |> Ok
                with
                | _ as e ->
                    return 
                        e 
                        |> InfrastructureError 
                        |> List.singleton
                        |> Error
                
            }


        let readEventsSpecificVersion (getEventStore:unit -> Task<EventStore<JToken,int64>>) eventConverter aggregate id version =
            task {
                let! eventStore = getEventStore ()
                let streamId = sprintf "%s-%s" aggregate id
                try
                    let! result = EventsReadRange.FromVersion(version) |> eventStore.GetEvents streamId
                    let events =
                        result
                        |> Seq.map (eventConverter)
                        |> Seq.toList
                    return events |> Ok
                with
                | _ as e ->
                    return 
                        e 
                        |> InfrastructureError 
                        |> List.singleton
                        |> Error
                
            }


        let readAllStreams (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate =
            task {
                let! eventStore = getEventStore ()
                try
                    let! streams = StreamsReadFilter.StartsWith(aggregate) |> eventStore.GetStreams
                    return streams |> Ok
                with
                | _ as e ->
                    return 
                        e 
                        |> InfrastructureError 
                        |> List.singleton
                        |> Error
        
            }


            


