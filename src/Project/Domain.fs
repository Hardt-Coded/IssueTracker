namespace Project

module Domain =
    
    open System
    open Types
    open Common.Types
    open Users.Types
    open Common.Domain

    [<AutoOpen>]
    module Issue =

        let (<*>) = Validation.apply 
        let (<!>) = Result.map

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
            AttachmentId:AttachmentId
            Title:NoneEmptyString
            FileName:NoneEmptyString
            BlobReference:NoneEmptyString
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
                ProjectId:string
                IssueId:string
                CreateBy:string
                AssignTo:string
                Title:string
                Description:string
            }

            type DeleteIssue = {
                ProjectId:string
                IssueId:string
                DeletedFrom:string
            }

            type ChangeIssueState = {
                ProjectId:string
                IssueId:string
                State:string
            }
            
            type AssignIssueToUser = {
                ProjectId:string
                IssueId:string
                UserId:string
            }

            type ChangeIssueTitle = {
                ProjectId:string
                IssueId:string
                Title:string
            }

            type ChangeIssueDescription = {
                ProjectId:string
                IssueId:string
                Description:string
            }

            type AddIssueComment = {
                ProjectId:string
                IssueId:string
                CommentId:string
                Text:string
                CreatedBy:string                
            }

            type ChangeIssueComment = {
                ProjectId:string
                IssueId:string
                CommentId:string
                Text:string
            }

            type DeleteIssueComment = {
                ProjectId:string
                IssueId:string
                CommentId:string
            }

            type AddIssueAttachment = {
                ProjectId:string
                IssueId:string
                AttachmentId:string
                Title:string
                FileName:string
                BlobReference:string
                CreatedBy:string

            }

            type RemoveIssueAttachment = {
                ProjectId:string
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
                ProjectId:ProjectId
                IssueId:IssueId
                CreatedBy:UserId
                AssignTo:UserId
                Title:NoneEmptyString
                Description:string
            }

            type IssueDeleted = {
                ProjectId:ProjectId
                IssueId:IssueId
                DeletedFrom:UserId
            }

            type IssueStateChanged = {
                ProjectId:ProjectId
                IssueId:IssueId
                State:IssueState
            }
            
            type IssueAssignedToUser = {
                ProjectId:ProjectId
                IssueId:IssueId
                UserId:UserId
            }

            type IssueTitleChanged = {
                ProjectId:ProjectId
                IssueId:IssueId
                Title:NoneEmptyString
            }

            type IssueDescriptionChanged = {
                ProjectId:ProjectId
                IssueId:IssueId
                Description:string
            }

            type IssueCommentAdded = {
                ProjectId:ProjectId
                IssueId:IssueId
                CommentId:CommentId
                Text:NoneEmptyString
                CreatedBy:UserId                
            }

            type IssueCommentChanged = {
                ProjectId:ProjectId
                IssueId:IssueId
                CommentId:CommentId
                Text:NoneEmptyString
            }

            type IssueCommentDeleted = {
                ProjectId:ProjectId
                IssueId:IssueId
                CommentId:CommentId
            }

            type IssueAttachmentAdded = {
                ProjectId:ProjectId
                IssueId:IssueId
                AttachmentId:AttachmentId
                Title:NoneEmptyString
                FileName:NoneEmptyString
                BlobReference:NoneEmptyString
                CreatedBy:UserId

            }

            type IssueAttachmentRemoved = {
                ProjectId:ProjectId
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


        let rec handle (state:Issue option) (command:IssueCommand) : Result<IssueEvent list,Errors list> =
               match state, command with
               | None, CreateIssue args ->
                   createIssue args
               | Some _, CreateIssue _ ->
                   "you can not have a create issue event, when this issue already exists"
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
            let create projectId issueId createdBy assignTo title description =
                IssueCreated {
                    ProjectId = projectId
                    IssueId=issueId
                    CreatedBy=createdBy
                    AssignTo=assignTo
                    Title=title
                    Description=description
                }
                |> List.singleton

            let projectId = ProjectId.create args.ProjectId
            let issueId = IssueId.create args.IssueId
            let createdBy = UserId.create args.CreateBy
            let assignTo = UserId.create args.AssignTo
            let title = NoneEmptyString.create "Issue Title" args.Title
            let description = Ok args.Description
            create <!> projectId <*> issueId <*> createdBy <*> assignTo <*> title <*> description


        and deleteIssue args =
            let create projectId issueId deletedFrom = 
                IssueDeleted {
                    ProjectId = projectId
                    IssueId=issueId
                    DeletedFrom=deletedFrom
                }
                |> List.singleton

            let projectId = ProjectId.create args.ProjectId
            let issueId = IssueId.create args.IssueId
            let deletedFrom = UserId.create args.DeletedFrom
            create <!> projectId <*> issueId <*> deletedFrom


        and changeState args =
            let create projectId issueId state = 
                IssueStateChanged {
                    ProjectId = projectId
                    IssueId=issueId
                    State=state
                }
                |> List.singleton

            let projectId = ProjectId.create args.ProjectId
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

            create <!> projectId <*> issueId <*> state


        and assignToUser args =
            let create projectId issueId userId = 
                IssueAssignedToUser {
                    ProjectId = projectId
                    IssueId=issueId
                    UserId=userId
                }
                |> List.singleton

            let projectId = ProjectId.create args.ProjectId
            let issueId = IssueId.create args.IssueId
            let userId = UserId.create args.UserId
            create <!> projectId <*> issueId <*> userId

               
        and changeTitle args =
            let create projectId issueId title =
                IssueTitleChanged {
                    ProjectId = projectId
                    IssueId=issueId
                    Title = title
                }
                |> List.singleton

            let projectId = ProjectId.create args.ProjectId
            let issueId = IssueId.create args.IssueId
            let title = NoneEmptyString.create "Issue Title" args.Title
            create <!> projectId <*> issueId <*> title

               
        and changeDescription args =
            let create projectId issueId description =
                IssueDescriptionChanged {
                    ProjectId = projectId
                    IssueId=issueId
                    Description=description
                }
                |> List.singleton

            let projectId = ProjectId.create args.ProjectId
            let issueId = IssueId.create args.IssueId
            let description = Ok args.Description
            create <!> projectId <*> issueId <*> description
               

        and addComment args =
            let create projectId issueId commentId text createdBy =
                IssueCommentAdded {
                    ProjectId = projectId
                    IssueId=issueId
                    CommentId=commentId
                    Text=text
                    CreatedBy=createdBy                
                }
                |> List.singleton

            let projectId = ProjectId.create args.ProjectId
            let issueId = IssueId.create args.IssueId
            let commentId = CommentId.create args.CommentId
            let text = NoneEmptyString.create "Comment Text" args.Text
            let createdBy = UserId.create args.CreatedBy
            create <!> projectId <*> issueId <*> commentId <*> text <*> createdBy


        and changeComment args =
            let create projectId issueId commentId text =
                IssueCommentChanged {
                    ProjectId = projectId
                    IssueId=issueId
                    CommentId=commentId
                    Text=text
                }
                |> List.singleton

            let projectId = ProjectId.create args.ProjectId
            let issueId = IssueId.create args.IssueId
            let commentId = CommentId.create args.CommentId
            let text = NoneEmptyString.create "Comment Text" args.Text
            create <!> projectId <*> issueId <*> commentId <*> text

               
        and deleteComment args =
            let create projectId issueId commentId =
                IssueCommentDeleted{
                    ProjectId = projectId
                    IssueId=issueId
                    CommentId=commentId
                }
                |> List.singleton

            let projectId = ProjectId.create args.ProjectId
            let issueId = IssueId.create args.IssueId
            let commentId = CommentId.create args.CommentId
            create <!> projectId <*> issueId <*> commentId

               
        and addAttachment args =
            let create projectId issueId attachmentId title fileName blobReference createdBy =
                IssueAttachmentAdded {
                    ProjectId = projectId
                    IssueId=issueId
                    AttachmentId=attachmentId
                    Title=title
                    FileName=fileName
                    BlobReference=blobReference
                    CreatedBy=createdBy
                }
                |> List.singleton

            let projectId = ProjectId.create args.ProjectId
            let issueId = IssueId.create args.IssueId
            let attachmentId = AttachmentId.create args.AttachmentId
            let title = NoneEmptyString.create "Attachment Title" args.Title
            let fileName = NoneEmptyString.create "Filename" args.FileName
            let blobReference = NoneEmptyString.create "Blob Reference" args.BlobReference
            let createdBy = UserId.create args.CreatedBy
            create <!> projectId <*> issueId <*> attachmentId <*> title <*> fileName <*> blobReference <*> createdBy
               
        and removeAttachment args =
            let create projectId issueId attachmentId =
                IssueAttachmentRemoved {
                    ProjectId = projectId
                    IssueId=issueId
                    AttachmentId=attachmentId
                }
                |> List.singleton

            let projectId = ProjectId.create args.ProjectId
            let issueId = IssueId.create args.IssueId
            let attachmentId = AttachmentId.create args.AttachmentId
            create <!> projectId <*> issueId <*> attachmentId




        let apply (state:Issue option) event : Issue option =
            match state,event with
            | None, IssueCreated ev ->
                Some {
                    IssueId = ev.IssueId
                    Title=ev.Title
                    Description=ev.Description
                    State=IssueState.New
                    AssignedTo=ev.AssignTo
                    CreatedBy=ev.CreatedBy
                    CreatedOn=DateTime.UtcNow
                    Comments= []
                    Attachments=[]
                
                }
            | Some _, IssueCreated ev ->
                failwith "a create event is invalid, when a issue state already exisits"
            | Some state, IssueDeleted ev ->
                None
            | Some state, IssueStateChanged ev ->
                { state with 
                    State = ev.State
                } |> Some
            | Some state, IssueAssignedToUser ev ->
                { state with 
                    AssignedTo = ev.UserId
                } |> Some
            | Some state, IssueTitleChanged ev ->
                { state with 
                    Title = ev.Title
                } |> Some
            | Some state, IssueDescriptionChanged ev ->
                { state with 
                    Description = ev.Description
                } |> Some
            | Some state, IssueCommentAdded ev ->
                { state with 
                    Comments = { CommentId = ev.CommentId; Text = ev.Text; CreatedBy = ev.CreatedBy; CreatedOn = DateTime.UtcNow } :: state.Comments
                } |> Some
            | Some state, IssueCommentChanged ev ->
                let comment = state.Comments |> List.tryFind (fun i-> i.CommentId = ev.CommentId)
                match comment with
                | None -> failwith "the comment with the id shopuld be there. we have a validation error in the command processing of the issues."
                | Some comment ->
                    let newComment = {
                        comment with
                            Text = ev.Text
                    }
                    let newComments = 
                        state.Comments 
                        |> List.map (fun c ->
                            if c.CommentId = ev.CommentId then newComment else c
                        )

                    { state with 
                        Comments = newComments
                    } |> Some
            | Some state, IssueCommentDeleted ev ->
                { state with 
                    Comments = state.Comments |> List.filter (fun i -> i.CommentId <> ev.CommentId)
                } |> Some
            | Some state, IssueAttachmentAdded ev ->
                { state with 
                    Attachments = {
                        AttachmentId=ev.AttachmentId
                        Title=ev.Title
                        FileName=ev.FileName
                        BlobReference=ev.BlobReference
                        CreatedBy=ev.CreatedBy
                        CreatedOn=DateTime.UtcNow
                    } :: state.Attachments
                } |> Some
            | Some state, IssueAttachmentRemoved ev ->
                { state with 
                    Attachments = state.Attachments |> List.filter (fun i -> i.AttachmentId <> ev.AttachmentId)
                } |> Some

            | None, _ ->
                sprintf "can not apply the event of type %s to the state" (event.GetType().Name)
                |> failwith


    type ProjectState =
        | Active
        | Done
        | Inactive


    type Project = {
        ProjectId:ProjectId
        Title:NoneEmptyString
        Description:string
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
            DeletedFrom:string
        }


        type ChangeProjectState = {
            ProjectId:string
            State:string
        }


        type ChangeProjectTitle = {
            ProjectId:string
            Title:string
        }

        type ChangeProjectDescription = {
            ProjectId:string
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
            Title:NoneEmptyString
            Description:string
            CreatedBy:UserId
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
        | IssueEvent of IssueEvent


    
    let rec handle (state:Project option) (command:ProjectCommand) : Result<ProjectEvent list,Errors list> =
        match state, command with
        | None, CreateProject args ->
            createProject args
        | Some _, CreateProject _ ->
            "you can not have a create project event, when this project already exists"
            |> DomainError
            |> List.singleton
            |> Error
        | Some state, DeleteProject args ->
            deleteProject args
        | Some state, ChangeProjectState args ->
            changeProjectState args
        | Some state, ChangeProjectTitle args ->
            changeProjectTitle args
        | Some state, ChangeProjectDescription args ->
            changeProjectDescription args
        | Some state, IssueCommand issueCommand ->
            handleIssueCommand state issueCommand
        | None, _ ->
            "invalid command, project does not exists"
            |> DomainError
            |> List.singleton
            |> Error


    and createProject args =
        let create projectId title description createdBy =
            ProjectCreated {
                ProjectId=projectId
                Title=title
                Description=description
                CreatedBy=createdBy
            } 
            |> List.singleton

        let projectId = ProjectId.create args.ProjectId
        let title = NoneEmptyString.create "Project Title" args.Title
        let description = Ok args.Description
        let createdBy = UserId.create args.CreatedBy
        create <!> projectId <*> title <*> description <*> createdBy

    
    and deleteProject args =
        let create projectId deletedFrom =
            ProjectDeleted {
                ProjectId=projectId
                DeletedFrom=deletedFrom
            }
            |> List.singleton

        let projectId = ProjectId.create args.ProjectId
        let deletedFrom = UserId.create args.DeletedFrom
        create <!> projectId <*> deletedFrom


    and changeProjectState args =
        let create projectId state =
            ProjectStateChanged {
                ProjectId=projectId
                State=state
            }
            |> List.singleton

        let projectId = ProjectId.create args.ProjectId
        let state = 
            match args.State.ToUpper() with            
            | "ACTIVE" -> Ok ProjectState.Active
            | "DONE" -> Ok ProjectState.Done
            | "ONHOLD" -> Ok ProjectState.Inactive
            | _ -> 
                "invalid project state"
                |> DomainError
                |> List.singleton
                |> Error

        create <!> projectId <*> state



    and changeProjectTitle args =
        let create projectId title =
            ProjectTitleChanged {
                ProjectId=projectId
                Title=title
            }
            |> List.singleton

        let projectId = ProjectId.create args.ProjectId
        let title = NoneEmptyString.create "Project Title" args.Title
        create <!> projectId <*> title


    and changeProjectDescription args =
        let create projectId description =
            ProjectDescriptionChanged {
                ProjectId=projectId
                Description=description
            }
            |> List.singleton

        let projectId = ProjectId.create args.ProjectId
        let description = Ok args.Description
        create <!> projectId <*> description


    and handleIssueCommand state issueCommand =
        // special caseslet
        let issueId = 
            match issueCommand with
            | CreateIssue cmd -> cmd.IssueId
            | DeleteIssue cmd -> cmd.IssueId
            | ChangeIssueState cmd -> cmd.IssueId
            | AssignIssueToUser cmd -> cmd.IssueId
            | ChangeIssueTitle cmd -> cmd.IssueId
            | ChangeIssueDescription cmd -> cmd.IssueId
            | AddIssueComment cmd -> cmd.IssueId
            | ChangeIssueComment cmd -> cmd.IssueId
            | DeleteIssueComment cmd -> cmd.IssueId
            | AddIssueAttachment cmd -> cmd.IssueId
            | RemoveIssueAttachment cmd -> cmd.IssueId
            
        let issue = 
            state.Issues 
            |> List.tryFind (fun i -> (IssueId.value i.IssueId) = issueId)
        
        Issue.handle issue issueCommand
        |> Result.map (List.map IssueEvent)


    let apply (state:Project option) event : Project option =
        match state,event with
        | None, ProjectCreated ev ->
            {
                ProjectId=ev.ProjectId
                Title=ev.Title
                Description=ev.Description
                State=ProjectState.Active
                Issues=[]
            } |> Some
        | Some _, ProjectCreated ev ->
            failwith "a create event is invalid, when a issue state already exisits"
        | Some state, ProjectDeleted ev ->
            None
        | Some state, ProjectStateChanged ev ->
            { state with
                State =ev.State
            } |> Some

        | Some state, ProjectTitleChanged ev ->
            { state with
                Title = ev.Title
            } |> Some
        | Some state, ProjectDescriptionChanged ev ->
            { state with
                Description = ev.Description
            } |> Some
        | Some state, IssueEvent issueEvent ->
            let issueId =
                match issueEvent with
                | IssueCreated ev -> ev.IssueId
                | IssueDeleted ev -> ev.IssueId
                | IssueStateChanged ev -> ev.IssueId
                | IssueAssignedToUser ev -> ev.IssueId
                | IssueTitleChanged ev -> ev.IssueId
                | IssueDescriptionChanged ev -> ev.IssueId
                | IssueCommentAdded ev -> ev.IssueId
                | IssueCommentChanged ev -> ev.IssueId
                | IssueCommentDeleted ev -> ev.IssueId
                | IssueAttachmentAdded ev -> ev.IssueId
                | IssueAttachmentRemoved ev -> ev.IssueId

            let oldIssueState = state.Issues |> List.tryFind (fun i-> i.IssueId = issueId)

            let newIssueState =
                Issue.apply oldIssueState issueEvent

            match oldIssueState, newIssueState with
            | None, Some newIssue ->
                { state with
                    Issues = newIssue :: state.Issues
                } |> Some
            | Some _, Some newIssueState ->
                { state with
                    Issues = state.Issues |> List.map (fun i -> if i.IssueId = newIssueState.IssueId then newIssueState else i)
                } |> Some
            | Some oldIssueState, None ->
                { state with
                    Issues = state.Issues |> List.filter (fun i -> i.IssueId <> oldIssueState.IssueId)
                } |> Some
            | None, None ->
                Some state
                
        | None, _ ->
            sprintf "can not apply the event of type %s to the state" (event.GetType().Name)
            |> failwith


    let private exec = exec apply
    let private execWithVersion = execWithEvents apply


    let aggregate : Aggregate<_,_,_,Errors list> = {
        Apply = apply
        Handle = handle
        Exec = exec
        ExecWithVersion = execWithVersion
    }




   
