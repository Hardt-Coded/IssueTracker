namespace Project

module Domain =
    
    open System
    open Types
    open Common.Types
    open Users.Types

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
                CreateUserId:string
                AssignTo:string
                Title:string
                Description:string
            }

            type DeleteIssue = {
                IssueId:string
                DeletedFrom:string
            }

            type ChangeState = {
                IssueId:string
                State:string
            }
            
            type AssignToUser = {
                IssueId:string
                UserId:string
            }

            type ChangeTitle = {
                IssueId:string
                Title:string
            }

            type ChangeDescription = {
                IssueId:string
                Description:string
            }

            type AddComment = {
                IssueId:string
                CommentId:string
                Text:string
                CreatedBy:string                
            }

            type ChangeComment = {
                IssueId:string
                CommentId:string
                Text:string
            }

            type DeleteComment = {
                IssueId:string
                CommentId:string
            }

            type AddAttachment = {
                IssueId:string
                AtachemntId:string
                Title:string
                FileName:string
                BlobReference:string
                CreatedBy:string

            }

            type RemoveAttachment = {
                IssueId:string
                AttachmentId:string
            }


        type IssueCommand =
            | CreateIssue of CommandArguments.CreateIssue
            | DeleteIssue of CommandArguments.DeleteIssue
            | ChangeState of CommandArguments.ChangeState
            | AssignToUser of CommandArguments.AssignToUser
            | ChangeTitle of CommandArguments.ChangeTitle
            | ChangeDescription of CommandArguments.ChangeDescription
            | AddComment of CommandArguments.AddComment
            | ChangeComment of CommandArguments.ChangeComment
            | DeleteComment of CommandArguments.DeleteComment
            | AddAttachment of CommandArguments.AddAttachment
            | RemoveAttachment of CommandArguments.RemoveAttachment


        module EventArguments =
            
            type IssueCreated = {
                IssueId:IssueId
                CreateUserId:UserId
                AssignTo:UserId
                Title:NoneEmptyString
                Description:string
            }

            type IssueDeleted = {
                IssueId:IssueId
                DeletedFrom:UserId
            }

            type StateChanged = {
                IssueId:IssueId
                State:IssueState
            }
            
            type AssignedToUser = {
                IssueId:IssueId
                UserId:UserId
            }

            type TitleChanged = {
                IssueId:IssueId
                Title:NoneEmptyString
            }

            type DescriptionChanged = {
                IssueId:IssueId
                Description:string
            }

            type CommentAdded = {
                IssueId:IssueId
                CommentId:CommentId
                Text:NoneEmptyString
                CreatedBy:UserId                
            }

            type CommentChanged = {
                IssueId:IssueId
                CommentId:CommentId
                Text:NoneEmptyString
            }

            type CommentDeleted = {
                IssueId:IssueId
                CommentId:CommentId
            }

            type AttachmentAdded = {
                IssueId:IssueId
                AtachemntId:AttachmentId
                Title:NoneEmptyString
                FileName:string
                BlobReference:string
                CreatedBy:UserId

            }

            type AttachmentRemoved = {
                IssueId:IssueId
                AttachmentId:AttachmentId
            }


        type IssueEvent =
            | IssueCreated of EventArguments.IssueCreated
            | IssueDeleted of EventArguments.IssueDeleted
            | StateChanged of EventArguments.StateChanged
            | AssignedToUser of EventArguments.AssignedToUser
            | TitleChanged of EventArguments.TitleChanged
            | DescriptionChanged of EventArguments.DescriptionChanged
            | CommentAdded of EventArguments.CommentAdded
            | CommentChanged of EventArguments.CommentChanged
            | CommentDeleted of EventArguments.CommentDeleted
            | AttachmentAdded of EventArguments.AttachmentAdded
            | AttachmentRemoved of EventArguments.AttachmentRemoved


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

