namespace Services

open FSharp.Control.Tasks.V2
open Microsoft.Azure.Cosmos.Table
open Dtos.User

module Common =

    let connection = "UseDevelopmentStorage=true;"

    let tableName = "IssueTrackerEventSource"
    
    let createAzureTable () =
        task {
            let storageAccount = CloudStorageAccount.Parse(connection)
            let client = storageAccount.CreateCloudTableClient()
            let cloudTable = client.GetTableReference(tableName)
            let! _ = cloudTable.CreateIfNotExistsAsync()
            return cloudTable
        }



module User =
    
    open System
    open Domain.User
    open Dtos.User.Commands    
    open Infrastructure

    let aggregateName = "User"


    let createUser (commandDto:CreateUser) = 
        task {
            let command : CommandArguments.CreateUser = {
                UserId = Guid.NewGuid().ToString("N")
                Name = commandDto.Name
                EMail = commandDto.EMail
                Password = commandDto.Password
            }

            let! eventTable = Common.createAzureTable ()
            // todo: vaidation, if user already exisits
            let events = aggregate.handle None (CreateUser command)
            match events with
            | Ok events ->
                let! versionResult =                     
                    events
                    |> List.map (fun i -> Dtos.User.Events.toDto i)
                    |> EventStore.storeEvents eventTable aggregateName command.UserId

                return versionResult
            | Error e ->
                return Error e
        }

    let deleteUser (commandDto:DeleteUser) =
        task {
            let command : CommandArguments.DeleteUser = {
                UserId = commandDto.UserId
            }

            let! eventTable = Common.createAzureTable ()

            let! userEvents =
                EventStore.User.readEvents eventTable aggregateName 1 command.UserId
            
            let user =
                userEvents |> aggregate.exec None
    
            match user with
            | None ->
                return failwith "user does not exisit"
            | Some state ->
                let newEvents =
                    aggregate.handle user (DeleteUser command)

                match newEvents with 
                | Ok newEvents ->
                    let! versionResult =
                        newEvents
                        |> List.map (fun i -> Dtos.User.Events.toDto i)
                        |> EventStore.storeEvents eventTable aggregateName command.UserId 

                    return versionResult
                | Error e ->
                    return Error e
        }


    let getUser id =
        // da14c2df38c941709b8d4add2fe52c65
        task {
            let! eventTable = Common.createAzureTable ()
            let! events = EventStore.User.readEvents eventTable aggregateName 1 id
            return events |> aggregate.exec None
        }
        




    

        


