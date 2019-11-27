namespace Services


module Common =

    open FSharp.Control.Tasks.V2
    open CosmoStore
    open CosmoStore.TableStorage
    open Microsoft.WindowsAzure.Storage

    let connection = "UseDevelopmentStorage=true;"

    let private tableName = "IssueTrackerEventSource"


    
        

    

