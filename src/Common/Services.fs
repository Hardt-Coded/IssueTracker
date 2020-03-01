namespace Common


module Services =

    let connection = "UseDevelopmentStorage=true;"

    let private tableName = "IssueTrackerEventSource"

    open FSharp.Control.Tasks.V2
    open Common.Domain
    open Common.Infrastructure.EventStore


    let inline private storeProjectEvents (eventStore:EventStoreService<'event>) id events =
        task {
            let! versionResult =                     
                events
                |> eventStore.StoreEvents id
            return versionResult
        }



    let inline handleCommand (aggregate:Aggregate<'state,'cmd,'event,Errors list>) (eventStore:EventStoreService<'event>) (getAggregaeId:'cmd->string) (state:'state option) (cmd:'cmd) =
        task {
            let events = aggregate.Handle state cmd
            match events with
            | Ok events ->
                return!
                    events
                    |> storeProjectEvents eventStore (getAggregaeId cmd)
            | Error e ->
                return Error e
        }


    
        

    

