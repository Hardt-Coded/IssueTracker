module Server

open Saturn
open System
open Config
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection

open Common.Projections
open Users.EventStore
open Users.Services
open Users.Projections


let endpointPipe = pipeline {
    plug head
    plug requestId
}

let app = application {
    pipe_through endpointPipe

    error_handler (fun ex _ -> pipeline { render_html (InternalError.layout ex) })
    use_router Router.appRouter
    force_ssl
    use_antiforgery

    use_developer_exceptions

    url "https://0.0.0.0:44300/"

    use_cookies_authentication_with_config (fun opts -> 
        //opts.LoginPath <- PathString "/login"
        opts.ExpireTimeSpan <- TimeSpan(1,0,0)
        ()
    )

    service_config (fun services ->

        let eventStoreConnection = "UseDevelopmentStorage=true;"
        let tableName = "IssueTrackerEventSource"
        let ulpStorageFileName = "userlist.json"

        let getEventStore = Common.Infrastructure.EventStore.eventStore tableName eventStoreConnection

            
        // event store
        services.AddSingleton<UserEventStore>(fun sp ->
            createUserEventStore getEventStore "User"
        ) |> ignore

        // user services
        services.AddSingleton<UserService>(fun sp ->
            createUserService(sp.GetService<UserEventStore>())
        ) |> ignore

        // user list projection
        services.AddSingleton<Users.Projections.UserList.UserListProjection>(fun sp -> 
            UserList.UserListProjection(sp.GetService<UserEventStore>(),
                (fun e -> ()),
                (fun () -> async { return FileStorage.loadProjection ulpStorageFileName }),
                (fun list -> async { return FileStorage.storeProjection ulpStorageFileName list })
            ) 
        )
    )

    

    memory_cache
    use_static "static"
    use_gzip
    use_config (fun _ -> {connectionString = "DataSource=database.sqlite"} ) //TODO: Set development time configuration
}

[<EntryPoint>]
let main _ =
    printfn "Working directory - %s" (System.IO.Directory.GetCurrentDirectory())
    run app
    0 // return an integer exit code