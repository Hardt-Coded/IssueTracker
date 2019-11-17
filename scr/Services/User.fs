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
        CreateUser:CreateUser->Task<Result<unit,Domain.Common.Errors>>
        DeleteUser:DeleteUser->Task<Result<unit,Domain.Common.Errors>>
        ChangeEMail:ChangeEMail->Task<Result<unit,Domain.Common.Errors>>
        ChangePassword:ChangePassword->Task<Result<unit,Domain.Common.Errors>>
        AddToGroup:AddToGroup->Task<Result<unit,Domain.Common.Errors>>
        RemoveFromGroup:RemoveFromGroup->Task<Result<unit,Domain.Common.Errors>>
    }


    let create storeEvents connection (commandDto:CreateUser) = 
        task {
            let command : CommandArguments.CreateUser = {
                UserId = Guid.NewGuid().ToString("N")
                Name = commandDto.Name
                EMail = commandDto.EMail
                Password = commandDto.Password
            }

            // todo: vaidation, if user already exisits
            let events = aggregate.handle None (CreateUser command)
            match events with
            | Ok events ->
                let! versionResult =                     
                    events
                    |> List.map (fun i -> Dtos.User.Events.toDto i)
                    |> storeEvents connection aggregateName command.UserId

                return versionResult
            | Error e ->
                return Error e
        }


    let delete readEvents storeEvents connection (commandDto:DeleteUser) =
        task {
            let command : CommandArguments.DeleteUser = {
                UserId = commandDto.UserId
            }

            let! userEvents =
                readEvents connection aggregateName command.UserId

            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (DeleteUser command)

            match newEvents with 
            | Ok newEvents ->
                let! versionResult =
                    newEvents
                    |> List.map (fun i -> Dtos.User.Events.toDto i)
                    |> storeEvents connection aggregateName command.UserId 

                return versionResult
            | Error e ->
                return Error e
        }


    let changeEMail readEvents storeEvents connection (commandDto:ChangeEMail) =
        task {
            let command : CommandArguments.ChangeEMail = {
                UserId = commandDto.UserId
                EMail = commandDto.EMail
            }

            let! userEvents =
                readEvents connection aggregateName command.UserId
            
            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (ChangeEMail command)

            match newEvents with 
            | Ok newEvents ->
                let! versionResult =
                    newEvents
                    |> List.map (fun i -> Dtos.User.Events.toDto i)
                    |> storeEvents connection aggregateName command.UserId 

                return versionResult
            | Error e ->
                return Error e
        }


    let changePassword readEvents storeEvents connection (commandDto:ChangePassword) =
        task {
            let command : CommandArguments.ChangePassword = {
                UserId = commandDto.UserId
                Password = commandDto.Password
            }

            let! userEvents =
                readEvents connection aggregateName command.UserId
            
            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (ChangePassword command)

            match newEvents with 
            | Ok newEvents ->
                let! versionResult =
                    newEvents
                    |> List.map (fun i -> Dtos.User.Events.toDto i)
                    |> storeEvents connection aggregateName command.UserId 

                return versionResult
            | Error e ->
                return Error e
        }


    let addToGroup readEvents storeEvents connection (commandDto:AddToGroup) =
        task {
            let command : CommandArguments.AddToGroup = {
                UserId = commandDto.UserId
                Group=commandDto.Group
            }

            let! userEvents =
                readEvents connection aggregateName command.UserId
            
            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (AddToGroup command)

            match newEvents with 
            | Ok newEvents ->
                let! versionResult =
                    newEvents
                    |> List.map (fun i -> Dtos.User.Events.toDto i)
                    |> storeEvents connection aggregateName command.UserId 

                return versionResult
            | Error e ->
                return Error e
        }


    let removeFromGroup readEvents storeEvents connection (commandDto:RemoveFromGroup) =
        task {
            let command : CommandArguments.RemoveFromGroup = {
                UserId = commandDto.UserId
                Group=commandDto.Group
            }

            let! userEvents =
                readEvents connection aggregateName command.UserId
            
            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (RemoveFromGroup command)

            match newEvents with 
            | Ok newEvents ->
                let! versionResult =
                    newEvents
                    |> List.map (fun i -> Dtos.User.Events.toDto i)
                    |> storeEvents connection aggregateName command.UserId 

                return versionResult
            | Error e ->
                return Error e
        }


    let get readEvents connection id =
        task {
            let! events = readEvents connection aggregateName id
            return events |> aggregate.exec None
        }




    let createUser = create EventStore.storeEvents Common.connection

    let deleteUser = delete EventStore.User.readEvents EventStore.storeEvents Common.connection

    let changeUserEMail = changeEMail EventStore.User.readEvents EventStore.storeEvents Common.connection

    let changeUserPassword = changePassword EventStore.User.readEvents EventStore.storeEvents Common.connection

    let addUserToGroup = addToGroup EventStore.User.readEvents EventStore.storeEvents Common.connection

    let removeUserFromGroup = removeFromGroup EventStore.User.readEvents EventStore.storeEvents Common.connection
    
    let getUser = get EventStore.User.readEvents Common.connection


    let userService = {
        CreateUser = createUser
        DeleteUser = deleteUser
        ChangeEMail = changeUserEMail
        ChangePassword = changeUserPassword
        AddToGroup = addUserToGroup
        RemoveFromGroup = removeUserFromGroup
    }
        

    
