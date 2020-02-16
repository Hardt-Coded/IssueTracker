namespace Project.Projections



module ProjectList =

    
    open Common.Types
    open Project.Types
    open Project.Domain    
    open Project.EventStore
    open System
        


    type Comment = {
        CommentId:string
        Text:string
        CreatedBy:string
        CreatedOn:DateTime
    }

    type Attachment = {
        AttachmentId:string
        Title:string
        FileName:string
        BlobReference:string
        CreatedBy:string
        CreatedOn:DateTime
    }

    type Issue = {
        IssueId:string
        Title:string
        Description:string
        State:string
        AssignedTo:string
        CreatedBy:string
        CreatedOn:DateTime
        Comments: Comment list
        Attachments:Attachment list
    }

    type Project = {
        ProjectId:string
        Title:string
        Description:string
        State:string
        Issues:Issue list
        Version:int64
    }



    type private Messages =
        | UpdateProjection
        | GetProjectList of AsyncReplyChannel<Project list>
    
    
    [<AutoOpen>]
    module private Helpers =

        open Users.Types

        let mapStateToProjection (state:Project.Domain.Project) : Project =
            let mapAttachment (state:Project.Domain.Issue.Attachment) : Attachment =
                {
                    AttachmentId = AttachmentId.value state.AttachmentId
                    Title = NoneEmptyString.value state.Title
                    FileName = NoneEmptyString.value state.FileName
                    BlobReference = NoneEmptyString.value state.BlobReference
                    CreatedBy = UserId.value state.CreatedBy
                    CreatedOn = state.CreatedOn
                }
                
            let mapComment (state:Project.Domain.Issue.Comment) : Comment =
                {
                    CommentId = CommentId.value state.CommentId
                    Text = NoneEmptyString.value state.Text
                    CreatedBy = UserId.value state.CreatedBy
                    CreatedOn = state.CreatedOn
                }


            let mapIssue (state:Project.Domain.Issue.Issue) : Issue =
                {
                    IssueId = IssueId.value state.IssueId
                    Title = NoneEmptyString.value state.Title
                    Description = state.Description
                    State = 
                        match state.State with
                        | Project.Domain.Issue.IssueState.New -> "NEW"
                        | Project.Domain.Issue.IssueState.Active -> "ACTIVE"
                        | Project.Domain.Issue.IssueState.Done -> "DONE"
                        | Project.Domain.Issue.IssueState.OnHold -> "ONHOLD"
                    AssignedTo = UserId.value state.AssignedTo
                    CreatedBy = UserId.value state.CreatedBy
                    CreatedOn = state.CreatedOn
                    Comments = state.Comments |> List.map mapComment
                    Attachments = state.Attachments |> List.map mapAttachment
                
                }
            
            {
                ProjectId = ProjectId.value state.ProjectId
                Title = NoneEmptyString.value state.Title
                Description = state.Description
                State = 
                    match state.State with            
                    | Project.Domain.ProjectState.Active -> "ACTIVE"
                    | Project.Domain.ProjectState.Done -> "DONE"
                    | Project.Domain.ProjectState.Inactive -> "ONHOLD"
                Issues = state.Issues |> List.map mapIssue
                Version = -1L
            }
        

        let mapStateToDomain (state:Project) : Project.Domain.Project =
            let mapAttachment (state:Attachment) : Project.Domain.Issue.Attachment =
                {
                    AttachmentId = AttachmentId.fromEventDto state.AttachmentId
                    Title = NoneEmptyString.fromEventDto state.Title
                    FileName = NoneEmptyString.fromEventDto state.FileName
                    BlobReference = NoneEmptyString.fromEventDto state.BlobReference
                    CreatedBy = UserId.fromEventDto state.CreatedBy
                    CreatedOn = state.CreatedOn
                }
                
            let mapComment (state:Comment) : Project.Domain.Issue.Comment =
                {
                    CommentId = CommentId.fromEventDto state.CommentId
                    Text = NoneEmptyString.fromEventDto state.Text
                    CreatedBy = UserId.fromEventDto state.CreatedBy
                    CreatedOn = state.CreatedOn
                }


            let mapIssue (state:Issue) : Project.Domain.Issue.Issue =
                {
                    IssueId = IssueId.fromEventDto state.IssueId
                    Title = NoneEmptyString.fromEventDto state.Title
                    Description = state.Description
                    State = 
                        match state.State.ToUpper() with
                        | "NEW" -> Project.Domain.Issue.IssueState.New
                        | "ACTIVE" -> Project.Domain.Issue.IssueState.Active
                        | "DONE" -> Project.Domain.Issue.IssueState.Done
                        | "ONHOLD" -> Project.Domain.Issue.IssueState.OnHold
                        | _ -> failwith "invalid issue state in projection"

                    AssignedTo = UserId.fromEventDto state.AssignedTo
                    CreatedBy = UserId.fromEventDto state.CreatedBy
                    CreatedOn = state.CreatedOn
                    Comments = state.Comments |> List.map mapComment
                    Attachments = state.Attachments |> List.map mapAttachment
                
                }
            
            {
                ProjectId = ProjectId.fromEventDto state.ProjectId
                Title = NoneEmptyString.fromEventDto state.Title
                Description = state.Description
                State = 
                    match state.State.ToUpper() with            
                    | "ACTIVE" -> Project.Domain.ProjectState.Active
                    | "DONE" -> Project.Domain.ProjectState.Done
                    | "ONHOLD" -> Project.Domain.ProjectState.Inactive
                    | _ -> failwith  "invalid projct state in projection"
                Issues = state.Issues |> List.map mapIssue
            }


        
        let entryProjector (state:Project option) (events:(ProjectEvent * int64) list) =
            
            let domainState:Project.Domain.Project option = 
                state
                |> Option.map mapStateToDomain

            let domainRes =
                (domainState,events)
                ||> List.fold (fun state (event,version) -> Project.Domain.apply state event)

            domainRes 
            |> Option.map mapStateToProjection 
            |> Option.map (fun p -> 
                let (_,v) = events |> List.maxBy snd
                {
                    p with Version = v  
                }
            )


        let projector projectList newItemEvents updateEvents =

            let newUsers =
                newItemEvents
                |> List.map (fun (_,events) -> entryProjector None events)
                |> List.choose (id)
            
            let updatedUserList =
                projectList
                |> List.choose (fun currentState ->
                    let events = 
                        updateEvents 
                        |> List.tryFind (fun (id,events:(ProjectEvent * int64) list) -> id = currentState.ProjectId)
                        
                    match events with
                    | None ->
                        Some currentState
                    | Some (_,events) ->
                        let newState = 
                            entryProjector (Some currentState) events
                        newState
                )

            updatedUserList @ newUsers


        


    type UserListProjection(userEventStore:ProjectEventStore,
        handleError,
        loadProjection,
        storeProjection
        ) =

        let refreshProjects (state:Project list) =
            async {
                
                let! streams = userEventStore.ReadAllUserStreams () |> Async.AwaitTask
                match streams with
                | Error e ->
                    e |> handleError 
                    return state
                | Ok streams ->
                    let currentIds =
                        state |> List.map (fun i -> i.ProjectId)

                    let currentIdsAndVersion =
                        state |> List.map (fun i -> i.ProjectId,i.Version)

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
        let initState = refreshProjects stateFromDisk |> Async.RunSynchronously


        let mailBox = 
            MailboxProcessor.Start(fun inbox ->
                async {
                    let rec agentLoop state =
                        async {
                        
                            let! msg = inbox.TryReceive 3000
                            match msg with
                            | None ->
                                let! newState = refreshProjects state
                                return! agentLoop newState
                            | Some msg ->
                                match msg with
                                | UpdateProjection ->
                                    let! newState = refreshProjects state
                                    return! agentLoop newState
                                | GetProjectList reply ->
                                    reply.Reply state
                                    return! agentLoop state
                        }
                    
                    return! agentLoop initState
                }
            )

        member __.UpdateProjection () =
            mailBox.Post UpdateProjection

        member __.GetProjectList () =
            async {
                let! result = mailBox.PostAndAsyncReply GetProjectList
                return result
            } |> Async.StartAsTask


    

    
        
        
