namespace Project

module Dtos =

    module Events =

        open Common.Dtos  

        [<CLIMutable>]
        type ProjectCreated = 
            {
                ProjectId:string
                Title:string
                Description:string
                CreatedBy:string
            }   
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "ProjectCreated"


        [<CLIMutable>]
        type ProjectDeleted = 
            {
                ProjectId:string
                DeletedFrom:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "ProjectDeleted"


        [<CLIMutable>]
        type ProjectStateChanged = 
            {
                ProjectId:string
                State:string
            }  
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "ProjectStateChanged"   
            
        
        [<CLIMutable>]
        type ProjectTitleChanged = 
            {
                ProjectId:string
                Title:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "ProjectTitleChanged"


        [<CLIMutable>]
        type ProjectDescriptionChanged =
            {
                ProjectId:string
                Description:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "ProjectDescriptionChanged"


        [<CLIMutable>]
        type IssueCreated = 
            {
                ProjectId:string
                IssueId:string
                CreatedBy:string
                AssignTo:string
                Title:string
                Description:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "IssueCreated"


        [<CLIMutable>]
        type IssueDeleted = 
            {
                ProjectId:string
                IssueId:string
                DeletedFrom:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "IssueDeleted"


        [<CLIMutable>]
        type IssueStateChanged = 
            {
                ProjectId:string
                IssueId:string
                State:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "IssueStateChanged"

        
        [<CLIMutable>]
        type IssueAssignedToUser = 
            {
                ProjectId:string
                IssueId:string
                UserId:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "IssueAssignedToUser"


        [<CLIMutable>]
        type IssueTitleChanged = 
            {
                ProjectId:string
                IssueId:string
                Title:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "IssueTitleChanged"


        [<CLIMutable>]
        type IssueDescriptionChanged = 
            {
                ProjectId:string
                IssueId:string
                Description:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "IssueDescriptionChanged"


        [<CLIMutable>]
        type IssueCommentAdded = 
            {
                ProjectId:string
                IssueId:string
                CommentId:string
                Text:string
                CreatedBy:string                
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "IssueCommentAdded"


        [<CLIMutable>]
        type IssueCommentChanged = 
            {
                ProjectId:string
                IssueId:string
                CommentId:string
                Text:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "IssueCommentChanged"


        [<CLIMutable>]
        type IssueCommentDeleted = 
            {
                ProjectId:string
                IssueId:string
                CommentId:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "IssueCommentDeleted"


        [<CLIMutable>]
        type IssueAttachmentAdded = 
            {
                ProjectId:string
                IssueId:string
                AttachmentId:string
                Title:string
                FileName:string
                BlobReference:string
                CreatedBy:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "IssueAttachmentAdded"


        [<CLIMutable>]
        type IssueAttachmentRemoved = 
            {
                ProjectId:string
                IssueId:string
                AttachmentId:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "IssueAttachmentRemoved"



        open Common.Types
        open Project.Types
        open Project.Domain
        open Users.Types


        let toIssueStateDto state =
            match state with
            | IssueState.New -> "NEW"
            | IssueState.Active -> "ACTIVE"
            | IssueState.Done -> "DONE"
            | IssueState.OnHold -> "ONHOLD"


        let inline toIssueDto event : ^ev when ^ev : (member EventType:string) =
            match event with
            | IssueCreated e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    IssueId         = IssueId.value e.IssueId
                    CreatedBy       = UserId.value e.CreatedBy
                    AssignTo        = UserId.value e.AssignTo
                    Title           = NoneEmptyString.value e.Title
                    Description     = e.Description
                }  |> unbox
            | IssueDeleted e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    IssueId         = IssueId.value e.IssueId
                    DeletedFrom     = UserId.value e.DeletedFrom
                }  |> unbox
            | IssueStateChanged e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    IssueId         = IssueId.value e.IssueId
                    State           = toIssueStateDto e.State
                }  |> unbox
            | IssueAssignedToUser e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    IssueId         = IssueId.value e.IssueId
                    UserId          = UserId.value e.UserId
                }  |> unbox
            | IssueTitleChanged e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    IssueId         = IssueId.value e.IssueId
                    Title           = NoneEmptyString.value e.Title
                }  |> unbox
            | IssueDescriptionChanged e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    IssueId         = IssueId.value e.IssueId
                    Description     = e.Description
                }  |> unbox
            | IssueCommentAdded e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    IssueId         = IssueId.value e.IssueId
                    CommentId       = CommentId.value e.CommentId
                    Text            = NoneEmptyString.value e.Text
                    CreatedBy       = UserId.value e.CreatedBy 
                }  |> unbox
            | IssueCommentChanged e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    IssueId         = IssueId.value e.IssueId
                    CommentId       = CommentId.value e.CommentId
                    Text            = NoneEmptyString.value e.Text
                }  |> unbox
            | IssueCommentDeleted e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    IssueId         = IssueId.value e.IssueId
                    CommentId       = CommentId.value e.CommentId
                }  |> unbox
            | IssueAttachmentAdded e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    IssueId         = IssueId.value e.IssueId
                    AttachmentId    = AttachmentId.value e.AttachmentId
                    Title           = NoneEmptyString.value e.Title
                    FileName        = NoneEmptyString.value e.FileName
                    BlobReference   = NoneEmptyString.value e.BlobReference
                    CreatedBy       = UserId.value e.CreatedBy
                }  |> unbox
            | IssueAttachmentRemoved e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    IssueId         = IssueId.value e.IssueId
                    AttachmentId    = AttachmentId.value e.AttachmentId
                }  |> unbox


        let toProjectStateDto state =
            match state with
            | ProjectState.Active -> "ACTIVE"
            | ProjectState.Done -> "DONE"
            | ProjectState.Inactive -> "ONHOLD"

        let inline toDto event : ^ev when ^ev : (member EventType:string) =
            match event with
            | ProjectCreated e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    Title           = NoneEmptyString.value e.Title
                    Description     = e.Description
                    CreatedBy       = UserId.value e.CreatedBy
                }  |> unbox
            | ProjectDeleted e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    DeletedFrom     = UserId.value e.DeletedFrom
                }  |> unbox
            | ProjectStateChanged e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    State           = toProjectStateDto e.State
                }  |> unbox
            | ProjectTitleChanged e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    Title           = NoneEmptyString.value e.Title
                }  |> unbox
            | ProjectDescriptionChanged e ->
                {
                    ProjectId       = ProjectId.value e.ProjectId
                    Description     = e.Description
                }  |> unbox
            | IssueEvent e ->
                toIssueDto e



        let private toIssueStateDomain state =
            match state with
            | "NEW" -> IssueState.New
            | "ACTIVE" -> IssueState.Active
            | "DONE" -> IssueState.Done
            | "ONHOLD" -> IssueState.OnHold
            | _ -> failwith "invalid issue state in event dto"


        

        let private toIssueDomain (ev:IEvent) =
            match ev with
            | :? IssueCreated as e ->
                Issue.IssueCreated {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    IssueId         = IssueId.fromEventDto e.IssueId
                    CreatedBy       = UserId.fromEventDto e.CreatedBy
                    AssignTo        = UserId.fromEventDto e.AssignTo
                    Title           = NoneEmptyString.fromEventDto e.Title
                    Description     = e.Description
                }
            | :? IssueDeleted as e ->
                Issue.IssueDeleted {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    IssueId         = IssueId.fromEventDto e.IssueId
                    DeletedFrom     = UserId.fromEventDto e.DeletedFrom
                }
            | :? IssueStateChanged as e ->
                Issue.IssueStateChanged {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    IssueId         = IssueId.fromEventDto e.IssueId
                    State           = toIssueStateDomain e.State
                }
            | :? IssueAssignedToUser as e ->
                Issue.IssueAssignedToUser {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    IssueId         = IssueId.fromEventDto e.IssueId
                    UserId          = UserId.fromEventDto e.UserId
                }
            | :? IssueTitleChanged as e ->
                Issue.IssueTitleChanged {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    IssueId         = IssueId.fromEventDto e.IssueId
                    Title           = NoneEmptyString.fromEventDto e.Title
                }
            | :? IssueDescriptionChanged as e ->
                Issue.IssueDescriptionChanged {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    IssueId         = IssueId.fromEventDto e.IssueId
                    Description     = e.Description
                }
            | :? IssueCommentAdded as e ->
                Issue.IssueCommentAdded {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    IssueId         = IssueId.fromEventDto e.IssueId
                    CommentId       = CommentId.fromEventDto e.CommentId
                    Text            = NoneEmptyString.fromEventDto e.Text
                    CreatedBy       = UserId.fromEventDto e.CreatedBy 
                }
            | :? IssueCommentChanged as e ->
                Issue.IssueCommentChanged {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    IssueId         = IssueId.fromEventDto e.IssueId
                    CommentId       = CommentId.fromEventDto e.CommentId
                    Text            = NoneEmptyString.fromEventDto e.Text
                } 
            | :? IssueCommentDeleted as e ->
                Issue.IssueCommentDeleted {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    IssueId         = IssueId.fromEventDto e.IssueId
                    CommentId       = CommentId.fromEventDto e.CommentId
                } 
            | :? IssueAttachmentAdded as e ->
                Issue.IssueAttachmentAdded {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    IssueId         = IssueId.fromEventDto e.IssueId
                    AttachmentId    = AttachmentId.fromEventDto e.AttachmentId
                    Title           = NoneEmptyString.fromEventDto e.Title
                    FileName        = NoneEmptyString.fromEventDto e.FileName
                    BlobReference   = NoneEmptyString.fromEventDto e.BlobReference
                    CreatedBy       = UserId.fromEventDto e.CreatedBy
                }
            | :? IssueAttachmentRemoved as e ->
                Issue.IssueAttachmentRemoved {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    IssueId         = IssueId.fromEventDto e.IssueId
                    AttachmentId    = AttachmentId.fromEventDto e.AttachmentId
                }
            | _ ->
                let t = ev.GetType().Name
                failwith (sprintf "invalid type '%s' for an event dto" t)


        let private toProjectStateDomain state =
            match state with
            | "ACTIVE" -> ProjectState.Active
            | "DONE" -> ProjectState.Done
            | "ONHOLD" -> ProjectState.Inactive
            | _ -> failwith "invalid project state in event dto"


        let toDomain (ev:IEvent) =
            match ev with
            | :? ProjectCreated as e ->
                Domain.ProjectCreated {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    Title           = NoneEmptyString.fromEventDto e.Title
                    Description     = e.Description
                    CreatedBy       = UserId.fromEventDto e.CreatedBy
                }
            | :? ProjectDeleted as e ->
                Domain.ProjectDeleted {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    DeletedFrom     = UserId.fromEventDto e.DeletedFrom
                } 
            | :? ProjectStateChanged as e ->
                Domain.ProjectStateChanged {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    State           = toProjectStateDomain e.State
                }  
            | :? ProjectTitleChanged as e ->
                Domain.ProjectTitleChanged {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    Title           = NoneEmptyString.fromEventDto e.Title
                }
            | :? ProjectDescriptionChanged as e ->
                Domain.ProjectDescriptionChanged {
                    ProjectId       = ProjectId.fromEventDto e.ProjectId
                    Description     = e.Description
                }
            | _ as e ->
                Domain.IssueEvent (toIssueDomain e)


        
