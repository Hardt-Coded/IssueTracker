// Learn more about F# at http://fsharp.org

open System
open Dtos.User.Commands


[<EntryPoint>]
let main argv =
    
    //let testUser = {
    //    EMail = "muh@test.de"
    //    Name = "Einhorni"
    //    Password = "test1234"
    //}

    //let version = Services.User.createUser testUser |> Async.AwaitTask |> Async.RunSynchronously

    //printfn "%A" version


    let user = Services.User.getUser "4ed44f214eb24a74b95fbed696679d6a" |> Async.AwaitTask |> Async.RunSynchronously
    printfn "%A" user
    
    let version = Services.User.deleteUser { UserId = "4ed44f214eb24a74b95fbed696679d6a" } |> Async.AwaitTask |> Async.RunSynchronously
    printfn "%A" version

    let user = Services.User.getUser "4ed44f214eb24a74b95fbed696679d6a" |> Async.AwaitTask |> Async.RunSynchronously
    printfn "%A" user


    //Console.ReadLine() |> ignore
    0 // return an integer exit code
