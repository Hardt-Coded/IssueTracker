module UserAdminController
    open Saturn
    open FSharp.Control.Tasks.V2
    open Microsoft.AspNetCore
    open Microsoft.AspNetCore.Http
    open Microsoft.Extensions.DependencyInjection
    open Projections.UserList
    open Domain.Types.Common
    open Domain.User
    open Saturn.ControllerHelpers
    open Giraffe
    open UserAdminViews
    open UserAdminModels

    
    


    let userListIndex (ctx:HttpContext) =
        task {
            let userListProjection = ctx.RequestServices.GetService<Projections.UserList.UserListProjection>()
            let! users = userListProjection.GetUserList ()
            let users =
                users
                |>List.sortBy (fun i -> i.EMail)
                |> List.map (fun i -> 
                    {
                        UserId = i.UserId
                        Name = i.Name
                        EMail = i.EMail
                        Groups = sprintf "(%i)" i.Groups.Length
                    }
                )
            return Controller.renderHtml ctx (userListLayout users ctx)
        }
    
    
    let userController = controller {
        index userListIndex
    
    }