module UserLoginController

    open Saturn
    open FSharp.Control.Tasks.V2
    open Microsoft.AspNetCore
    open Microsoft.AspNetCore.Http
    open Microsoft.Extensions.DependencyInjection
    open UserLoginModel
    open Users.Projections.UserList
    open Common.Types
    open Users.Domain
    open Users.Types
    open Users.Services
    open Common.Domain
    open System.Security.Claims
    open Microsoft.AspNetCore.Authentication
    open Saturn.ControllerHelpers
    open Giraffe
    open Microsoft.AspNetCore.Authentication.Cookies


    let private renderLoginPage ctx model =
        Controller.renderHtml ctx (UserLoginView.loginLayout ctx model)


    let private generateClaimPrincipal user =
        let name = NoneEmptyString.value user.Name
        let email = EMail.value user.EMail

        let userClaim = Claim(ClaimTypes.Name,name)
        let emailClaim = Claim(ClaimTypes.Email,email)
        let roleClaims =
            user.Groups
            |> List.map (fun i -> NoneEmptyString.value i)
            |> List.map (fun group -> new Claim(ClaimTypes.Role,group))
        let claims = roleClaims @ [ userClaim; emailClaim ]

        let userIdentity = ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)
        ClaimsPrincipal(userIdentity)


    let private login (ctx:HttpContext) =
        task {
            let userListProjection = ctx.RequestServices.GetService<UserListProjection>()
            let userService = ctx.RequestServices.GetService<UserService>()

            let! model = Controller.getModel<UserLoginModel.Model>(ctx)

            let! user = getUserByEmail userListProjection model.EMail

            match user with
            | None ->
                return renderLoginPage ctx model
            | Some user ->
                let! cUser = userService.GetUser user.UserId

                return!
                    cUser 
                    |> Option.map (fun dUser ->
                        if checkPassword dUser model.Password then
                            let claimPrincipal = generateClaimPrincipal dUser

                            task {
                                let authProperties = AuthenticationProperties(IsPersistent= true)
                                do! ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,claimPrincipal,authProperties)
                                return (Controller.redirect ctx "/")
                            }
                        else
                            task { return (renderLoginPage ctx model) }
                    )
                    |> Option.defaultValue (task { return (renderLoginPage ctx model) })
            
        }
        


    let loginController = 
        controller {
            index (fun ctx -> renderLoginPage ctx Model.Empty)
            create login
        }


    let logoutRouter = router {
        get "/logout" (
            fun next ctx -> 
                task { 
                    do! ctx.SignOutAsync()
                    return! Core.redirectTo true "/" next ctx
                }
            )
    }


