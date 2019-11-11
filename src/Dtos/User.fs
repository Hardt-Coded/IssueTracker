namespace Dtos

module User =
    

    module Commands =

        [<CLIMutable>]
        type CreateUser = {
            Name:string
            EMail:string
            Password:string
        }

        [<CLIMutable>]
        type DeleteUser = {
            UserId:string
        }

        [<CLIMutable>]
        type ChangeEMail = {
            UserId:string
            EMail:string
        }

        [<CLIMutable>]
        type ChangePassword = {
            UserId:string
            Password:string
        }

        [<CLIMutable>]
        type AddToGroup = {
            UserId:string
            Group:string
        }

        [<CLIMutable>]
        type RemoveFromGroup = {
            UserId:string
            Group:string
        }

    module Events =
    
        open Common    

        [<CLIMutable>]
        type UserCreated = 
            {
                EventType:string
                UserId:string
                Name:string
                EMail:string
                PasswordHash:string
                PasswordSalt:string
            } 
            interface IEvent with
                member this.EventType = this.EventType
            

        [<CLIMutable>]
        type UserDeleted = 
            {
                EventType:string
                UserId:string
            } 
            interface IEvent with
                member this.EventType = this.EventType

        type EMailChanged = 
            {
                EventType:string
                UserId:string
                EMail:string
            }
            interface IEvent with
                member this.EventType = this.EventType

        [<CLIMutable>]
        type PasswordChanged = 
            {
                EventType:string
                UserId:string
                PasswordHash:string
                PasswordSalt:string
            }
            interface IEvent with
                member this.EventType = this.EventType

        [<CLIMutable>]
        type AddedToGroup = 
            {
                EventType:string
                UserId:string
                Group:string
            }
            interface IEvent with
                member this.EventType = this.EventType

        [<CLIMutable>]
        type RemovedFromGroup = 
            {
                EventType:string
                UserId:string
                Group:string
            }
            interface IEvent with
                member this.EventType = this.EventType



        // open Domain.User
        open Domain.Types.Common
        open Domain.Types.User

        

        let inline toDto event : ^ev when ^ev : (member EventType:string) =
            match event with
            | Domain.User.UserCreated e ->
                let hashPair = PasswordHash.value e.PasswordHash
                { 
                    UserCreated.UserId = UserId.value e.UserId
                    EMail = EMail.value e.EMail
                    Name = NotEmptyString.value e.Name
                    PasswordHash = hashPair.Hash
                    PasswordSalt = hashPair.Salt
                    EventType = "UserCreated"
                } |> unbox
            | Domain.User.UserDeleted e ->
                {
                    UserId = UserId.value e.UserId
                    EventType = "UserDeleted"
                } |> unbox
            | Domain.User.EMailChanged e ->
                {
                    UserId = UserId.value e.UserId
                    EMail = EMail.value e.EMail
                    EventType = "EMailChanged"
                } |> unbox
            | Domain.User.PasswordChanged e ->
                let hashPair = PasswordHash.value e.PasswordHash
                {
                    UserId = UserId.value e.UserId
                    PasswordHash = hashPair.Hash
                    PasswordSalt = hashPair.Salt
                    EventType = "PasswordChanged"
                } |> unbox
            | Domain.User.AddedToGroup e ->
                {
                    AddedToGroup.UserId = UserId.value e.UserId
                    Group = NotEmptyString.value e.Group
                    EventType = "AddedToGroup"
                } |> unbox
            | Domain.User.RemovedFromGroup e ->
                {
                    RemovedFromGroup.UserId = UserId.value e.UserId
                    Group = NotEmptyString.value e.Group
                    EventType = "RemovedFromGroup"
                } |> unbox
    
       


        let toDomain (ev:obj) =
            match ev with
            | :? UserCreated as e ->
                let result:Domain.User.EventArguments.UserCreated =
                    {
                        UserId = UserId.fromEventDto e.UserId
                        Name = NotEmptyString.fromEventDto e.Name
                        EMail = EMail.fromEventDto e.EMail
                        PasswordHash = PasswordHash.fromEventDto e.PasswordSalt e.PasswordHash
                    }
                result |> Domain.User.UserCreated
            | :? UserDeleted as e ->
                {
                    Domain.User.EventArguments.UserDeleted.UserId = UserId.fromEventDto e.UserId
                }
                |> Domain.User.UserDeleted

            | :? EMailChanged as e ->
                let result:Domain.User.EventArguments.EMailChanged =
                    {
                        UserId = UserId.fromEventDto e.UserId
                        EMail = EMail.fromEventDto e.EMail
                    }
                result |> Domain.User.EMailChanged
            | :? PasswordChanged as e ->
                let result:Domain.User.EventArguments.PasswordChanged =
                    {
                        UserId = UserId.fromEventDto e.UserId
                        PasswordHash = PasswordHash.fromEventDto e.PasswordSalt e.PasswordHash
                    }
                result |> Domain.User.PasswordChanged
            | :? AddedToGroup as e ->
                let result:Domain.User.EventArguments.AddedToGroup =
                    {
                        UserId = UserId.fromEventDto e.UserId
                        Group = NotEmptyString.fromEventDto e.Group
                    }
                result |> Domain.User.AddedToGroup
            | :? RemovedFromGroup as e ->
                let result:Domain.User.EventArguments.RemovedFromGroup =
                    {
                        UserId = UserId.fromEventDto e.UserId
                        Group = NotEmptyString.fromEventDto e.Group
                    }
                result |> Domain.User.RemovedFromGroup
            | _ ->
                let t = ev.GetType().Name
                failwith (sprintf "invalid type '%s' for an event dto" t)

            



    
            
