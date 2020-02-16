module ProjectController

    open Saturn
    open FSharp.Control.Tasks.V2
    open Microsoft.AspNetCore
    open Microsoft.AspNetCore.Http
    open Microsoft.Extensions.DependencyInjection
    open Project.Projections.ProjectList
    open Common.Types
    open Project.Domain
    open Project.Types
    open Project.Services
    open Common.Domain
    open Saturn.ControllerHelpers
    open Giraffe
    open ProjectViews
    open ProjectModels
    open Common
    open System



    let convertErrorToString (errors:Errors list) =
        let joinStr (l:string seq) = String.Join("<br />", l)

        errors
        |> List.map (fun error ->
            match error with
            | Errors.DomainError e ->
                e
            | InfrastructureError ie ->
                sprintf "an internal error has occured: %s" ie.Message
        )
        |> joinStr



    module ProjectList =
        
        let private projectListIndex (ctx:HttpContext) =
            task {
                let projectListProjection = ctx.RequestServices.GetService<ProjectListProjection>()
                let! projects = projectListProjection.GetProjectList ()

                let count = 
                    ctx.GetQueryStringValue("count") 
                    |> Result.map (Helpers.parseInt) 
                    |> Result.ifError None
                    |> Option.defaultValue 10
                let count = if count < 0 then 10 else count
            
                let maxPage = (projects.Length / count)

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

                let pagedProjectList =
                    {
                        Page = page
                        Count = count
                        MaxPage = maxPage
                        Data =
                            projects
                            |> List.sortBy (fun i -> i.Title)
                            |> List.skip ((page - 1) * count)
                            |> List.truncate count
                            |> List.map (fun i -> 
                                {
                                    ProjectId = i.ProjectId
                                    Title = i.Title
                                    Description = i.Description
                                    IssueCount = i.Issues.Length
                                }
                            )
                    }

            
                return Controller.renderHtml ctx (ProjectListView.projectListLayout ctx pagedProjectList)
            }
    
    
        let projectListController = controller {
            index projectListIndex
            
        }


    module ProjectCreate =
        
        let private projectCreateIndex (ctx:HttpContext)  =
            task {
                
                let projectCreateModel:ProjectCreateModel = {
                    Title = ""
                    Description = ""
                }
                return Controller.renderHtml ctx (ProjectCreatePage.projectCreateLayout ctx projectCreateModel)
            }

        let private createProject (ctx:HttpContext)  =
            task {
                let projectService = ctx.RequestServices.GetService<ProjectService>()
                let! model = Controller.getModel<ProjectCreateModel>(ctx)

                let command:CommandArguments.CreateProject = {
                        ProjectId = System.Guid.NewGuid().ToString("N")
                        Title = model.Title
                        Description = model.Description
                        CreatedBy = ctx.User.Identity.Name
                    }

                let projectProjection = ctx.RequestServices.GetService<ProjectListProjection>()

                let! res = projectService.CreateProject command
                match res with
                | Error e ->
                    let errorMessage = convertErrorToString e
                    return Controller.renderHtml ctx (projectFormError ctx errorMessage)
                | Ok _ ->
                    projectProjection.UpdateProjection ()
                    return Controller.renderHtml ctx (projectFormSuccess ctx "the project was successfully created.")
            }
    
    
        let projectCreateController = controller {
            index (projectCreateIndex)
            create (createProject)
        }




    let projectDetailRouter = router {
        forward "" (fun next ctx -> ProjectList.projectListController next ctx)
        //forwardf "/%s/changeName" (fun id next ctx -> (UserChangeName.userChangeNameController id) next ctx)
        //forwardf "/%s/changeEMail" (fun id next ctx ->(UserChangeEMail.userChangeEMailController id) next ctx)
        //forwardf "/%s/changePassword" (fun id next ctx -> (UserChangePassword.userChangePasswordController id) next ctx)
        //forwardf "/%s/addToGroup" (fun id next ctx -> (UserAddToGroup.userAddToGroupController id) next ctx)
        //forwardf "/%s/removefromGroup" (fun id next ctx -> (UserRemoveFromGroup.userRemoveFromGroupController id) next ctx)
        //forwardf "/%s/delete" (fun id next ctx -> (UserDelete.userDeleteController id) next ctx)
        //forwardf "/%s" (fun id next ctx -> Controller.text ctx (sprintf "%s" id) )
        forward "/create" (fun next ctx -> (ProjectCreate.projectCreateController) next ctx)
    }

