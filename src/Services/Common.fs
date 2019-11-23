namespace Services


module Common =

    open FSharp.Control.Tasks.V2
    open CosmoStore
    open CosmoStore.TableStorage
    open Microsoft.WindowsAzure.Storage

    let connection = "UseDevelopmentStorage=true;"
    //let connection = "DefaultEndpointsProtocol=https;AccountName=hardtstatictest1;AccountKey=2ytSulb0ifa7EFeInkMV+GkGXphrrwNNA3I9Hhb6JQT2khLwJBm8K/dcUWIJNxfVUr5voZbIpmicf2p0bCDeiw==;EndpointSuffix=core.windows.net"

    let private tableName = "IssueTrackerEventSource"


    
        

    

