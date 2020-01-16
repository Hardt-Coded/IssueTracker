namespace Project

module Domain =
    
    open System
    open Types
    open Common.Types
    open Users.Types
    open Common.Domain

    [<AutoOpen>]
    module Issue =

        type IssueState =
            | New
            | Active
            | Done
            | OnHold


        type Comment = {
            CommentId:CommentId
            Text:NoneEmptyString
            CreatedBy:UserId
            CreatedOn:DateTime
        }

        type Attachment = {
            AtachemntId:AttachmentId
            Title:NoneEmptyString
            FileName:NoneEmptyString
            BlobReference:string
            CreatedBy:UserId
            CreatedOn:DateTime
        }

        type Issue = {
            IssueId:IssueId
            Title:NoneEmptyString
            Description:string
            State:IssueState
            AssignedTo:UserId
            CreatedBy:UserId
            CreatedOn:DateTime
            Comments: Comment list
            Attachments:Attachment list
        }

        module CommandArguments =
            
            type CreateIssue = {
                IssueId:string
                CreateBy:string
                AssignTo:string
                Title:string
                Description:string
            }

            type DeleteIssue = {
                IssueId:string
                DeletedFrom:string
            }

            type ChangeIssueState = {
                IssueId:string
                State:string
            }
            
            type AssignIssueToUser = {
                IssueId:string
                UserId:string
            }

            type ChangeIssueTitle = {
                IssueId:string
                Title:string
            }

            type ChangeIssueDescription = {
                IssueId:string
                Description:string
            }

            type AddIssueComment = {
                IssueId:string
                CommentId:string
                Text:string
                CreatedBy:string                
            }

            type ChangeIssueComment = {
                IssueId:string
                CommentId:string
                Text:string
            }

            type DeleteIssueComment = {
                IssueId:string
                CommentId:string
            }

            type AddIssueAttachment = {
                IssueId:string
                AttachmentId:string
                Title:string
                FileName:string
                BlobReference:string
                CreatedBy:string

            }

            type RemoveIssueAttachment = {
                IssueId:string
                AttachmentId:string
            }


        type IssueCommand =
            | CreateIssue of CommandArguments.CreateIssue
            | DeleteIssue of CommandArguments.DeleteIssue
            | ChangeIssueState of CommandArguments.ChangeIssueState
            | AssignIssueToUser of CommandArguments.AssignIssueToUser
            | ChangeIssueTitle of CommandArguments.ChangeIssueTitle
            | ChangeIssueDescription of CommandArguments.ChangeIssueDescription
            | AddIssueComment of CommandArguments.AddIssueComment
            | ChangeIssueComment of CommandArguments.ChangeIssueComment
            | DeleteIssueComment of CommandArguments.DeleteIssueComment
            | AddIssueAttachment of CommandArguments.AddIssueAttachment
            | RemoveIssueAttachment of CommandArguments.RemoveIssueAttachment


        module EventArguments =
            
            type IssueCreated = {
                IssueId:IssueId
                CreatedBy:UserId
                AssignTo:UserId
                Title:NoneEmptyString
                Description:string
            }

            type IssueDeleted = {
                IssueId:IssueId
                DeletedFrom:UserId
            }

            type IssueStateChanged = {
                IssueId:IssueId
                State:IssueState
            }
            
            type IssueAssignedToUser = {
                IssueId:IssueId
                UserId:UserId
            }

            type IssueTitleChanged = {
                IssueId:IssueId
                Title:NoneEmptyString
            }

            type IssueDescriptionChanged = {
                IssueId:IssueId
                Description:string
            }

            type IssueCommentAdded = {
                IssueId:IssueId
                CommentId:CommentId
                Text:NoneEmptyString
                CreatedBy:UserId                
            }

            type IssueCommentChanged = {
                IssueId:IssueId
                CommentId:CommentId
                Text:NoneEmptyString
            }

            type IssueCommentDeleted = {
                IssueId:IssueId
                CommentId:CommentId
            }

            type IssueAttachmentAdded = {
                IssueId:IssueId
                AttachmentId:AttachmentId
                Title:NoneEmptyString
                FileName:NoneEmptyString
                BlobReference:NoneEmptyString
                CreatedBy:UserId

            }

            type IssueAttachmentRemoved = {
                IssueId:IssueId
                AttachmentId:AttachmentId
            }


        type IssueEvent =
            | IssueCreated of EventArguments.IssueCreated
            | IssueDeleted of EventArguments.IssueDeleted
            | IssueStateChanged of EventArguments.IssueStateChanged
            | IssueAssignedToUser of EventArguments.IssueAssignedToUser
            | IssueTitleChanged of EventArguments.IssueTitleChanged
            | IssueDescriptionChanged of EventArguments.IssueDescriptionChanged
            | IssueCommentAdded of EventArguments.IssueCommentAdded
            | IssueCommentChanged of EventArguments.IssueCommentChanged
            | IssueCommentDeleted of EventArguments.IssueCommentDeleted
            | IssueAttachmentAdded of EventArguments.IssueAttachmentAdded
            | IssueAttachmentRemoved of EventArguments.IssueAttachmentRemoved


    type ProjectState =
        | Active
        | Done
        | Inactive


    type Project = {
        ProjectId:ProjectId
        Title:NoneEmptyString
        Desciption:string
        State:ProjectState
        Issues: Issue list
    }

    module CommandArguments =
        
        type CreateProject = {
            ProjectId:string
            Title:string
            Description:String
            CreatedBy:string
        }

        type DeleteProject = {
            ProjectId:string
        }


        type ChangeProjectState = {
            IssueId:string
            State:string
        }


        type ChangeProjectTitle = {
            IssueId:string
            Title:string
        }

        type ChangeProjectDescription = {
            IssueId:string
            Description:string
        }


    type ProjectCommand =
        | CreateProject of CommandArguments.CreateProject
        | DeleteProject of CommandArguments.DeleteProject
        | ChangeProjectState of CommandArguments.ChangeProjectState
        | ChangeProjectTitle of CommandArguments.ChangeProjectTitle
        | ChangeProjectDescription of CommandArguments.ChangeProjectDescription
        | IssueCommand of IssueCommand


    module EventArguments =
        
        type ProjectCreated = {
            ProjectId:ProjectId
            CreateUserId:UserId
            AssignTo:UserId
            Title:NoneEmptyString
            Description:string
        }

        type ProjectDeleted = {
            ProjectId:ProjectId
            DeletedFrom:UserId
        }

        type ProjectStateChanged = {
            ProjectId:ProjectId
            State:ProjectState
        }        
        

        type ProjectTitleChanged = {
            ProjectId:ProjectId
            Title:NoneEmptyString
        }

        type ProjectDescriptionChanged = {
            ProjectId:ProjectId
            Description:string
        }


    type ProjectEvent =
        | ProjectCreated of EventArguments.ProjectCreated
        | ProjectDeleted of EventArguments.ProjectDeleted
        | ProjectStateChanged of EventArguments.ProjectStateChanged
        | ProjectTitleChanged of EventArguments.ProjectTitleChanged
        | ProjectDescriptionChanged of EventArguments.ProjectDescriptionChanged
        | IssureEvent of IssueEvent


    let (<*>) = Validation.apply 
    let (<!>) = Result.map



    let rec private handleIssueCommand (state:Issue option) (command:IssueCommand) : Result<IssueEvent list,Errors list> =
        match state, command with
        | None, CreateIssue args ->
            createIssue args
        | Some _, CreateIssue _ ->
            "you can not have a create issue event, when a issue already exists"
            |> DomainError
            |> List.singleton
            |> Error
        | Some state, DeleteIssue args ->
            deleteIssue args
        | Some state, ChangeIssueState args ->
            changeState args
        | Some state, AssignIssueToUser args ->
            assignToUser args
        | Some state, ChangeIssueTitle args ->
            changeTitle args
        | Some state, ChangeIssueDescription args ->
            changeDescription args
        | Some state, AddIssueComment args ->
            addComment args
        | Some state, ChangeIssueComment args ->
            changeComment args
        | Some state, DeleteIssueComment args ->
            deleteComment args
        | Some state, AddIssueAttachment args ->
            addAttachment args
        | Some state, RemoveIssueAttachment args ->
            removeAttachment args
        | None, _ ->
            "invalid command, issue does not exists"
            |> DomainError
            |> List.singleton
            |> Error


    and createIssue args =
        let create issueId createdBy assignTo title description =
            IssueCreated {
                IssueId=issueId
                CreatedBy=createdBy
                AssignTo=assignTo
                Title=title
                Description=description
            }
            |> List.singleton

        let issueId = IssueId.create args.IssueId
        let createdBy = UserId.create args.CreateBy
        let assignTo = UserId.create args.AssignTo
        let title = NoneEmptyString.create "Issue Title" args.Title
        let description = Ok args.Description
        create <!> issueId <*> createdBy <*> assignTo <*> title <*> description


    and deleteIssue args =
        let create issueId deletedFrom = 
            IssueDeleted {
                IssueId=issueId
                DeletedFrom=deletedFrom
            }
            |> List.singleton

        let issueId = IssueId.create args.IssueId
        let deletedFrom = UserId.create args.DeletedFrom
        create <!> issueId <*> deletedFrom


    and changeState args =
        let create issueId state = 
            IssueStateChanged {
                IssueId=issueId
                State=state
            }
            |> List.singleton

        let issueId = IssueId.create args.IssueId
        let state = 
            match args.State.ToUpper() with
            | "NEW" -> Ok IssueState.New
            | "ACTIVE" -> Ok IssueState.Active
            | "DONE" -> Ok IssueState.Done
            | "ONHOLD" -> Ok IssueState.OnHold
            | _ -> 
                "invalid issue state"
                |> DomainError
                |> List.singleton
                |> Error

        create <!> issueId <*> state


    and assignToUser args =
        let create issueId userId = 
            IssueAssignedToUser {
                IssueId=issueId
                UserId=userId
            }
            |> List.singleton

        let issueId = IssueId.create args.IssueId
        let userId = UserId.create args.UserId
        create <!> issueId <*> userId

        
    and changeTitle args =
        let create issueId title =
            IssueTitleChanged {
                IssueId=issueId
                Title = title
            }
            |> List.singleton

        let issueId = IssueId.create args.IssueId
        let title = NoneEmptyString.create "Issue Title" args.Title
        create <!> issueId <*> title

        
    and changeDescription args =
        let create issueId description =
            IssueDescriptionChanged {
                IssueId=issueId
                Description=description
            }
            |> List.singleton

        let issueId = IssueId.create args.IssueId
        let description = Ok args.Description
        create <!> issueId <*> description
        

    and addComment args =
        let create issueId commentId text createdBy =
            IssueCommentAdded {
                IssueId=issueId
                CommentId=commentId
                Text=text
                CreatedBy=createdBy                
            }
            |> List.singleton

        let issueId = IssueId.create args.IssueId
        let commentId = CommentId.create args.CommentId
        let text = NoneEmptyString.create "Comment Text" args.Text
        let createdBy = UserId.create args.CreatedBy
        create <!> issueId <*> commentId <*> text <*> createdBy


    and changeComment args =
        let create issueId commentId text =
            IssueCommentChanged {
                IssueId=issueId
                CommentId=commentId
                Text=text
            }
            |> List.singleton

        let issueId = IssueId.create args.IssueId
        let commentId = CommentId.create args.CommentId
        let text = NoneEmptyString.create "Comment Text" args.Text
        create <!> issueId <*> commentId <*> text

        
    and deleteComment args =
        let create issueId commentId =
            IssueCommentDeleted{
                IssueId=issueId
                CommentId=commentId
            }
            |> List.singleton

        let issueId = IssueId.create args.IssueId
        let commentId = CommentId.create args.CommentId
        create <!> issueId <*> commentId

        
    and addAttachment args =
        let create issueId attachmentId title fileName blobReference createdBy =
            IssueAttachmentAdded {
                IssueId=issueId
                AttachmentId=attachmentId
                Title=title
                FileName=fileName
                BlobReference=blobReference
                CreatedBy=createdBy
            }
            |> List.singleton

        let issueId = IssueId.create args.IssueId
        let attachmentId = AttachmentId.create args.AttachmentId
        let title = NoneEmptyString.create "Attachment Title" args.Title
        let fileName = NoneEmptyString.create "Filename" args.FileName
        let blobReference = NoneEmptyString.create "Blob Reference" args.BlobReference
        let createdBy = UserId.create args.CreatedBy
        create <!> issueId <*> attachmentId <*> title <*> fileName <*> blobReference <*> createdBy
        
    and removeAttachment args =
        let create issueId attachmentId =
            IssueAttachmentRemoved {
                IssueId=issueId
                AttachmentId=attachmentId
            }
            |> List.singleton

        let issueId = IssueId.create args.IssueId
        let attachmentId = AttachmentId.create args.AttachmentId
        create <!> issueId <*> attachmentId
