namespace Users

module Dtos =


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
                    UserCreated.UserId      = UserId.value e.UserId
                    EMail                   = EMail.value e.EMail
                    Name                    = NoneEmptyString.value e.Name
                    PasswordHash            = hashPair.Hash
                    PasswordSalt            = hashPair.Salt
                    
                } |> unbox
            | UserDeleted e ->
                {
                    UserId                  = UserId.value e.UserId
                } |> unbox
            | EMailChanged e ->
                {
                    UserId                  = UserId.value e.UserId
                    EMail                   = EMail.value e.EMail
                } |> unbox
            | NameChanged e ->
                {
                    UserId                  = UserId.value e.UserId
                    Name                    = NoneEmptyString.value e.Name
                } |> unbox
            | PasswordChanged e ->
                let hashPair = PasswordHash.value e.PasswordHash
                {
                    UserId                  = UserId.value e.UserId
                    PasswordHash            = hashPair.Hash
                    PasswordSalt            = hashPair.Salt
                } |> unbox
            | AddedToGroup e ->
                {
                    AddedToGroup.UserId     = UserId.value e.UserId
                    Group                   = NoneEmptyString.value e.Group
                } |> unbox
            | RemovedFromGroup e ->
                {
                    RemovedFromGroup.UserId = UserId.value e.UserId
                    Group                   = NoneEmptyString.value e.Group
                } |> unbox
    
       

        

        let toDomain (ev:IEvent) =
            match ev with
            | :? UserCreated as e ->
                UserCreated
                    {
                        UserId              = UserId.fromEventDto e.UserId
                        Name                = NoneEmptyString.fromEventDto e.Name
                        EMail               = EMail.fromEventDto e.EMail
                        PasswordHash        = PasswordHash.fromEventDto e.PasswordSalt e.PasswordHash
                    }
            | :? UserDeleted as e ->
                 UserDeleted {
                    UserId                  = UserId.fromEventDto e.UserId
                }

            | :? EMailChanged as e ->
                EMailChanged
                    {
                        UserId              = UserId.fromEventDto e.UserId
                        EMail               = EMail.fromEventDto e.EMail
                    }
            | :? NameChanged as e ->
                NameChanged
                    {
                        UserId              = UserId.fromEventDto e.UserId
                        Name                = NoneEmptyString.fromEventDto e.Name
                    }
            | :? PasswordChanged as e ->
                PasswordChanged
                    {
                        UserId              = UserId.fromEventDto e.UserId
                        PasswordHash        = PasswordHash.fromEventDto e.PasswordSalt e.PasswordHash
                    }
            | :? AddedToGroup as e ->
                AddedToGroup
                    {
                        UserId              = UserId.fromEventDto e.UserId
                        Group               = NoneEmptyString.fromEventDto e.Group
                    }
            | :? RemovedFromGroup as e ->
                RemovedFromGroup
                    {
                        UserId              = UserId.fromEventDto e.UserId
                        Group               = NoneEmptyString.fromEventDto e.Group
                    }
            | _ ->
                let t = ev.GetType().Name
                failwith (sprintf "invalid type '%s' for an event dto" t)

            



    
            
