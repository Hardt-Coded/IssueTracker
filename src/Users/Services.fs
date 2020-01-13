namespace Users

module Services =
    
    open System
    open System.Threading.Tasks
    open FSharp.Control.Tasks.V2    
    open Users.Domain
    open Common.Domain
    open Users.EventStore
    open Users.Dtos
    open Users.Types

    let aggregateName =  "User"


    type CheckEMailDuplication = string -> bool
    

    let private storeUserEvents (eventStore:UserEventStore) id events =
        task {
            let! versionResult =                     
                events
                |> List.map (fun i -> Events.toDto i)
                |> eventStore.StoreEvents id
            return versionResult
        }


    let private create (eventStore:UserEventStore) (checkEMailDuplicate:CheckEMailDuplication) (command:CommandArguments.CreateUser) = 
        task {

            if (checkEMailDuplicate command.EMail) then
                return DomainError "email already in use" |> Error
            else
                let events = aggregate.Handle None (CreateUser command)
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
                userEvents |> aggregate.Exec None
            
            let newEvents =
                aggregate.Handle user (DeleteUser command)

            match newEvents with 
            | Ok newEvents ->
                return!
                    newEvents
                    |> storeUserEvents eventStore command.UserId
            | Error e ->
                return Error e
        }


    let changeEMail (eventStore:UserEventStore) (checkEMailDuplicate:CheckEMailDuplication) (command:CommandArguments.ChangeEMail) =
        task {

            if (checkEMailDuplicate command.EMail) then
                return DomainError "email already in use" |> Error
            else
                let! userEvents =
                    eventStore.ReadEvents command.UserId
            
                let user =
                    userEvents |> aggregate.Exec None
            
                let newEvents =
                    aggregate.Handle user (ChangeEMail command)

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
                userEvents |> aggregate.Exec None
            
            let newEvents =
                aggregate.Handle user (ChangeName command)

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
                userEvents |> aggregate.Exec None
            
            let newEvents =
                aggregate.Handle user (ChangePassword command)

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
                userEvents |> aggregate.Exec None
            
            let newEvents =
                aggregate.Handle user (AddToGroup command)

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
                userEvents |> aggregate.Exec None
            
            let newEvents =
                aggregate.Handle user (RemoveFromGroup command)

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
            return events |> aggregate.Exec None
        }


    let checkPassword user password =
        PasswordHash.isValid password user.PasswordHash
        


    

    type UserService = {
        GetUser:string -> Task<User option>
        CreateUser: CheckEMailDuplication->CommandArguments.CreateUser->Task<Result<unit,Errors>>
        DeleteUser:CommandArguments.DeleteUser->Task<Result<unit,Errors>>
        ChangeEMail:CheckEMailDuplication->CommandArguments.ChangeEMail->Task<Result<unit,Errors>>
        ChangeName:CommandArguments.ChangeName->Task<Result<unit,Errors>>
        ChangePassword:CommandArguments.ChangePassword->Task<Result<unit,Errors>>
        AddToGroup:CommandArguments.AddToGroup->Task<Result<unit,Errors>>
        RemoveFromGroup:CommandArguments.RemoveFromGroup->Task<Result<unit,Errors>>
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

    


   
        

    
