namespace Users

module Dtos =

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
        type ChangeName = {
            UserId:string
            Name:string
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
    
        open Common.Dtos  

        [<CLIMutable>]
        type UserCreated = 
            {
                UserId:string
                Name:string
                EMail:string
                PasswordHash:string
                PasswordSalt:string
            } 
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "UserCreated"
            

        [<CLIMutable>]
        type UserDeleted = 
            {
                UserId:string
            } 
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "UserDeleted"

        type EMailChanged = 
            {
                UserId:string
                EMail:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "EMailChanged"


        type NameChanged = 
            {
                UserId:string
                Name:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "NameChanged"

        [<CLIMutable>]
        type PasswordChanged = 
            {
                UserId:string
                PasswordHash:string
                PasswordSalt:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "PasswordChanged"

        [<CLIMutable>]
        type AddedToGroup = 
            {
                UserId:string
                Group:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "AddedToGroup"

        [<CLIMutable>]
        type RemovedFromGroup = 
            {
                UserId:string
                Group:string
            }
            interface IEvent with
                member this.EventType = this.EventType
            member this.EventType = "RemovedFromGroup"



        open Common.Types
        open Users.Types
        open Users.Domain

        

        let inline toDto event : ^ev when ^ev : (member EventType:string) =
            match event with
            | UserCreated e ->
                let hashPair = PasswordHash.value e.PasswordHash
                { 
                    UserCreated.UserId = UserId.value e.UserId
                    EMail = EMail.value e.EMail
                    Name = NotEmptyString.value e.Name
                    PasswordHash = hashPair.Hash
                    PasswordSalt = hashPair.Salt
                    
                } |> unbox
            | UserDeleted e ->
                {
                    UserId = UserId.value e.UserId
                } |> unbox
            | EMailChanged e ->
                {
                    UserId = UserId.value e.UserId
                    EMail = EMail.value e.EMail
                } |> unbox
            | NameChanged e ->
                {
                    UserId = UserId.value e.UserId
                    Name = NotEmptyString.value e.Name
                } |> unbox
            | PasswordChanged e ->
                let hashPair = PasswordHash.value e.PasswordHash
                {
                    UserId = UserId.value e.UserId
                    PasswordHash = hashPair.Hash
                    PasswordSalt = hashPair.Salt
                } |> unbox
            | AddedToGroup e ->
                {
                    AddedToGroup.UserId = UserId.value e.UserId
                    Group = NotEmptyString.value e.Group
                } |> unbox
            | RemovedFromGroup e ->
                {
                    RemovedFromGroup.UserId = UserId.value e.UserId
                    Group = NotEmptyString.value e.Group
                } |> unbox
    
       


        let toDomain (ev:obj) =
            match ev with
            | :? UserCreated as e ->
                let result:EventArguments.UserCreated =
                    {
                        UserId = UserId.fromEventDto e.UserId
                        Name = NotEmptyString.fromEventDto e.Name
                        EMail = EMail.fromEventDto e.EMail
                        PasswordHash = PasswordHash.fromEventDto e.PasswordSalt e.PasswordHash
                    }
                result |> UserCreated
            | :? UserDeleted as e ->
                {
                    EventArguments.UserDeleted.UserId = UserId.fromEventDto e.UserId
                }
                |> UserDeleted

            | :? EMailChanged as e ->
                let result:EventArguments.EMailChanged =
                    {
                        UserId = UserId.fromEventDto e.UserId
                        EMail = EMail.fromEventDto e.EMail
                    }
                result |> EMailChanged
            | :? NameChanged as e ->
                let result:EventArguments.NameChanged =
                    {
                        UserId = UserId.fromEventDto e.UserId
                        Name = NotEmptyString.fromEventDto e.Name
                    }
                result |> NameChanged
            | :? PasswordChanged as e ->
                let result:EventArguments.PasswordChanged =
                    {
                        UserId = UserId.fromEventDto e.UserId
                        PasswordHash = PasswordHash.fromEventDto e.PasswordSalt e.PasswordHash
                    }
                result |> PasswordChanged
            | :? AddedToGroup as e ->
                let result:EventArguments.AddedToGroup =
                    {
                        UserId = UserId.fromEventDto e.UserId
                        Group = NotEmptyString.fromEventDto e.Group
                    }
                result |> AddedToGroup
            | :? RemovedFromGroup as e ->
                let result:EventArguments.RemovedFromGroup =
                    {
                        UserId = UserId.fromEventDto e.UserId
                        Group = NotEmptyString.fromEventDto e.Group
                    }
                result |> RemovedFromGroup
            | _ ->
                let t = ev.GetType().Name
                failwith (sprintf "invalid type '%s' for an event dto" t)

            



    
            
