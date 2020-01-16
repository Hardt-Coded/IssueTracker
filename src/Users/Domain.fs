namespace Users

module Domain =

    open Common.Types
    open Users.Types
    open Common.Domain
    

    type State = {
        UserId:UserId
        Name:NoneEmptyString
        EMail:EMail
        PasswordHash:PasswordHash
        Groups: NoneEmptyString list
    }


    module CommandArguments =

        type CreateUser = {
            UserId:string
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

        type ChangeName = {
            UserId:string
            Name:string
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
        | ChangeName of CommandArguments.ChangeName
        | ChangePassword of CommandArguments.ChangePassword
        | AddToGroup of CommandArguments.AddToGroup
        | RemoveFromGroup of CommandArguments.RemoveFromGroup


    module EventArguments =

        type UserCreated = {
            UserId:UserId
            Name:NoneEmptyString
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

        type NameChanged = {
            UserId:UserId
            Name:NoneEmptyString
        }

        type PasswordChanged = {
            UserId:UserId
            PasswordHash:PasswordHash
        }

        type AddedToGroup = {
            UserId:UserId
            Group:NoneEmptyString
        }

        type RemovedFromGroup = {
            UserId:UserId
            Group:NoneEmptyString
        }


    type Event = 
        | UserCreated of EventArguments.UserCreated
        | UserDeleted of EventArguments.UserDeleted
        | EMailChanged of EventArguments.EMailChanged
        | NameChanged of EventArguments.NameChanged
        | PasswordChanged of EventArguments.PasswordChanged
        | AddedToGroup of EventArguments.AddedToGroup
        | RemovedFromGroup of EventArguments.RemovedFromGroup
        
        


    let (<*>) = Validation.apply 
    let (<!>) = Result.map


    let rec private handle (state:State option) command : Result<Event list,Errors list> =
        match state,command with
        | None, CreateUser args ->
            userCreated args
        | Some _, CreateUser _ ->
            "you can not have a create user event, when a user alread exists"
            |> DomainError
            |> List.singleton
            |> Error
        | Some state, DeleteUser args ->
            userDeleted args
        | Some _, ChangeEMail args ->
            emailChanged args 
        | Some _, ChangeName args ->
            nameChanged args 
        | Some state, ChangePassword args ->
            changePassword args
        | Some state, AddToGroup args ->
            addedToGroup state args
        | Some state, RemoveFromGroup args ->
            removedFromGroup state args
        | None, _ ->
            "user does not exists"
            |> DomainError
            |> List.singleton
            |> Error


    and userCreated args =
        let create userId name email passwordHash = 
            [
                UserCreated {
                    UserId = userId
                    Name = name
                    EMail = email
                    PasswordHash = passwordHash
                }
            ]

        let name = NoneEmptyString.create "Name" args.Name
        let email = EMail.create args.EMail
        let passwordHash = PasswordHash.create args.Password
        let userId = UserId.create args.UserId
        create <!> userId <*> name <*> email <*> passwordHash
        


    and userDeleted args =
        let create userId = [ UserDeleted { UserId = userId } ]
        let userId = UserId.create args.UserId
        create <!> userId

        


    and emailChanged args =
        let create userId email = [ EMailChanged { UserId = userId; EMail = email } ]

        let userId = UserId.create args.UserId
        let email = EMail.create args.EMail
        create <!> userId <*> email
        


    and nameChanged args =
        let create userId name = [ NameChanged { UserId = userId; Name = name } ]

        let userId = UserId.create args.UserId
        let name = NoneEmptyString.create "Name" args.Name
        create <!> userId <*> name
        


    and changePassword args =
        let create userId passwordHash = [ PasswordChanged { UserId = userId; PasswordHash = passwordHash } ]
        
        let userId = UserId.create args.UserId
        let passwordHash = PasswordHash.create args.Password
        create <!> userId <*> passwordHash
        


    and addedToGroup state args =
        let create userId group = [ AddedToGroup { UserId = userId; Group = group } ]

        let userId = UserId.create args.UserId
        let group = NoneEmptyString.create "Group" args.Group

        if (state.Groups |> List.map Ok |> List.exists (fun i -> i = group)) then
            sprintf "user already assigned to group %s" args.Group
            |> DomainError
            |> List.singleton
            |> Error
        else
            create <!> userId <*> group
        


    and removedFromGroup state args =
        let create userId group = [ RemovedFromGroup { UserId = userId; Group = group } ]

        let userId = UserId.create args.UserId
        let group = NoneEmptyString.create "Group" args.Group

        if (state.Groups |> List.map Ok |> List.exists (fun i -> i = group)) then
            create <!> userId <*> group
        else
            sprintf "user is not member of the group %s" args.Group
            |> DomainError
            |> List.singleton
            |> Error

                
       
    let private apply (state:State option) event : State option =
        match state, event with
        | None, UserCreated args ->
            { 
                UserId = args.UserId
                EMail = args.EMail
                Name = args.Name
                PasswordHash = args.PasswordHash
                Groups = []
            } |> Some
        | Some _, UserCreated args ->
            failwith "a create event is invalid, when a user state already exisits"
        | Some _, UserDeleted _ ->
            None
        | Some state, EMailChanged args ->
            { state with
                EMail = args.EMail } 
            |> Some
        | Some state, NameChanged args ->
            { state with
                Name = args.Name } 
            |> Some
        | Some state, PasswordChanged args ->
            { state with
                PasswordHash = args.PasswordHash } 
            |> Some
        | Some state, AddedToGroup args ->
            { state with
                Groups = args.Group :: state.Groups } 
            |> Some
        | Some state, RemovedFromGroup args ->
            let newGroups =
                state.Groups
                |> List.filter (fun i -> i<> args.Group)

            { state with
                Groups = newGroups } 
            |> Some
        | None, _ ->
            sprintf "can not apply the event of type %s to the state" (event.GetType().Name)
            |> failwith
            


    let private exec = exec apply
    let private execWithVersion = execWithEvents apply

    let aggregate : Aggregate<_,_,_,Errors list> = {
        Apply = apply
        Handle = handle
        Exec = exec
        ExecWithVersion = execWithVersion
    }
            





