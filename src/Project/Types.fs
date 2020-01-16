namespace Project    
    
module Types =

    open System
    open Common.Domain

    type ProjectId = private ProjectId of string

    module ProjectId =
        
        let create projectId =
            if String.IsNullOrWhiteSpace(projectId) then
                sprintf "project id must not be empty" 
                |> DomainError
                |> List.singleton
                |> Error
            else
                ProjectId projectId |> Ok


        let value (ProjectId projectId) = projectId

        /// use only for event dto convertion
        let fromEventDto projectId = ProjectId projectId


    type IssueId = private IssueId of string
    
    module IssueId =
            
        let create issueId =
            if String.IsNullOrWhiteSpace(issueId) then
                sprintf "issue id must not be empty" 
                |> DomainError
                |> List.singleton
                |> Error
            else
                IssueId issueId |> Ok
    
    
        let value (IssueId issueId) = issueId
    
        /// use only for event dto convertion
        let fromEventDto issueId = IssueId issueId



    type CommentId = private CommentId of string
    
    module CommentId =
            
        let create commentId =
            if String.IsNullOrWhiteSpace(commentId) then
                sprintf "comment id must not be empty" 
                |> DomainError
                |> List.singleton
                |> Error
            else
                CommentId commentId |> Ok
    
    
        let value (CommentId commentId) = commentId
    
        /// use only for event dto convertion
        let fromEventDto commentId = CommentId commentId


    type AttachmentId = private AttachmentId of string
    
    module AttachmentId =
            
        let create attachmentId =
            if String.IsNullOrWhiteSpace(attachmentId) then
                sprintf "attachment id must not be empty" 
                |> DomainError
                |> List.singleton
                |> Error
            else
                AttachmentId attachmentId |> Ok
    
    
        let value (AttachmentId attachmentId) = attachmentId
    
        /// use only for event dto convertion
        let fromEventDto attachmentId = AttachmentId attachmentId




