namespace Services


module Common =

    open FSharp.Control.Tasks.V2
    open CosmoStore
    open CosmoStore.TableStorage
    open Microsoft.WindowsAzure.Storage

    let connection = "UseDevelopmentStorage=true;"
    //let connection = "DefaultEndpointsProtocol=https;AccountName=hardtstatictest1;AccountKey=2ytSulb0ifa7EFeInkMV+GkGXphrrwNNA3I9Hhb6JQT2khLwJBm8K/dcUWIJNxfVUr5voZbIpmicf2p0bCDeiw==;EndpointSuffix=core.windows.net"

    let private tableName = "IssueTrackerEventSource"

    let eventStore =
        task {
            let account = CloudStorageAccount.Parse(connection)
            let authKey = account.Credentials.ExportBase64EncodedKey()
            let config = 
                if (connection.StartsWith("UseDevelopmentStorage")) then
                    TableStorage.Configuration.CreateDefaultForLocalEmulator ()
                else
                    TableStorage.Configuration.CreateDefault account.Credentials.AccountName authKey            
            let config = { config with TableName = tableName }            
            return TableStorage.EventStore.getEventStore config
        }

    let getEventStore () = eventStore

