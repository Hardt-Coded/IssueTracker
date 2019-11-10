namespace Infrastructure



module EventStore =

    open System
    open FSharp.Control.Tasks.V2
    open Newtonsoft.Json
    open Streamstone
    open Domain.Common
    
    
    let connectionString = ""

    [<CLIMutable>]
    type EventEntity = {
        Id:string
        EventType:string
        Data:string
        Version:int
    }

    let inline toEventData aggregate (data:^ev when ^ev : (member EventType:string)) =
        let id = sprintf "%s-%s" aggregate (Guid.NewGuid().ToString("N"))
        let json = JsonConvert.SerializeObject(data)
        let eventType = (^ev: (member EventType:string) data)
        let properties = {
            Id = id            
            EventType = eventType
            Data = json
            Version = 0
        }
        EventData(EventId.From(id),EventProperties.From(properties))


    let inline storeEvents eventTable aggregate id (events:^ev list when ^ev : (member EventType:string)) =
        task {
            let partitionKey = sprintf "%s-%s" aggregate id
            let partition = Partition(eventTable,partitionKey)
            let! streamExisist = Stream.TryOpenAsync(partition)
            let stream =
                if streamExisist.Found then
                    streamExisist.Stream
                else
                    Stream(partition)
            //let currentVersion = stream.Version

            let evs = 
                events
                |> List.map (fun i -> i |> toEventData aggregate)
                |> List.toArray

            try
                let! result = Stream.WriteAsync(stream, evs)
                return result.Stream.Version |> Ok
            with
            | _ as e ->
                return e |> InfrastructureError |> Error
        }

    let private readEvents eventTable eventConverter aggregate startVersion id =
        task {
            let partitionKey = sprintf "%s-%s" aggregate id
            let partition = Partition(eventTable,partitionKey)
            let! exists = Stream.ExistsAsync(partition)
            if not exists then
                return None |> Ok
            else
                try
                    let! res = Stream.ReadAsync<EventEntity>(partition, startVersion)
                    let events =
                        res.Events
                        |> Seq.map (eventConverter)
                        |> Seq.toList
                    return events |> Some |> Ok
                with
                | _ as e ->
                    return e |> InfrastructureError |> Error
        }
    

    module User =

        open Dtos.User
        open System.Threading.Tasks

        let readEvents eventTable aggregate startVersion id : Task<Result<(Domain.User.Event * int) list option,Errors>> =
            let eventConverter (event:EventEntity) : Domain.User.Event * int =
                match event.EventType with
                | x when x = nameof UserCreated ->
                    let e = JsonConvert.DeserializeObject<UserCreated>(event.Data)
                    e |> Dtos.User.toDomain, event.Version
                | x when x = nameof  UserDeleted ->
                    let e = JsonConvert.DeserializeObject<UserDeleted>(event.Data)
                    e |> Dtos.User.toDomain, event.Version
                | x when x = nameof  EMailChanged ->
                    let e = JsonConvert.DeserializeObject<EMailChanged>(event.Data)
                    e |> Dtos.User.toDomain, event.Version
                | x when x = nameof  PasswordChanged ->
                    let e = JsonConvert.DeserializeObject<PasswordChanged>(event.Data)
                    e |> Dtos.User.toDomain, event.Version
                | x when x = nameof  AddedToGroup ->
                    let e = JsonConvert.DeserializeObject<AddedToGroup>(event.Data)
                    e |> Dtos.User.toDomain, event.Version
                | x when x = nameof  RemovedFromGroup ->
                    let e = JsonConvert.DeserializeObject<RemovedFromGroup>(event.Data)
                    e |> Dtos.User.toDomain, event.Version
                | _ ->
                    failwith "can not convert event"

            readEvents eventTable eventConverter aggregate startVersion id
                    
                
        


         