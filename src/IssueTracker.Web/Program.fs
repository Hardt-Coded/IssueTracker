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

open Project.EventStore
open Project.Services
open Project.Projections

open Microsoft.Extensions.Configuration


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
        opts.ExpireTimeSpan <- TimeSpan(1,0,0)
        ()
    )

    service_config (fun services ->

        let ulpStorageFileName = "userlist.json"
        let plpStorageFileName = "projectlist.json"


        let getEventStoreConnection (sp:IServiceProvider) =
            let config = sp.GetService<IConfiguration>()
            let eventStoreConnection = config.GetSection("EventStore").GetValue("Connection")
            let tableName = config.GetSection("EventStore").GetValue("TableName")
            (eventStoreConnection,tableName)

        
            
            
        // event store
        services.AddSingleton<UserEventStore>(fun sp ->
            let (eventStoreConnection,tableName) = getEventStoreConnection sp
            let getEventStore = Common.Infrastructure.EventStore.eventStore tableName eventStoreConnection
            Users.EventStore.createUserEventStore getEventStore "User"
        ) |> ignore

        services.AddSingleton<ProjectEventStore>(fun sp ->
            let (eventStoreConnection,tableName) = getEventStoreConnection sp
            let getEventStore = Common.Infrastructure.EventStore.eventStore tableName eventStoreConnection
            Project.EventStore.createUserEventStore getEventStore "Project"
        ) |> ignore

        // user services
        services.AddSingleton<UserService>(fun sp ->
            createUserService(sp.GetService<UserEventStore>())
        ) |> ignore

        // user services
        services.AddSingleton<ProjectService>(fun sp ->
            createProjectService(sp.GetService<ProjectEventStore>())
        ) |> ignore

        // user list projection
        services.AddSingleton<Users.Projections.UserList.UserListProjection>(fun sp -> 
            UserList.UserListProjection(sp.GetService<UserEventStore>(),
                (fun e -> 
                    e 
                    |> List.iter (fun i -> 
                        match i with 
                        | Common.Domain.Errors.DomainError de -> failwith de
                        | Common.Domain.Errors.InfrastructureError exn -> raise exn
                     )
                ),
                (fun () -> async { return FileStorage.loadProjection ulpStorageFileName }),
                (fun list -> async { return FileStorage.storeProjection ulpStorageFileName list })
            ) 
        ) |> ignore


        // project list projection
        services.AddSingleton<Project.Projections.ProjectList.ProjectListProjection>(fun sp -> 
            ProjectList.ProjectListProjection(sp.GetService<ProjectEventStore>(),
                (fun e -> 
                    e 
                    |> List.iter (fun i -> 
                        match i with 
                        | Common.Domain.Errors.DomainError de -> failwith de
                        | Common.Domain.Errors.InfrastructureError exn -> raise exn
                     )
                ),
                (fun () -> async { return FileStorage.loadProjection plpStorageFileName }),
                (fun list -> async { return FileStorage.storeProjection plpStorageFileName list })
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