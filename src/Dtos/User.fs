namespace Dtos

module User =
    
    [<CLIMutable>]
    type UserCreated = {
        EventType:string
        UserId:string
        Name:string
        EMail:string
        PasswordHash:string
        PasswordSalt:string
    }

    [<CLIMutable>]
    type UserDeleted = {
        EventType:string
        UserId:string
    }

    type EMailChanged = {
        EventType:string
        UserId:string
        EMail:string
    }

    [<CLIMutable>]
    type PasswordChanged = {
        EventType:string
        UserId:string
        PasswordHash:string
        PasswordSalt:string
    }

    [<CLIMutable>]
    type AddedToGroup = {
        EventType:string
        UserId:string
        Group:string
    }

    [<CLIMutable>]
    type RemovedFromGroup = {
        EventType:string
        UserId:string
        Group:string
    }



    // open Domain.User
    open Domain.Types.Common
    open Domain.Types.User


    let inline toDto event : obj =
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
            } :> obj
        | Domain.User.UserDeleted e ->
            {
                UserId = UserId.value e.UserId
                EventType = "UserDeleted"
            } :> obj
        | Domain.User.EMailChanged e ->
            {
                UserId = UserId.value e.UserId
                EMail = EMail.value e.EMail
                EventType = "EMailChanged"
            } :> obj
        | Domain.User.PasswordChanged e ->
            let hashPair = PasswordHash.value e.PasswordHash
            {
                UserId = UserId.value e.UserId
                PasswordHash = hashPair.Hash
                PasswordSalt = hashPair.Salt
                EventType = "PasswordChanged"
            } :> obj
        | Domain.User.AddedToGroup e ->
            {
                AddedToGroup.UserId = UserId.value e.UserId
                Group = NotEmptyString.value e.Group
                EventType = "AddedToGroup"
            } :> obj
        | Domain.User.RemovedFromGroup e ->
            {
                RemovedFromGroup.UserId = UserId.value e.UserId
                Group = NotEmptyString.value e.Group
                EventType = "RemovedFromGroup"
            } :> obj
    
       


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

            



    
            
