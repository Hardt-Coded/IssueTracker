// Learn more about F# at http://fsharp.org
open Domain.Common
open FSharp.Control.Tasks
open Terminal.Gui.Elmish
open Projections.UserList
open Domain.Types.Common


let mutable userListProjection : UserListProjection option = None


type UserFormData = 
    {
        Name:string
        EMail:string
        Password:string          
        Group:string
    }
    with 
        static member Empty = {
            Name = ""
            EMail = ""
            Password = ""            
            Group = ""
        }


type Form =
    | ChangeEMailForm
    | ChangePasswordForm
    | AddToGroupForm
    | RemoveFromGroupForm
    | ErrorForm
    | SuccessfulForm
    | NoForm

type Model = {
    Users:Projections.UserList.State list
    UserId:string
    CurrentUser: Projections.UserList.State option

    NewUserData:UserFormData
    Form:Form
    Error:string
}



type Msg = 
    | LoadUsers
    | UsersLoaded of State list 
    | ChangeSelectedUser of string

    | StartChangeEMail
    | StartChangePassword
    | StartAddToGroup
    | StartRemoveFromGroup


    | ChangeEMail of string
    | ChangePassword of string
    | AddToGroup of string
    | RemoveFromGroup of string

    | ChangeEMailText of string
    | ChangePasswordText of string
    | AddToGroupText of string
    | RemoveFromGroupText of string

    | CancelForm
    | Successfull

    | OnError of Domain.Common.Errors

    | Nothing



let printError e =
    match e with
    | DomainError s ->
        printfn "Error: %s" s
    | InfrastructureError ex ->
        printfn "Exception: %s" ex.Message
    



let init () =
    { 
        Users= [] 
        UserId = ""
        CurrentUser = None
        NewUserData = UserFormData.Empty
        Form = NoForm
        Error = ""
        
    }, Cmd.none


let userListProjectionCmd () = 
    task {
        match userListProjection with
        | None ->
            return Nothing
        | Some up ->            
            let! users = up.GetUserList()
            return UsersLoaded users
    } |> Cmd.OfTask.result


let updateUserListProjectionCmd ()  = 
    match userListProjection with
    | None ->
        Nothing
    | Some up ->            
        up.UpdateProjection ()
        Nothing
    |> Cmd.OfFunc.result


let update msg model =
    match msg with
    | LoadUsers ->
        model, userListProjectionCmd ()
    | UsersLoaded users -> 
        let users = users |> List.sortBy (fun i -> i.Name)
        { model with Users = users}, Cmd.none
    | ChangeSelectedUser userId ->
        let user = model.Users |> List.find (fun i -> i.UserId = userId)
        { model with UserId = userId; CurrentUser = Some user}, Cmd.none

    | StartChangeEMail ->
        let newUserData = { UserFormData.Empty with EMail = model.CurrentUser |> Option.map (fun i -> i.EMail) |> Option.defaultValue "" }
        { model with Form = ChangeEMailForm; NewUserData = newUserData }, Cmd.none
    | StartChangePassword->
        { model with Form = ChangePasswordForm }, Cmd.none
    | StartAddToGroup->
        { model with Form = AddToGroupForm }, Cmd.none
    | StartRemoveFromGroup->
        { model with Form = RemoveFromGroupForm }, Cmd.none

    | ChangeEMail email->
        let changeEmailCmd =
            task {
                let command:Dtos.User.Commands.ChangeEMail = {
                    UserId = model.UserId
                    EMail = email
                }
                let! res = Services.User.userService.ChangeEMail command
                match res with
                | Error e ->
                    return (OnError e)
                | Ok _ ->
                    return Successfull
            } |> Cmd.OfTask.result
        model, changeEmailCmd
    | ChangePassword pw ->
        let changePaswordCmd =
            task {
                let command:Dtos.User.Commands.ChangePassword = {
                    UserId = model.UserId
                    Password = pw
                }
                let! res = Services.User.userService.ChangePassword command
                match res with
                | Error e ->
                    return (OnError e)
                | Ok _ ->
                    return Successfull
            } |> Cmd.OfTask.result
        model, changePaswordCmd
    | AddToGroup group ->
        let addToGroupCmd =
            task {
                let command:Dtos.User.Commands.AddToGroup = {
                    UserId = model.UserId
                    Group = group
                }
                let! res = Services.User.userService.AddToGroup command
                match res with
                | Error e ->
                    return (OnError e)
                | Ok _ ->
                    return Successfull
            } |> Cmd.OfTask.result
        model, addToGroupCmd
    | RemoveFromGroup group ->
        let addToGroupCmd =
            task {
                let command:Dtos.User.Commands.RemoveFromGroup = {
                    UserId = model.UserId
                    Group = group
                }
                let! res = Services.User.userService.RemoveFromGroup command
                match res with
                | Error e ->
                    return (OnError e)
                | Ok _ ->
                    return Successfull
            } |> Cmd.OfTask.result
        model, addToGroupCmd

    | ChangeEMailText email->
        { model with NewUserData = { model.NewUserData with EMail = email } }, Cmd.none
    | ChangePasswordText pw ->
        { model with NewUserData = { model.NewUserData with Password = pw } }, Cmd.none
    | AddToGroupText group ->
        { model with NewUserData = { model.NewUserData with Group = group } }, Cmd.none
    | RemoveFromGroupText group ->
        { model with NewUserData = { model.NewUserData with Group = group } }, Cmd.none

    | CancelForm ->
        { model with NewUserData = UserFormData.Empty; Form = NoForm }, Cmd.ofMsg LoadUsers
    | Successfull ->
        
        { model with NewUserData = UserFormData.Empty; Form = SuccessfulForm }, updateUserListProjectionCmd ()

    | OnError error ->
        let error =
            match error with
            | DomainError de ->
                sprintf "Domain Error: %s" de
            | InfrastructureError ie ->
                sprintf "Infrastructure Error: %s" ie.Message

        { model with Error = error; Form = ErrorForm}, Cmd.none

    | Nothing ->
        model, Cmd.none



open System 


let userToListItem user =
    let groups = String.Join(",",user.Groups)
    user.UserId, sprintf "%s - %s - %s - %s (%i) - %s" user.UserId user.Name user.EMail groups user.Version (user.PasswordHash.[1..8])


let formWindow title content =
    window [
        Styles [
            Pos (AbsPos 5,AbsPos 5)
            Dim (Dimension.FillMargin 25 ,Dimension.FillMargin 5)
        ]
        Title title
    ] [
        yield! content    
    ]

let changeTextForm description value isSecret message saveMessage dispatch : Terminal.Gui.View list =
    [
        label [
            Styles [
                Pos (AbsPos 1,AbsPos 5)
                Dim (AbsDim 14,AbsDim 1)                
            ]
            Text description
        ]

        textField [
            Styles [
                Pos (AbsPos 14,AbsPos 5)
                Dim (Fill,AbsDim 1)

            ]
            Value value
            OnChanged (fun t -> dispatch (message t))
            if isSecret then Secret
        ]

        button [
            Styles [
                Pos (AbsPos 1,AbsPos 10)
                Dim (AbsDim 14,AbsDim 1)                
            ]
            Text "Cancel"
            OnClicked (fun () -> dispatch (CancelForm))
        ]

        button [
            Styles [
                Pos (AbsPos 30,AbsPos 10)
                Dim (AbsDim 14,AbsDim 1)                
            ]
            Text "Ok"
            OnClicked (fun () -> dispatch (saveMessage value))
        ]
    ]


let changeEMailForm model dispatch =
    formWindow 
        "Change E-Mail" 
        (changeTextForm "New EMail" model.NewUserData.EMail false ChangeEMailText ChangeEMail dispatch)


let changePasswordForm model dispatch =
    formWindow 
        "Change Password" 
        (changeTextForm "Password" model.NewUserData.Password true ChangePasswordText ChangePassword dispatch)


let addToGroupForm model dispatch =
    formWindow 
        "Add To Group" 
        (changeTextForm "Gruppenname" model.NewUserData.Group false AddToGroupText AddToGroup dispatch)


let removeFromGroupForm model dispatch =
    formWindow 
        "Remove From Group" 
        (changeTextForm "Gruppenname" model.NewUserData.Group false RemoveFromGroupText RemoveFromGroup dispatch)

let errorForm model dispatch =
    formWindow 
        "Error!!!!"
        [
            label [
                Styles [
                    Pos (AbsPos 1,AbsPos 5)
                    Dim (AbsDim 14,AbsDim 1)                
                ]
                Text "Error:"
            ]

            label [
                Styles [
                    Pos (AbsPos 14,AbsPos 5)
                    Dim (Fill,AbsDim 1)
                    Colors (Terminal.Gui.Color.Red,Terminal.Gui.Color.BrightYellow)
                ]
                Text model.Error
                
            ]

            button [
                Styles [
                    Pos (AbsPos 1,AbsPos 10)
                    Dim (AbsDim 14,AbsDim 1)                
                ]
                Text "Ok"
                OnClicked (fun () -> dispatch (CancelForm))
            ]
        ]


let successfulForm model dispatch =
    formWindow 
        "Successful"
        [
            label [
                Styles [
                    Pos (AbsPos 1,AbsPos 5)
                    Dim (AbsDim 14,AbsDim 1)                
                ]
                Text "Your Operation was successful!"
            ]

            button [
                Styles [
                    Pos (AbsPos 1,AbsPos 10)
                    Dim (AbsDim 14,AbsDim 1)                
                ]
                Text "Ok"
                OnClicked (fun () -> dispatch (CancelForm))
            ]
        ]
    


let mainSite model dispatch : Terminal.Gui.View list =
    [
        button [
            Styles [
                Pos (AbsPos 1, AbsPos 1)                        
            ]
            Text "Load Users"
            OnClicked (fun () -> dispatch (LoadUsers))
        ]

        if model.UserId <> "" then
            
            button [
                Styles [
                    Pos (AbsPos 20, AbsPos 2)                        
                ]
                Text "Change EMail"
                OnClicked (fun () -> dispatch (StartChangeEMail))
            ]

            button [
                Styles [
                    Pos (AbsPos 40, AbsPos 2)                        
                ]
                Text "Change Password"
                OnClicked (fun () -> dispatch (StartChangePassword))
            ]

            button [
                Styles [
                    Pos (AbsPos 60, AbsPos 2)                        
                ]
                Text "Add To Group"
                OnClicked (fun () -> dispatch (StartAddToGroup))
            ]

            button [
                Styles [
                    Pos (AbsPos 80, AbsPos 2)                        
                ]
                Text "Remove From Group"
                OnClicked (fun () -> dispatch (StartRemoveFromGroup))
            ]

        label [
            Styles [
                Pos (AbsPos 1, AbsPos 3)                        
            ]
            Text (sprintf "CurrentId: %s - %s" model.UserId (model.CurrentUser |> Option.map (fun i -> i.EMail) |> Option.defaultValue ""))                    
        ]

                
        listView [
            Styles [
                Pos (AbsPos 0,AbsPos 5)                
                Dim (Fill,Fill)

            ]
            Value model.UserId
            Items (model.Users |> List.map (userToListItem))
            OnChanged (fun r -> dispatch (ChangeSelectedUser r))
        ]
    ]
    


let view model dispatch =
    page [
        window [
            Styles [
                Pos (AbsPos 0,AbsPos 1)
                Dim (Fill,Fill)
            ]
            Title "Dingsbums Test"
        ] [
            match model.Form with
            | NoForm ->
                yield! mainSite model dispatch
            | ChangeEMailForm ->
                changeEMailForm model dispatch
            | ChangePasswordForm ->
                changePasswordForm model dispatch
            | AddToGroupForm ->
                addToGroupForm model dispatch
            | RemoveFromGroupForm ->
                removeFromGroupForm model dispatch
            | ErrorForm ->
                errorForm model dispatch
            | SuccessfulForm ->
                successfulForm model dispatch
        ]
    ]


[<EntryPoint>]
let main argv =
    
    printfn "Initiate app ..."

    userListProjection <- Some (UserListProjection(printError))

    Program.mkProgram init update view 
    //|> Program.withSubscription (fun _ -> Cmd.ofSub App.timerSubscription)
    |> Program.run


    
    0 // return an integer exit code
