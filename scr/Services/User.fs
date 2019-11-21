namespace Services

module User =
    
    open System
    open System.Threading.Tasks
    open FSharp.Control.Tasks.V2    
    open Domain.User
    open Dtos.User.Commands    
    open Infrastructure

    let aggregateName =  "User"

    type UserService = {
        CreateUser:CommandArguments.CreateUser->Task<Result<unit,Domain.Common.Errors>>
        DeleteUser:CommandArguments.DeleteUser->Task<Result<unit,Domain.Common.Errors>>
        ChangeEMail:CommandArguments.ChangeEMail->Task<Result<unit,Domain.Common.Errors>>
        ChangeName:CommandArguments.ChangeName->Task<Result<unit,Domain.Common.Errors>>
        ChangePassword:CommandArguments.ChangePassword->Task<Result<unit,Domain.Common.Errors>>
        AddToGroup:CommandArguments.AddToGroup->Task<Result<unit,Domain.Common.Errors>>
        RemoveFromGroup:CommandArguments.RemoveFromGroup->Task<Result<unit,Domain.Common.Errors>>
    }


    let create storeEvents getEventStore (command:CommandArguments.CreateUser) = 
        task {

            // todo: vaidation, if user already exisits
            let events = aggregate.handle None (CreateUser command)
            match events with
            | Ok events ->
                let! versionResult =                     
                    events
                    |> List.map (fun i -> Dtos.User.Events.toDto i)
                    |> storeEvents getEventStore aggregateName command.UserId

                return versionResult
            | Error e ->
                return Error e
        }


    let delete readEvents storeEvents getEventStore (command:CommandArguments.DeleteUser) =
        task {
            
            let! userEvents =
                readEvents getEventStore aggregateName command.UserId

            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (DeleteUser command)

            match newEvents with 
            | Ok newEvents ->
                let! versionResult =
                    newEvents
                    |> List.map (fun i -> Dtos.User.Events.toDto i)
                    |> storeEvents getEventStore aggregateName command.UserId 

                return versionResult
            | Error e ->
                return Error e
        }


    let changeEMail readEvents storeEvents getEventStore (command:CommandArguments.ChangeEMail) =
        task {

            let! userEvents =
                readEvents getEventStore aggregateName command.UserId
            
            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (ChangeEMail command)

            match newEvents with 
            | Ok newEvents ->
                let! versionResult =
                    newEvents
                    |> List.map (fun i -> Dtos.User.Events.toDto i)
                    |> storeEvents getEventStore aggregateName command.UserId 

                return versionResult
            | Error e ->
                return Error e
        }


    let changeName readEvents storeEvents getEventStore (command:CommandArguments.ChangeName) =
        task {

            let! userEvents =
                readEvents getEventStore aggregateName command.UserId
            
            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (ChangeName command)

            match newEvents with 
            | Ok newEvents ->
                let! versionResult =
                    newEvents
                    |> List.map (fun i -> Dtos.User.Events.toDto i)
                    |> storeEvents getEventStore aggregateName command.UserId 

                return versionResult
            | Error e ->
                return Error e
        }


    let changePassword readEvents storeEvents getEventStore (command:CommandArguments.ChangePassword) =
        task {

            let! userEvents =
                readEvents getEventStore aggregateName command.UserId
            
            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (ChangePassword command)

            match newEvents with 
            | Ok newEvents ->
                let! versionResult =
                    newEvents
                    |> List.map (fun i -> Dtos.User.Events.toDto i)
                    |> storeEvents getEventStore aggregateName command.UserId 

                return versionResult
            | Error e ->
                return Error e
        }


    let addToGroup readEvents storeEvents getEventStore (command:CommandArguments.AddToGroup) =
        task {

            let! userEvents =
                readEvents getEventStore aggregateName command.UserId
            
            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (AddToGroup command)

            match newEvents with 
            | Ok newEvents ->
                let! versionResult =
                    newEvents
                    |> List.map (fun i -> Dtos.User.Events.toDto i)
                    |> storeEvents getEventStore aggregateName command.UserId 

                return versionResult
            | Error e ->
                return Error e
        }


    let removeFromGroup readEvents storeEvents getEventStore (command:CommandArguments.RemoveFromGroup) =
        task {

            let! userEvents =
                readEvents getEventStore aggregateName command.UserId
            
            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (RemoveFromGroup command)

            match newEvents with 
            | Ok newEvents ->
                let! versionResult =
                    newEvents
                    |> List.map (fun i -> Dtos.User.Events.toDto i)
                    |> storeEvents getEventStore aggregateName command.UserId 

                return versionResult
            | Error e ->
                return Error e
        }


    let get readEvents connection id =
        task {
            let! events = readEvents connection aggregateName id
            return events |> aggregate.exec None
        }




    let createUser = create EventStore.storeEvents Common.getEventStore

    let deleteUser = delete EventStore.User.readEvents EventStore.storeEvents Common.getEventStore

    let changeUserEMail = changeEMail EventStore.User.readEvents EventStore.storeEvents Common.getEventStore

    let changeUserName = changeName EventStore.User.readEvents EventStore.storeEvents Common.getEventStore

    let changeUserPassword = changePassword EventStore.User.readEvents EventStore.storeEvents Common.getEventStore

    let addUserToGroup = addToGroup EventStore.User.readEvents EventStore.storeEvents Common.getEventStore

    let removeUserFromGroup = removeFromGroup EventStore.User.readEvents EventStore.storeEvents Common.getEventStore
    
    let getUser = get EventStore.User.readEvents Common.getEventStore


    let userService = {
        CreateUser = createUser
        DeleteUser = deleteUser
        ChangeEMail = changeUserEMail
        ChangeName = changeUserName
        ChangePassword = changeUserPassword
        AddToGroup = addUserToGroup
        RemoveFromGroup = removeUserFromGroup
    }
        

    
