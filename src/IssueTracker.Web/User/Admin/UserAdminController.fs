
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
    open Common
    open Domain.Types.User

    let convertErrorToString (error:Domain.Common.Errors) =
        match error with
        | Domain.Common.Errors.DomainError e ->
            e
        | Domain.Common.Errors.InfrastructureError ie ->
            sprintf "an internal error has occured: %s" ie.Message


    open System

    let createEMailDuplicateValidation (userListProjection:Projections.UserList.UserListProjection) =
        task {
            let! userList = userListProjection.GetUserList ()
            let isEMailDuplicate = 
                fun email -> 
                    userList 
                    |> List.exists (fun i -> String.Equals(i.EMail,email,StringComparison.InvariantCultureIgnoreCase))
            return isEMailDuplicate
        }


    
    module UserList =


        let private userListIndex (ctx:HttpContext) =
            task {
                let userListProjection = ctx.RequestServices.GetService<Projections.UserList.UserListProjection>()
                let! users = userListProjection.GetUserList ()

                let count = 
                    ctx.GetQueryStringValue("count") 
                    |> Result.map (Helpers.parseInt) 
                    |> Result.ifError None
                    |> Option.defaultValue 10
                let count = if count < 0 then 10 else count
            
                let maxPage = (users.Length / count)

                let page = 
                    ctx.GetQueryStringValue("page") 
                    |> Result.map (Helpers.parseInt) 
                    |> Result.ifError None
                    |> Option.defaultValue 1
                let page = 
                    if page < 0 then 1 
                    else if page > maxPage then maxPage
                    else page

                let flattenGroups strList =
                    let first3 =
                        System.String.Join(", ", strList |> List.truncate 3)
                    
                    if (strList.Length > 3) then
                        sprintf "%s, ..." first3
                    else 
                        first3

                let pagedUserList =
                    {
                        Page = page
                        Count = count
                        MaxPage = maxPage
                        Data =
                            users
                            |>List.sortBy (fun i -> i.EMail)
                            |> List.skip ((page - 1) * count)
                            |> List.truncate count
                            |> List.map (fun i -> 
                                {
                                    UserId = i.UserId
                                    Name = i.Name
                                    EMail = i.EMail
                                    Groups = flattenGroups i.Groups
                                }
                            )
                    }

            
                return Controller.renderHtml ctx (UserListPage.userListLayout ctx pagedUserList)
            }
    
    
        let userListController = controller {
            index userListIndex
            
        }

        


    module UserChangeName =
        
        let private userChangeNameIndex id (ctx:HttpContext)  =
            task {
                let userService = ctx.RequestServices.GetService<Services.User.UserService>()

                let! user = userService.GetUser id
                match user with
                | None ->
                    return (Controller.text ctx "invalid user id")
                | Some user ->
                    let userChangeNameModel:UserChangeNameModel = {
                        UserId = UserId.value user.UserId
                        Name = NotEmptyString.value user.Name
                    }
                    return Controller.renderHtml ctx (UserChangeNamePage.userChangeNameLayout ctx userChangeNameModel)
            }

        let private changeUserName id (ctx:HttpContext)  =
            task {
                let userService = ctx.RequestServices.GetService<Services.User.UserService>()
                let! model = Controller.getModel<UserChangeNameModel>(ctx)
                if id <> model.UserId then
                    return Controller.renderHtml ctx (userFormError ctx "invalid user id!")
                else
                    let command:CommandArguments.ChangeName = {
                            UserId = model.UserId
                            Name = model.Name
                        }

                    let! res = userService.ChangeName command
                    match res with
                    | Error e ->
                        let errorMessage = convertErrorToString e
                        return Controller.renderHtml ctx (userFormError ctx errorMessage)
                    | Ok _ ->
                        let userProjection = ctx.RequestServices.GetService<Projections.UserList.UserListProjection>()
                        userProjection.UpdateProjection ()
                        return Controller.renderHtml ctx (userFormSuccess ctx "the username was successfully changed.")
            }
    
    
        let userChangeNameController id = controller {
            index (userChangeNameIndex id)
            create (changeUserName id)
        }


    module UserChangeEMail =
        
        let private userChangeEMailIndex id (ctx:HttpContext)  =
            task {
                let userService = ctx.RequestServices.GetService<Services.User.UserService>()

                let! user = userService.GetUser id
                match user with
                | None ->
                    return (Controller.text ctx "invalid user id")
                | Some user ->
                    let userChangeEMailModel:UserChangeEMailModel = {
                        UserId = UserId.value user.UserId
                        EMail = EMail.value user.EMail
                    }
                    return Controller.renderHtml ctx (UserChangeEMailPage.userChangeEMailLayout ctx userChangeEMailModel)
            }

        let private changeUserEMail id (ctx:HttpContext)  =
            task {
                let userService = ctx.RequestServices.GetService<Services.User.UserService>()
                


                let! model = Controller.getModel<UserChangeEMailModel>(ctx)
                if id <> model.UserId then
                    return Controller.renderHtml ctx (userFormError ctx "invalid user id!")
                else
                    let command:CommandArguments.ChangeEMail = {
                            UserId = model.UserId
                            EMail = model.EMail
                        }

                    let userProjection = ctx.RequestServices.GetService<Projections.UserList.UserListProjection>()
                    let! emailDuplicationValidator = createEMailDuplicateValidation userProjection

                    let! res = userService.ChangeEMail emailDuplicationValidator command
                    match res with
                    | Error e ->
                        let errorMessage = convertErrorToString e
                        return Controller.renderHtml ctx (userFormError ctx errorMessage)
                    | Ok _ ->                        
                        userProjection.UpdateProjection ()
                        return Controller.renderHtml ctx (userFormSuccess ctx "the email address was successfully changed.")
            }
    
    
        let userChangeEMailController id = controller {
            index (userChangeEMailIndex id)
            create (changeUserEMail id)
        }


    module UserChangePassword =
        
        let private userChangePasswordIndex id (ctx:HttpContext)  =
            task {
                let userService = ctx.RequestServices.GetService<Services.User.UserService>()

                let! user = userService.GetUser id
                match user with
                | None ->
                    return (Controller.text ctx "invalid user id")
                | Some user ->
                    let userChangePasswordModel:UserChangePasswordModel = {
                        UserId = UserId.value user.UserId
                        Password = ""
                    }
                    return Controller.renderHtml ctx (UserChangePasswordPage.userChangePasswordLayout ctx userChangePasswordModel)
            }

        let private changeUserPassword id (ctx:HttpContext)  =
            task {
                let userService = ctx.RequestServices.GetService<Services.User.UserService>()
                let! model = Controller.getModel<UserChangePasswordModel>(ctx)
                if id <> model.UserId then
                    return Controller.renderHtml ctx (userFormError ctx "invalid user id!")
                else
                    let command:CommandArguments.ChangePassword = {
                            UserId = model.UserId
                            Password = model.Password
                        }

                    let! res = userService.ChangePassword command
                    match res with
                    | Error e ->
                        let errorMessage = convertErrorToString e
                        return Controller.renderHtml ctx (userFormError ctx errorMessage)
                    | Ok _ ->
                        let userProjection = ctx.RequestServices.GetService<Projections.UserList.UserListProjection>()
                        userProjection.UpdateProjection ()
                        return Controller.renderHtml ctx (userFormSuccess ctx "the password was successfully changed.")
            }
    
    
        let userChangePasswordController id = controller {
            index (userChangePasswordIndex id)
            create (changeUserPassword id)
        }


    module UserAddToGroup =
        
        let private userAddToGroupIndex id (ctx:HttpContext)  =
            task {
                let userService = ctx.RequestServices.GetService<Services.User.UserService>()

                let! user = userService.GetUser id
                match user with
                | None ->
                    return (Controller.text ctx "invalid user id")
                | Some user ->
                    let userAddToGroupModel:UserAddToGroupModel = {
                        UserId = UserId.value user.UserId
                        Group = ""
                    }
                    return Controller.renderHtml ctx (UserAddToGroupPage.userAddToGroupLayout ctx userAddToGroupModel)
            }

        let private changeUserName id (ctx:HttpContext)  =
            task {
                let userService = ctx.RequestServices.GetService<Services.User.UserService>()
                let! model = Controller.getModel<UserAddToGroupModel>(ctx)
                if id <> model.UserId then
                    return Controller.renderHtml ctx (userFormError ctx "invalid user id!")
                else
                    let command:CommandArguments.AddToGroup = {
                            UserId = model.UserId
                            Group = model.Group
                        }

                    let! res = userService.AddToGroup command
                    match res with
                    | Error e ->
                        let errorMessage = convertErrorToString e
                        return Controller.renderHtml ctx (userFormError ctx errorMessage)
                    | Ok _ ->
                        let userProjection = ctx.RequestServices.GetService<Projections.UserList.UserListProjection>()
                        userProjection.UpdateProjection ()
                        return Controller.renderHtml ctx (userFormSuccess ctx "the user was added successfully to the group.")
            }
    
    
        let userAddToGroupController id = controller {
            index (userAddToGroupIndex id)
            create (changeUserName id)
        }


    module UserRemoveFromGroup =
        
        let private userRemoveFromGroupIndex id (ctx:HttpContext)  =
            task {
                let userService = ctx.RequestServices.GetService<Services.User.UserService>()

                let! user = userService.GetUser id
                match user with
                | None ->
                    return (Controller.text ctx "invalid user id")
                | Some user ->
                    let userRemoveFromGroupModel:UserRemoveFromGroupModel = {
                        UserId = UserId.value user.UserId
                        CurrentGroups = user.Groups |> List.map NotEmptyString.value
                        Group = ""
                    }
                    return Controller.renderHtml ctx (UserRemoveFromGroupPage.userRemoveFromGroupLayout ctx userRemoveFromGroupModel)
            }

        let private changeUserName id (ctx:HttpContext)  =
            task {
                let userService = ctx.RequestServices.GetService<Services.User.UserService>()
                let! model = Controller.getModel<UserRemoveFromGroupModel>(ctx)
                if id <> model.UserId then
                    return Controller.renderHtml ctx (userFormError ctx "invalid user id!")
                else
                    let command:CommandArguments.RemoveFromGroup = {
                            UserId = model.UserId
                            Group = model.Group
                        }

                    let! res = userService.RemoveFromGroup command
                    match res with
                    | Error e ->
                        let errorMessage = convertErrorToString e
                        return Controller.renderHtml ctx (userFormError ctx errorMessage)
                    | Ok _ ->
                        let userProjection = ctx.RequestServices.GetService<Projections.UserList.UserListProjection>()
                        userProjection.UpdateProjection ()
                        return Controller.renderHtml ctx (userFormSuccess ctx "the user was removed successfully from the group.")
            }
    
    
        let userRemoveFromGroupController id = controller {
            index (userRemoveFromGroupIndex id)
            create (changeUserName id)
        }


    module UserCreate =
        
        let private userCreateIndex (ctx:HttpContext)  =
            task {
                
                let userCreateModel:UserCreateModel = {
                    Name = ""
                    EMail = ""
                    Password = ""
                }
                return Controller.renderHtml ctx (UserCreatePage.userCreateLayout ctx userCreateModel)
            }

        let private createUser (ctx:HttpContext)  =
            task {
                let userService = ctx.RequestServices.GetService<Services.User.UserService>()
                let! model = Controller.getModel<UserCreateModel>(ctx)

                let command:CommandArguments.CreateUser = {
                        UserId = System.Guid.NewGuid().ToString("N")
                        Name = model.Name
                        EMail = model.EMail
                        Password = model.Password
                    }

                let userProjection = ctx.RequestServices.GetService<Projections.UserList.UserListProjection>()
                let! emailDuplicationValidator = createEMailDuplicateValidation userProjection

                let! res = userService.CreateUser emailDuplicationValidator command
                match res with
                | Error e ->
                    let errorMessage = convertErrorToString e
                    return Controller.renderHtml ctx (userFormError ctx errorMessage)
                | Ok _ ->
                    userProjection.UpdateProjection ()
                    return Controller.renderHtml ctx (userFormSuccess ctx "the user was successfully created.")
            }
    
    
        let userCreateController = controller {
            index (userCreateIndex)
            create (createUser)
        }


    module UserDelete =
        
        let private userDeleteIndex id (ctx:HttpContext)  =
            task {
                let userService = ctx.RequestServices.GetService<Services.User.UserService>()

                let! user = userService.GetUser id
                match user with
                | None ->
                    return (Controller.text ctx "invalid user id")
                | Some user ->
                    let userDeleteModel:UserDeleteModel = {
                        UserId = UserId.value user.UserId
                    }
                    return Controller.renderHtml ctx (UserDeletePage.userDeleteLayout ctx userDeleteModel)
            }

        let private deleteUser id (ctx:HttpContext)  =
            task {
                let userService = ctx.RequestServices.GetService<Services.User.UserService>()
                let! model = Controller.getModel<UserDeleteModel>(ctx)
                if id <> model.UserId then
                    return Controller.renderHtml ctx (userFormError ctx "invalid user id!")
                else
                    let command:CommandArguments.DeleteUser = {
                            UserId = model.UserId
                        }

                    let! res = userService.DeleteUser command
                    match res with
                    | Error e ->
                        let errorMessage = convertErrorToString e
                        return Controller.renderHtml ctx (userFormError ctx errorMessage)
                    | Ok _ ->
                        let userProjection = ctx.RequestServices.GetService<Projections.UserList.UserListProjection>()
                        userProjection.UpdateProjection ()
                        return Controller.renderHtml ctx (userFormSuccess ctx "the username was successfully deleted.")
            }
    
    
        let userDeleteController id = controller {
            index (userDeleteIndex id)
            create (deleteUser id)
        }



    let userDetailRouter = router {
        forward "" (fun next ctx -> UserList.userListController next ctx)
        forwardf "/%s/changeName" (fun id next ctx -> (UserChangeName.userChangeNameController id) next ctx)
        forwardf "/%s/changeEMail" (fun id next ctx ->(UserChangeEMail.userChangeEMailController id) next ctx)
        forwardf "/%s/changePassword" (fun id next ctx -> (UserChangePassword.userChangePasswordController id) next ctx)
        forwardf "/%s/addToGroup" (fun id next ctx -> (UserAddToGroup.userAddToGroupController id) next ctx)
        forwardf "/%s/removefromGroup" (fun id next ctx -> (UserRemoveFromGroup.userRemoveFromGroupController id) next ctx)
        forwardf "/%s/delete" (fun id next ctx -> (UserDelete.userDeleteController id) next ctx)
        forwardf "/%s" (fun id next ctx -> Controller.text ctx (sprintf "%s" id) )
        forward "/create" (fun next ctx -> (UserCreate.userCreateController) next ctx)
    }