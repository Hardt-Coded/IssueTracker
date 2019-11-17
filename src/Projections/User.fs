namespace Projections

open Dtos.User.Events

module UserList =


    open Infrastructure
    open Domain.Types.Common
    open Domain.Types.User
        

    type State = {
        UserId:string
        Name:string
        EMail:string
        Groups: string list
        Version:int64
    }



    type private Messages =
        | UpdateProjection
        | GetUserList of AsyncReplyChannel<State list>



    let private entryProjector (state:State option) (events:(Domain.User.Event * int64) list) =
        let applyEvent state (event:Domain.User.Event) (version:int64) =
            match state,event with
            | None, Domain.User.Event.UserCreated ev ->
                {
                    UserId = ev.UserId |> UserId.value
                    Name= ev.Name |> NotEmptyString.value
                    EMail=ev.EMail |> EMail.value
                    Groups=[]
                    Version = version
                } |> Some
            | Some _, Domain.User.Event.UserDeleted ev ->
                None
            | Some state, Domain.User.Event.EMailChanged ev ->
                {
                    state with 
                        EMail = EMail.value ev.EMail
                        Version = version
                } |> Some
            | Some state, Domain.User.Event.PasswordChanged ev ->
                state |> Some
            | Some state, Domain.User.Event.AddedToGroup ev ->
                let newGroup = ev.Group |> NotEmptyString.value
                {
                    state with 
                        Groups = newGroup :: state.Groups
                        Version = version
                } |> Some
            | Some state, Domain.User.Event.RemovedFromGroup ev ->
                let group = ev.Group |> NotEmptyString.value
                {
                    state with 
                        Groups = state.Groups |> List.filter (fun i -> i<>group)
                        Version = version
                } |> Some
            | _, _ ->
                state

        (state,events)
        ||> List.fold (fun state (event,version) -> applyEvent state event version)


    let private projector userList newItemEvents updateEvents =

        let newUsers =
            newItemEvents
            |> List.map (fun (_,events) -> entryProjector None events)
            |> List.choose (id)
        
        let updatedUserList =
            userList
            |> List.choose (fun currentState ->
                let events = 
                    updateEvents 
                    |> List.tryFind (fun (id,events:(Domain.User.Event * int64) list) -> id = currentState.UserId)
                    
                match events with
                | None ->
                    Some currentState
                | Some (_,events) ->
                    let newState = 
                        entryProjector (Some currentState) events
                    newState
            )

        updatedUserList @ newUsers
        


    type UserListProjection(handleError, firstInitSucceed) =
        let mailBox = 
            MailboxProcessor.Start(fun inbox ->
                async {

                    let refreshUsers (state:State list) =
                        async {
                            
                            let! streams = EventStore.User.readAllUserStreams Services.Common.getEventStore Services.User.aggregateName |> Async.AwaitTask
                            match streams with
                            | Error e ->
                                // todo: log errors away
                                e |> handleError 
                                return state
                            | Ok streams ->
                                let currentIds =
                                    state |> List.map (fun i -> i.UserId)

                                let currentIdsAndVersion =
                                    state |> List.map (fun i -> i.UserId,i.Version)

                                let aggregatePrefix = sprintf "%s-" Services.User.aggregateName

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
                                    |> List.map (fun (id,_,stateVersion) -> id,stateVersion)

                                let runParallel max items = Async.Parallel (items,max)

                                let! newIdsEvents = 
                                    newIds
                                    |> List.map (fun id ->
                                        async {
                                            printf "."
                                            let! events = EventStore.User.readEvents Services.Common.getEventStore Services.User.aggregateName id |> Async.AwaitTask
                                            return id,events
                                        }
                                    )
                                    |> runParallel 8

                                let! changedIdsEvents =
                                    changedIdsAndLastVersion
                                    |> List.map ( fun (id,version) ->
                                        async {
                                            printf "."
                                            let! events = EventStore.User.readEventsStartSpecificVersion Services.Common.getEventStore Services.User.aggregateName id version |> Async.AwaitTask
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
                                    
                                

                                return newState
                        }


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


                    let! currentState = refreshUsers []
                    firstInitSucceed()
                    
                    return! agentLoop currentState
                }
            )

        member __.UpdateProjection () =
            mailBox.Post UpdateProjection

        member __.GetUserList () =
            async {
                let! result = mailBox.PostAndAsyncReply GetUserList
                return result
            } |> Async.StartAsTask

    
        
        
