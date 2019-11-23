namespace Services

module User =
    
    open System
    open System.Threading.Tasks
    open FSharp.Control.Tasks.V2    
    open Domain.User
    open Domain.Common
    open Infrastructure.EventStore.User

    let aggregateName =  "User"

    

    let private storeUserEvents (eventStore:UserEventStore) id events =
        task {
            let! versionResult =                     
                events
                |> List.map (fun i -> Dtos.User.Events.toDto i)
                |> eventStore.StoreEvents id
            return versionResult
        }


    let private create (eventStore:UserEventStore) (checkEMailDuplicate:string -> bool) (command:CommandArguments.CreateUser) = 
        task {

            if (checkEMailDuplicate command.EMail) then
                return DomainError "email already in use" |> Error
            else
                let events = aggregate.handle None (CreateUser command)
                match events with
                | Ok events ->
                    return!
                        events
                        |> storeUserEvents eventStore command.UserId
                | Error e ->
                    return Error e
        }


    let delete (eventStore:UserEventStore) (command:CommandArguments.DeleteUser) =
        task {
            
            let! userEvents =
                eventStore.ReadEvents command.UserId

            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (DeleteUser command)

            match newEvents with 
            | Ok newEvents ->
                return!
                    newEvents
                    |> storeUserEvents eventStore command.UserId
            | Error e ->
                return Error e
        }


    let changeEMail (eventStore:UserEventStore) (checkEMailDuplicate:string -> bool) (command:CommandArguments.ChangeEMail) =
        task {

            if (checkEMailDuplicate command.EMail) then
                return DomainError "email already in use" |> Error
            else
                let! userEvents =
                    eventStore.ReadEvents command.UserId
            
                let user =
                    userEvents |> aggregate.exec None
            
                let newEvents =
                    aggregate.handle user (ChangeEMail command)

                match newEvents with 
                | Ok newEvents ->
                    return!
                        newEvents
                        |> storeUserEvents eventStore command.UserId
                | Error e ->
                    return Error e
        }


    let changeName (eventStore:UserEventStore) (command:CommandArguments.ChangeName) =
        task {

            let! userEvents =
                eventStore.ReadEvents command.UserId
            
            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (ChangeName command)

            match newEvents with 
            | Ok newEvents ->
                return!
                    newEvents
                    |> storeUserEvents eventStore command.UserId
            | Error e ->
                return Error e
        }


    let changePassword (eventStore:UserEventStore) (command:CommandArguments.ChangePassword) =
        task {

            let! userEvents =
                eventStore.ReadEvents command.UserId
            
            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (ChangePassword command)

            match newEvents with 
            | Ok newEvents ->
                return!
                    newEvents
                    |> storeUserEvents eventStore command.UserId
            | Error e ->
                return Error e
        }


    let addToGroup (eventStore:UserEventStore) (command:CommandArguments.AddToGroup) =
        task {

            let! userEvents =
                eventStore.ReadEvents command.UserId
            
            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (AddToGroup command)

            match newEvents with 
            | Ok newEvents ->
                return!
                    newEvents
                    |> storeUserEvents eventStore command.UserId
            | Error e ->
                return Error e
        }


    let removeFromGroup (eventStore:UserEventStore) (command:CommandArguments.RemoveFromGroup) =
        task {

            let! userEvents =
                eventStore.ReadEvents command.UserId
            
            let user =
                userEvents |> aggregate.exec None
            
            let newEvents =
                aggregate.handle user (RemoveFromGroup command)

            match newEvents with 
            | Ok newEvents ->
                return!
                    newEvents
                    |> storeUserEvents eventStore command.UserId
            | Error e ->
                return Error e
        }


    let get (eventStore:UserEventStore) id =
        task {
            let! events = eventStore.ReadEvents id
            return events |> aggregate.exec None
        }


    let checkPassword user password =
        Domain.Types.User.PasswordHash.isValid password user.PasswordHash
        


    type UserService = {
        GetUser:string -> Task<State option>
        CreateUser:(string -> bool)->CommandArguments.CreateUser->Task<Result<unit,Domain.Common.Errors>>
        DeleteUser:CommandArguments.DeleteUser->Task<Result<unit,Domain.Common.Errors>>
        ChangeEMail:(string -> bool)->CommandArguments.ChangeEMail->Task<Result<unit,Domain.Common.Errors>>
        ChangeName:CommandArguments.ChangeName->Task<Result<unit,Domain.Common.Errors>>
        ChangePassword:CommandArguments.ChangePassword->Task<Result<unit,Domain.Common.Errors>>
        AddToGroup:CommandArguments.AddToGroup->Task<Result<unit,Domain.Common.Errors>>
        RemoveFromGroup:CommandArguments.RemoveFromGroup->Task<Result<unit,Domain.Common.Errors>>
    }


    let createUserService userEventStore =
        {
            GetUser = get userEventStore
            CreateUser = create userEventStore
            DeleteUser = delete userEventStore
            ChangeEMail = changeEMail userEventStore
            ChangeName = changeName userEventStore
            ChangePassword = changePassword userEventStore
            AddToGroup = addToGroup userEventStore
            RemoveFromGroup = removeFromGroup userEventStore
        }

    


   
        

    
