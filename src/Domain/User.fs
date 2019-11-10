namespace Domain


//open Result
open Types.Common
open Types.User
open System
open Common

module User =

    open Common

    type State = {
        UserId:UserId
        Name:NotEmptyString
        EMail:EMail
        PasswordHash:PasswordHash
        Groups: NotEmptyString list
    }


    module CommandArguments =

        type CreateUser = {
            Name:string
            EMail:string
            Password:string
        }

        type DeleteUser = {
            UserId:string
        }

        type ChangeEMail = {
            UserId:string
            EMail:string
        }

        type ChangePassword = {
            UserId:string
            Password:string
        }

        type AddToGroup = {
            UserId:string
            Group:string
        }

        type RemoveFromGroup = {
            UserId:string
            Group:string
        }


    type Command = 
        | CreateUser of CommandArguments.CreateUser
        | DeleteUser of CommandArguments.DeleteUser
        | ChangeEMail of CommandArguments.ChangeEMail
        | ChangePassword of CommandArguments.ChangePassword
        | AddToGroup of CommandArguments.AddToGroup
        | RemoveFromGroup of CommandArguments.RemoveFromGroup


    module EventArguments =

        type UserCreated = {
            UserId:UserId
            Name:NotEmptyString
            EMail:EMail
            PasswordHash:PasswordHash
        }

        type UserDeleted = {
            UserId:UserId
        }

        type EMailChanged = {
            UserId:UserId
            EMail:EMail
        }

        type PasswordChanged = {
            UserId:UserId
            PasswordHash:PasswordHash
        }

        type AddedToGroup = {
            UserId:UserId
            Group:NotEmptyString
        }

        type RemovedFromGroup = {
            UserId:UserId
            Group:NotEmptyString
        }


    type Event = 
        | UserCreated of EventArguments.UserCreated
        | UserDeleted of EventArguments.UserDeleted
        | EMailChanged of EventArguments.EMailChanged
        | PasswordChanged of EventArguments.PasswordChanged
        | AddedToGroup of EventArguments.AddedToGroup
        | RemovedFromGroup of EventArguments.RemovedFromGroup


    let rec private handle (state:State option) command : Result<Event list,Errors> =
        match state,command with
        | None, CreateUser args ->
            userCreated args
        | Some _, CreateUser _ ->
            "you can not have a create user event, when a user alread existis"
            |> DomainError
            |> Error
        | Some state, DeleteUser args ->
            userDeleted state args
        | Some _, ChangeEMail args ->
            emailChanged args 
        | Some state, ChangePassword args ->
            changePassword args
        | Some state, AddToGroup args ->
            addedToGroup state args
        | Some state, RemoveFromGroup args ->
            removedFromGroup state args
        | None, _ ->
            "you can not have any other event excpect of the created event on an empty state"
            |> DomainError
            |> Error


    and userCreated args =
        result {
            let! name = NotEmptyString.create "Name" args.Name
            let! email = EMail.create args.EMail
            let! passwordHash = PasswordHash.create args.Password
            let! userId = UserId.create <| Guid.NewGuid().ToString("N")
            let userCreated : EventArguments.UserCreated = {
                UserId = userId
                Name = name
                EMail = email
                PasswordHash = passwordHash
            }
            return [ UserCreated userCreated ]
        }


    and userDeleted (state:State) args =
        result {
            let! userId = UserId.create args.UserId
            // does it makes any sense to check, if the user id matches?
            // i tend to no, but I leave it here
            if (userId <> state.UserId) then
                return! 
                    "the userId does not match" 
                    |> DomainError
                    |> Error
            else
                return [ UserDeleted { UserId = userId} ]
        }


    and emailChanged args =
        result {
            let! userId = UserId.create args.UserId
            let! email = EMail.create args.EMail
            return [ EMailChanged { UserId = userId; EMail = email } ]
        }


    and changePassword args =
        result {
            let! userId = UserId.create args.UserId
            let! passwordHash = PasswordHash.create args.Password
            return [ PasswordChanged { UserId = userId; PasswordHash = passwordHash } ]
        }


    and addedToGroup state args =
        result {
            let! userId = UserId.create args.UserId
            let! group = NotEmptyString.create "Group" args.Group
            if (state.Groups |> List.exists (fun i -> i = group)) then
                return! 
                    sprintf "user already assigned to group %s" args.Group
                    |> DomainError
                    |> Error
            else
                return [ AddedToGroup { UserId = userId; Group = group } ]
        }


    and removedFromGroup state args =
        result {
            let! userId = UserId.create args.UserId
            let! group = NotEmptyString.create "Group" args.Group
            if (state.Groups |> List.exists (fun i -> i = group)) then
                return [ RemovedFromGroup { UserId = userId; Group = group } ]
            else
                return! 
                    sprintf "user is not member of the group %s" args.Group
                    |> DomainError
                    |> Error
        }
                
       
    let private apply (state:State option) event : Result<State option,Errors> =
        match state, event with
        | None, UserCreated args ->
            Some { 
                UserId = args.UserId
                EMail = args.EMail
                Name = args.Name
                PasswordHash = args.PasswordHash
                Groups = []
            } |> Ok
        | Some _, UserCreated args ->
            DomainError "a create event is invalid, when a user state already exisits" |> Error
        | Some _, UserDeleted _ ->
            None |> Ok
        | Some state, EMailChanged args ->
            { state with
                EMail = args.EMail } 
            |> Some |> Ok
        | Some state, PasswordChanged args ->
            { state with
                PasswordHash = args.PasswordHash } 
            |> Some |> Ok
        | Some state, AddedToGroup args ->
            { state with
                Groups = args.Group :: state.Groups } 
            |> Some |> Ok
        | Some state, RemovedFromGroup args ->
            let newGroups =
                state.Groups
                |> List.filter (fun i -> i<> args.Group)

            { state with
                Groups = newGroups } 
            |> Some |> Ok
        | _ ->
            sprintf "can not apply the event of type %s to the state" (event.GetType().Name)
            |> DomainError
            |> Error


    let aggregate = {
        apply = apply
        handle = handle
    }
            





