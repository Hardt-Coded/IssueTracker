module Index

open Giraffe.GiraffeViewEngine
open Microsoft.AspNetCore.Http
open System.Security.Claims
open Microsoft.AspNetCore.Authentication

let index (ctx:HttpContext) =
    [
        div [ ] [
            h1 [ ] [ encodedText "Arg! Aaaaahaaa!" ]
        ]

        if (ctx.User.Identity.IsAuthenticated) then
            let email = ctx.User.FindFirst(ClaimTypes.Email)
            h1 [ ] [ encodedText (sprintf "Hello %s (%s)" ctx.User.Identity.Name email.Value ) ]
    ]

let layout ctx =
    App.layout ctx (index ctx)