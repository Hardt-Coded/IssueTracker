namespace Users

module Services =
    
    open System
    open System.Threading.Tasks
    open FSharp.Control.Tasks.V2    
    open Project.Domain
    open Common.Domain
    open Project.EventStore
    open Project.Dtos
    open Project.Types



    let private storeProjectEvents (eventStore:ProjectEventStore) id events =
        task {
            let! versionResult =                     
                events
                |> eventStore.StoreEvents id
            return versionResult
        }


    let private create (eventStore:ProjectEventStore) (command:CommandArguments.CreateProject) = 
        task {
            let events = aggregate.Handle None (CreateProject command)
            match events with
            | Ok events ->
                return!
                    events
                    |> storeProjectEvents eventStore command.ProjectId
            | Error e ->
                return Error e
        }



    type ProjectService = {
        CreateProject: CommandArguments.CreateProject->Task<Result<unit,Errors list>>
    }


    let createProjectService userEventStore =
        {
            CreateProject = create userEventStore
        }


