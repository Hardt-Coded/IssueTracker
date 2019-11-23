module Router

open Saturn
open Giraffe.Core
open Giraffe.ResponseWriters
open Microsoft.AspNetCore.Authentication.Cookies


let browser = pipeline {
    plug acceptHtml
    plug putSecureBrowserHeaders
    plug fetchSession
    set_header "x-pipeline-type" "Browser"
}

let securedPipeline = pipeline {
    plug acceptHtml
    plug putSecureBrowserHeaders
    plug fetchSession
    set_header "x-pipeline-type" "Secured"
    requires_authentication (fun next ctx -> printfn "whoops; in auth"; redirectTo false "/login" next ctx)
}

let securedView = router {
    pipe_through securedPipeline
    get "/" (fun next ctx -> htmlView (Index.layout ctx) next ctx)    
    get "/index.html" (redirectTo false "/")
    get "/default.html" (redirectTo false "/")
    forward "/users" (fun next ctx -> UserAdminController.userController next ctx)
}

let defaultView = router {
    get "/" securedView
    get "/index.html" securedView
    get "/default.html" securedView

    get "/users" (fun next ctx -> securedView next ctx)    
    forward "/login" (fun next ctx -> UserLoginController.loginController next ctx)
    get "/logout" UserLoginController.logoutRouter
    
    
}



let browserRouter = router {
    not_found_handler (htmlView NotFound.layout) //Use the default 404 webpage
    pipe_through browser //Use the default browser pipeline

    forward "" defaultView //Use the default view
}

let appRouter = router {
    forward "" browserRouter
}




let myLazyCalc = 
    lazy(
        let r = 5
        let l = 7
        printfn "ich werde berechnet!"
        r + l
    )

myLazyCalc.Force()
myLazyCalc.Force()


