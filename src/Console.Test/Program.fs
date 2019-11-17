// Learn more about F# at http://fsharp.org

open System
open Dtos.User.Commands


[<EntryPoint>]
let main argv =
    

    //for i in 1..100 do
    //    let testUser = {
    //        EMail = sprintf "muh%03d@test.de" i
    //        Name = sprintf "Einhorni %3d" i
    //        Password = "test1234"
    //    }

    //    let version = Services.User.createUser testUser |> Async.AwaitTask |> Async.RunSynchronously
    //    printfn "%A" version
        


    
    

    let user = Services.User.getUser "01ff7d19a9f4459aaf91b73d381f030d" |> Async.AwaitTask |> Async.RunSynchronously
    printfn "%A" user
    
    let version = Services.User.deleteUser { UserId = "01ff7d19a9f4459aaf91b73d381f030d" } |> Async.AwaitTask |> Async.RunSynchronously
    printfn "%A" version

    let user = Services.User.getUser "01ff7d19a9f4459aaf91b73d381f030d" |> Async.AwaitTask |> Async.RunSynchronously
    printfn "%A" user


    //Console.ReadLine() |> ignore
    0 // return an integer exit code
