namespace Users.Projections



module UserList =

    
    open Common.Types
    open Users.Types
    open Users.Domain    
    open Users.EventStore
        

    type State = {
        UserId:string
        Name:string
        EMail:string
        PasswordHash:string
        PasswordSalt:string
        Groups: string list
        Version:int64
    }



    type private Messages =
        | UpdateProjection
        | GetUserList of AsyncReplyChannel<State list>
    
    
    [<AutoOpen>]
    module private Helpers =
        
        let entryProjector (state:State option) (events:(Event * int64) list) =
            let applyEvent state (event:Event) (version:int64) =
                match state,event with
                | None, UserCreated ev ->
                    let hPair = PasswordHash.value ev.PasswordHash
                    {
                        UserId = ev.UserId |> UserId.value
                        Name= ev.Name |> NoneEmptyString.value
                        EMail=ev.EMail |> EMail.value
                        PasswordHash = hPair.Hash
                        PasswordSalt = hPair.Salt
                        Groups=[]
                        Version = version
                    } |> Some
                | Some _, UserDeleted ev ->
                    None
                | Some state, EMailChanged ev ->
                    {
                        state with 
                            EMail = EMail.value ev.EMail
                            Version = version
                    } |> Some
                | Some state, NameChanged ev ->
                    {
                        state with 
                            Name = NoneEmptyString.value ev.Name
                            Version = version
                    } |> Some
                | Some state, PasswordChanged ev ->
                    let hPair = PasswordHash.value ev.PasswordHash
                    {
                        state with 
                            PasswordHash = hPair.Hash
                            PasswordSalt = hPair.Salt
                            Version = version
                    } |> Some
                | Some state, AddedToGroup ev ->
                    let newGroup = ev.Group |> NoneEmptyString.value
                    {
                        state with 
                            Groups = newGroup :: state.Groups
                            Version = version
                    } |> Some
                | Some state, RemovedFromGroup ev ->
                    let group = ev.Group |> NoneEmptyString.value
                    {
                        state with 
                            Groups = state.Groups |> List.filter (fun i -> i<>group)
                            Version = version
                    } |> Some
                | _, _ ->
                    state

            (state,events)
            ||> List.fold (fun state (event,version) -> applyEvent state event version)


        let projector userList newItemEvents updateEvents =

            let newUsers =
                newItemEvents
                |> List.map (fun (_,events) -> entryProjector None events)
                |> List.choose (id)
            
            let updatedUserList =
                userList
                |> List.choose (fun currentState ->
                    let events = 
                        updateEvents 
                        |> List.tryFind (fun (id,events:(Event * int64) list) -> id = currentState.UserId)
                        
                    match events with
                    | None ->
                        Some currentState
                    | Some (_,events) ->
                        let newState = 
                            entryProjector (Some currentState) events
                        newState
                )

            updatedUserList @ newUsers


        


    type UserListProjection(userEventStore:UserEventStore,
        handleError,
        loadProjection,
        storeProjection
        ) =

        let refreshUsers (state:State list) =
            async {
                
                let! streams = userEventStore.ReadAllUserStreams () |> Async.AwaitTask
                match streams with
                | Error e ->
                    e |> handleError 
                    return state
                | Ok streams ->
                    let currentIds =
                        state |> List.map (fun i -> i.UserId)

                    let currentIdsAndVersion =
                        state |> List.map (fun i -> i.UserId,i.Version)

                    let aggregatePrefix = sprintf "%s-" userEventStore.AggregateName
                    
                    let newIds =
                        streams 
                        |> List.map (fun i -> i.Id.Replace(aggregatePrefix,""))
                        |> List.except (currentIds)

                    let changedIdsAndLastVersion =
                        streams 
                        |> List.choose (fun i ->
                            let streamId = i.Id.Replace(aggregatePrefix,"")
                            let stateItem = currentIdsAndVersion |> List.tryFind (fun (id,version) -> id = streamId)
                            stateItem
                            |> Option.map (fun (_,stateVersion) ->
                                streamId,
                                i.LastVersion,
                                stateVersion
                            )
                        )
                        |> List.filter (fun (_,streamVersion,stateVersion) -> streamVersion <> stateVersion)
                        |> List.map (fun (id,streamVersion,stateVersion) -> id,stateVersion)

                    let runParallel max items = Async.Parallel (items,max)

                    let! newIdsEvents = 
                        newIds
                        |> List.map (fun id ->
                            async {
                                printf "."
                                let! events = userEventStore.ReadEvents id |> Async.AwaitTask
                                return id,events
                            }
                        )
                        |> runParallel 8

                    let! changedIdsEvents =
                        changedIdsAndLastVersion
                        |> List.map ( fun (id,version) ->
                            async {
                                let nextVersion = version + 1L
                                let! events = userEventStore.ReadEventsStartSpecificVersion id nextVersion |> Async.AwaitTask
                                return id,events
                            }
                        )
                        |> runParallel 8

                    let toProcessNewItemEvents = 
                        (newIdsEvents |> List.ofArray)
                        |> List.choose (fun (id,item) ->
                            match item with
                            | Error e ->
                                handleError (e)
                                None
                            | Ok event ->
                                Some (id,event)
                        )

                    let toProcessUpdateEvents = 
                        (changedIdsEvents |> List.ofArray)
                        |> List.choose (fun (id,item) ->
                            match item with
                            | Error e ->
                                handleError (e)
                                None
                            | Ok event ->
                                Some (id,event)
                        )

                    printfn " - done"

                    let newState = projector state toProcessNewItemEvents toProcessUpdateEvents
                        
                    // store state in file
                    do! storeProjection newState

                    return newState
            }
        
        // need to run snychonously, to have all in place when the object is initialized
        let stateFromDisk = loadProjection () |> Async.RunSynchronously
        // first init
        let initState = refreshUsers stateFromDisk |> Async.RunSynchronously


        let mailBox = 
            MailboxProcessor.Start(fun inbox ->
                async {
                    let rec agentLoop state =
                        async {
                        
                            let! msg = inbox.TryReceive 3000
                            match msg with
                            | None ->
                                let! newState = refreshUsers state
                                return! agentLoop newState
                            | Some msg ->
                                match msg with
                                | UpdateProjection ->
                                    let! newState = refreshUsers state
                                    return! agentLoop newState
                                | GetUserList reply ->
                                    reply.Reply state
                                    return! agentLoop state
                        }
                    
                    return! agentLoop initState
                }
            )

        member __.UpdateProjection () =
            mailBox.Post UpdateProjection

        member __.GetUserList () =
            async {
                let! result = mailBox.PostAndAsyncReply GetUserList
                return result
            } |> Async.StartAsTask


    open System
    open FSharp.Control.Tasks.V2

    let getUserByEmail (userListProjection:UserListProjection) (email:string) =
        task {
            let! users = userListProjection.GetUserList ()
            return 
                users
                |> List.tryFind (fun i -> i.EMail.ToUpper() = email.ToUpper())
        }

    
        
        
