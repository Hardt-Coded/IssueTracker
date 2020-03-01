namespace Project

module EventStore =
    
    open Newtonsoft.Json.Linq
    open CosmoStore
    open System.Threading.Tasks
    open Common.Infrastructure
    open Common.Dtos
    open Common.Domain
    open Project.Dtos.Events
    open Project.Domain


    let private eventConverter (event:EventRead<JToken,int64>) : ProjectEvent * int64 =
        match event.Name with

        | x when x = nameof IssueCreated ->
            let e = event.Data.ToObject<IssueCreated>()
            e |> toDomain, event.Version
        | x when x = nameof IssueDeleted ->
            let e = event.Data.ToObject<IssueDeleted>()
            e |> toDomain, event.Version
        | x when x = nameof IssueStateChanged ->
            let e = event.Data.ToObject<IssueStateChanged>()
            e |> toDomain, event.Version
        | x when x = nameof IssueAssignedToUser ->
            let e = event.Data.ToObject<IssueAssignedToUser>()
            e |> toDomain, event.Version
        | x when x = nameof IssueTitleChanged ->
            let e = event.Data.ToObject<IssueTitleChanged>()
            e |> toDomain, event.Version
        | x when x = nameof IssueDescriptionChanged ->
            let e = event.Data.ToObject<IssueDescriptionChanged>()
            e |> toDomain, event.Version
        | x when x = nameof IssueCommentAdded ->
            let e = event.Data.ToObject<IssueCommentAdded>()
            e |> toDomain, event.Version
        | x when x = nameof IssueCommentChanged ->
            let e = event.Data.ToObject<IssueCommentChanged>()
            e |> toDomain, event.Version
        | x when x = nameof IssueCommentDeleted ->
            let e = event.Data.ToObject<IssueCommentDeleted>()
            e |> toDomain, event.Version
        | x when x = nameof IssueAttachmentAdded ->
            let e = event.Data.ToObject<IssueAttachmentAdded>()
            e |> toDomain, event.Version
        | x when x = nameof IssueAttachmentRemoved ->
            let e = event.Data.ToObject<IssueAttachmentRemoved>()
            e |> toDomain, event.Version
        | x when x = nameof ProjectCreated ->
            let e = event.Data.ToObject<ProjectCreated>()
            e |> toDomain, event.Version
        | x when x = nameof ProjectDeleted ->
            let e = event.Data.ToObject<ProjectDeleted>()
            e |> toDomain, event.Version
        | x when x = nameof ProjectStateChanged ->
            let e = event.Data.ToObject<ProjectStateChanged>()
            e |> toDomain, event.Version
        | x when x = nameof ProjectTitleChanged ->
            let e = event.Data.ToObject<ProjectTitleChanged>()
            e |> toDomain, event.Version
        | x when x = nameof ProjectDescriptionChanged ->
            let e = event.Data.ToObject<ProjectDescriptionChanged>()
            e |> toDomain, event.Version
        | _ ->
            failwith "can not convert event"
        


    let private readEvents (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate id : Task<Result<(ProjectEvent * int64) list,Errors list>> =
        EventStore.readAllEvents getEventStore eventConverter aggregate id


    let private readEventsStartSpecificVersion (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate id version : Task<Result<(ProjectEvent * int64) list,Errors list>> =
        EventStore.readEventsSpecificVersion getEventStore eventConverter aggregate id version 


    let private readAllUserStreams (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate : Task<Result<Stream<int64> list,Errors list>> =
        EventStore.readAllStreams getEventStore aggregate


    let private storeProjectEvents getEventStore aggregate id events =
        let events =
            events
            |> List.map (fun i -> toDto i)
        EventStore.storeEvents getEventStore aggregate id events



    
    open Common.Infrastructure.EventStore

    type ProjectEventStore = EventStoreService<ProjectEvent>

    let createUserEventStore (getEventStore:unit -> Task<EventStore<JToken,int64>>) aggregate : ProjectEventStore =
        {
            AggregateName = aggregate
            StoreEvents = storeProjectEvents getEventStore aggregate
            ReadEvents =  readEvents getEventStore aggregate
            ReadEventsStartSpecificVersion =  readEventsStartSpecificVersion getEventStore aggregate
            ReadAllStreams =  fun () -> readAllUserStreams getEventStore aggregate
        }