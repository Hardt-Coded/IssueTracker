// Learn more about F# at http://fsharp.org

open System
open Users.Dtos
open FSharp.Control.Tasks.V2
open Users.Domain
open Users.Domain.CommandArguments


[<EntryPoint>]
let main argv =
    

    let userId = System.Guid.NewGuid().ToString("N")

    let createUserCommand =
        match argv with
        | [||] ->
            {
                UserId = userId
                Name = "Admin"
                EMail = "admin@admin.net"
                Password = "admin"
            }
        | [| username |] ->
            {
                UserId = userId
                Name = username
                EMail = "admin@admin.net"
                Password = "admin"
            }
        | [| username; password |] ->
            {
                UserId = userId
                Name = username
                EMail = "admin@admin.net"
                Password = password
            } 
        | _ ->
            failwith "max 2 arguments needed. username and password."

       
    let addToGroupCommand:AddToGroup =
        {
            UserId = userId
            Group = "admin"
        }
       
    
    let getEventStore = Common.Infrastructure.EventStore.eventStore "IssueTrackerEventSource" Common.Services.connection
    let userEventStore = Users.EventStore.createUserEventStore getEventStore "User"
    let userService = Users.Services.createUserService userEventStore


    let mutable userList = []    

    let projection = 
        Users.Projections.UserList.UserListProjection(userEventStore,
            (fun e -> ()),
            (fun () -> Async.retn userList),
            (fun ul -> userList <- ul; Async.retn ())
        )

    let createEMailDuplicateValidation (userListProjection:Users.Projections.UserList.UserListProjection) =
        task {
            let! userList = userListProjection.GetUserList ()
            let isEMailDuplicate = 
                fun email -> 
                    userList 
                    |> List.exists (fun i -> String.Equals(i.EMail,email,StringComparison.InvariantCultureIgnoreCase))
            return isEMailDuplicate
        }

    let createAdminUserTask = 
        task {
            let! isDuplicate = createEMailDuplicateValidation projection
            let! result = userService.CreateUser isDuplicate createUserCommand            
            match result with
            | Error e ->
                failwith (sprintf "%A" e)
            | Ok _ ->
                let! result = userService.AddToGroup addToGroupCommand
                match result with
                | Error e ->
                    failwith (sprintf "%A" e)
                | Ok _ ->
                    ()
                ()
        }

    createAdminUserTask.Result

    printfn "Admin User Created"
    0 // return an integer exit code
