// Learn more about F# at http://fsharp.org
open Domain.Common
open FSharp.Control.Tasks
open Terminal.Gui.Elmish
open Projections.UserList

type Model = {
    Users:Projections.UserList.State list
    UserListProjection:UserListProjection option
    IsLoading:bool
}

type Msg = 
    | LoadUsers
    | UsersLoaded of State list
    | InitProjection of UserListProjection
    | ProjectionInitiated
    | Nothing



let printError e =
    match e with
    | DomainError s ->
        printfn "Error: %s" s
    | InfrastructureError ex ->
        printfn "Exception: %s" ex.Message


let initProjectionCmd =
    fun dispatch ->
        let p = UserListProjection(printError, (fun () -> dispatch ProjectionInitiated))
        dispatch (InitProjection p)
    |> Cmd.ofSub


let init () =
    { 
        Users= [] 
        UserListProjection = None
        IsLoading = true
    }, initProjectionCmd


let userListProjectionCmd (model: Model) = 
    task {
        match model.UserListProjection with
        | None ->
            return Nothing
        | Some up ->
            let! users = up.GetUserList()
            return UsersLoaded users
    } |> Cmd.OfTask.result


let update msg model =
    match msg with
    | LoadUsers ->
        model, (userListProjectionCmd model)
    | UsersLoaded users ->
        let users = users |> List.sortBy (fun i -> i.Name)
        { model with Users = users}, Cmd.none
    | InitProjection p ->
        { model with UserListProjection = Some p }, Cmd.none
    | ProjectionInitiated ->
        { model with IsLoading = false }, Cmd.none
    | Nothing ->
        model, Cmd.none



open System 

let userToListItem user =
    let groups = String.Join(",",user.Groups)
    user.UserId, sprintf "%s - %s - %s - %s (%i)" user.UserId user.Name user.EMail groups user.Version

let view model dispatch =
    page [
        window [
            Styles [
                Pos (AbsPos 0,AbsPos 1)
                Dim (Fill,Fill)
            ]
        ] [
            if model.IsLoading then
                label [
                    Styles [
                        Pos (AbsPos 1, AbsPos 1)                        
                    ]
                    Text "init projection please wait"
                ]

            else

                button [
                    Styles [
                        Pos (AbsPos 1, AbsPos 1)                        
                    ]
                    Text "Load Users"
                    OnClicked (fun () -> dispatch (LoadUsers))
                ]

            
                listView [
                    Styles [
                        Pos (AbsPos 0,AbsPos 5)                
                        Dim (Fill,Fill)

                    ]
                    Value ""
                    Items (model.Users |> List.map (userToListItem))
                ]
        ]
    ]



[<EntryPoint>]
let main argv =
    

    Program.mkProgram init update view 
    //|> Program.withSubscription (fun _ -> Cmd.ofSub App.timerSubscription)
    |> Program.run


    let runParallel max items = Async.Parallel (items,max)

    //let res =
    //    [1..500]
    //    |> List.map (fun i ->
    //        async {
    //            let testUser = {
    //                EMail = sprintf "muh%03d@test.de" i
    //                Name = sprintf "Einhorni %3d" i
    //                Password = "test1234"
    //            }
    //            printf "+"
    //            let! version = Services.User.createUser testUser |> Async.AwaitTask
    //            return version
    //        }
    //    )
    //    |> runParallel 8
    //    |> Async.RunSynchronously
        
    //printfn ""

    ////let user = Services.User.getUser "01ff7d19a9f4459aaf91b73d381f030d" |> Async.AwaitTask |> Async.RunSynchronously
    ////printfn "%A" user
    
    ////let version = Services.User.deleteUser { UserId = "01ff7d19a9f4459aaf91b73d381f030d" } |> Async.AwaitTask |> Async.RunSynchronously
    ////printfn "%A" version

    ////let user = Services.User.getUser "01ff7d19a9f4459aaf91b73d381f030d" |> Async.AwaitTask |> Async.RunSynchronously
    ////printfn "%A" user

    //let printError e =
    //    match e with
    //    | DomainError s ->
    //        printfn "Error: %s" s
    //    | InfrastructureError ex ->
    //        printfn "Exception: %s" ex.Message


    //let userListProjection = UserListProjection(printError)

    //let currentUserList =
    //    userListProjection.GetUserList() |> Async.AwaitTask |> Async.RunSynchronously

    //printfn "%A" currentUserList


    // "fa42b9176a994f32978ec7735a44f64a"

    //Console.ReadLine() |> ignore
    0 // return an integer exit code
