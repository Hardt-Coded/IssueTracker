namespace Users


module EventStore =

    open Newtonsoft.Json.Linq
    open CosmoStore
    open System.Threading.Tasks
    open Common.Infrastructure
    open Common.Dtos
    open Common.Domain
    open Users.Dtos.Events
    open Users.Domain
    


    let private eventConverter (event:EventRead<JToken,int64>) : Event * int64 =
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
        | x when x = nameof  NameChanged ->
            let e = event.Data.ToObject<NameChanged>()
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


    let private readEvents (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate id : Task<Result<(Event * int64) list,Errors list>> =
        EventStore.readAllEvents getEventStore eventConverter aggregate id


    let private readEventsStartSpecificVersion (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate id version : Task<Result<(Event * int64) list,Errors list>> =
        EventStore.readEventsSpecificVersion getEventStore eventConverter aggregate id version 


    let private readAllUserStreams (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate : Task<Result<Stream<int64> list,Errors list>> =
        EventStore.readAllStreams getEventStore aggregate

    let private storeUserEvents getEventStore aggregate id events =
        let events =
            events
            |> List.map (fun i -> toDto i)
        EventStore.storeEvents getEventStore aggregate id events

    
    open Common.Infrastructure.EventStore

    type UserEventStore = EventStoreService<Event>
    

    let createUserEventStore (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate : UserEventStore =
        {
            AggregateName = aggregate
            StoreEvents = storeUserEvents getEventStore aggregate
            ReadEvents =  readEvents getEventStore aggregate
            ReadEventsStartSpecificVersion =  readEventsStartSpecificVersion getEventStore aggregate
            ReadAllStreams =  fun () -> readAllUserStreams getEventStore aggregate
        }


    

        
        
            
                    
                
        


         