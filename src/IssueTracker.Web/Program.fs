module Server

open Saturn
open System
open Config
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection

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

        let getEventStore = Infrastructure.EventStore.eventStore tableName eventStoreConnection

       
            
        // event store
        services.AddSingleton<Infrastructure.EventStore.User.UserEventStore>(fun sp ->
            Infrastructure.EventStore.User.createUserEventStore getEventStore "User"
        ) |> ignore

        // user services
        services.AddSingleton<Services.User.UserService>(fun sp ->
            Services.User.createUserService(sp.GetService<Infrastructure.EventStore.User.UserEventStore>())
        ) |> ignore

        // user list projection
        services.AddSingleton<Projections.UserList.UserListProjection>(fun sp -> 
            Projections.UserList.UserListProjection(sp.GetService<Infrastructure.EventStore.User.UserEventStore>(),
                (fun e -> ()),
                (fun () -> async { return Projections.FileStorage.loadProjection ulpStorageFileName }),
                (fun list -> async { return Projections.FileStorage.storeProjection ulpStorageFileName list })
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