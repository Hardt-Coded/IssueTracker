module ProjectViews

    open Giraffe.GiraffeViewEngine
    open ProjectModels
    open Microsoft.AspNetCore.Http


    let projectPageLayout (ctx:HttpContext) content =
        [
            div [
                _style "margin-top:20px"
            ] [
                section [ _class "hero is-info - welcom -is-small" ] [
                    div [ _class "hero-body" ] [
                        div [ _class "container" ] [
                            h1 [ _class "title" ] [
                                str "Project Managment"
                            ]

                            h2 [ _class "subtitle" ] [
                                str (sprintf "Welcome %s" ctx.User.Identity.Name)
                            ]
                        ]
                    ]
                ]
            ]
        

            div [
                _style "margin-top:20px"
            ] [
                yield! content
            ]

        ]


    let projectFormCard (ctx:HttpContext) title content =
        div [ _class "container" ] [
            div [ _class "columns" ] [
                div [ _class "column is-half is-offset-one-quarter" ] [
                               
                    div [ _class "card"] [
                        div [ _class "card-header" ] [
                            p [ _class "card-header-title" ] [
                                str title
                            ]
                        ]

                        div [ _class "card-content" ] [
                            div [ _class "content" ] [
                                           
                                yield! content

                            ]
                        ]
                    ]
        
                ]
            ]
        ]

    let projectFormMessage (ctx:HttpContext) colorCss title text returnUrl =
        div [ _class "container" ] [
            div [ _class "columns" ] [
                div [ _class "column is-half is-offset-one-quarter" ] [
                           
                    article [ _class (sprintf "message %s" colorCss) ] [
                        div [ _class "message-header" ] [
                            p [ ] [
                                str title
                            ]
                            a [ 
                                _class "delete" 
                                _href returnUrl
                            ] [ ]
                        ]

                        div [ _class "message-body" ] [
                            div [ _class "content" ] [
                                       
                                rawText text

                            ]
                            div [ _class "buttons is-right" ] [
                                a [ 
                                    _class (sprintf "button %s" colorCss) 
                                    _href returnUrl
                                ] [ str "Ok" ]
                            ]
                        
                        ]

                    
                    ]
    
                ]
            ]
        ]


    let projectFormError (ctx:HttpContext) message =
        App.layout 
            ctx
            (projectPageLayout ctx [
                projectFormMessage ctx "is-danger" "An Error has occured!" message "/projects"
            ])
    


    let projectFormSuccess (ctx:HttpContext) message =
        App.layout 
            ctx
            (projectPageLayout ctx [
                projectFormMessage ctx "is-success" "Successful!" message "/projects"
            ])


    module ProjectListView =

        let private getProjectLink id page =
            sprintf "/projects/%s/%s" id page


        let private projectActionButton (ctx:HttpContext) (project:ProjectListEntryModel) text urlId =
            a [ 
                _class "dropdown-item"
                _href (getProjectLink project.ProjectId urlId)
            ] [ str text ]


        let private projectActionButtons (ctx:HttpContext) (project:ProjectListEntryModel) =
            [
                div [ _class "dropdown" ] [
                    div [ _class "dropdown-trigger" ] [
                        button [ 
                            _class "button"
                            attr "aria-haspopup" "true"
                            attr "aria-controls" "dropdown-menu"
                        ] [ 
                            span [ ] [str "Actions" ]
                            span [ _class "icon is-small" ] [
                                i [ 
                                    _class "fa fa-angle-down" 
                                    attr "aria-hidden" "true"
                                ] [ ]
                            ]
                        ]
                    ]
                    div [
                        _class "dropdown-menu"
                        _id "dropdown-menu"
                        attr "role" "menu"
                    ] [
                        div [ _class "dropdown-content" ] [
                            projectActionButton ctx project "Change Name" "changeName" 
                            projectActionButton ctx project "Change EMail" "changeEMail"
                            projectActionButton ctx project "Change Password" "changePassword"
                            projectActionButton ctx project "Add to Group" "addToGroup"
                            projectActionButton ctx project "Remove from Group" "removefromGroup"
                            projectActionButton ctx project "Delete" "delete"
                        ]
                    ]
                ]
            
            ]


        let private projectListView (ctx:HttpContext) (projectListPage:ProjectListPage)  =
            [
                div [ _style "margin-bottom:20px" ] [
                    a [ 
                        _class "button is-success" 
                        _href "/projects/create"
                    ] [ 
                        span [ _class "icon is-small" ] [ 
                            i [ _class "fa fa-plus" ] [ ] 
                            
                        ]
                        span [ ] [ str "Add New Project" ]
                        
                    ]
                ]

                div [ _class "card-table" ] [
                    div [ _class "content" ] [
                        table [ 
                            _class "table" 
                            _style "width:100%"
                        ] [
                            colgroup [ ] [ 
                                col [ _style "width:20%" ]
                                col [ _style "width:25%" ]
                                col [ _style "width:45%" ]
                                col [ _style "width:10%" ]
                            ]
                            thead [ ] [
                                th [  ] [ str "Project Title" ]
                                th [  ] [ str "Description" ]
                                th [  ] [ str "Gruppen" ]
                                th [  ] [ str "" ]
                            ]
                            tbody [ ] [
                                for project in projectListPage.Data do
                                    tr [ ] [
                                        td [ ] [ str project.Title ]
                                        td [ ] [ str project.Description ]
                                        td [ ] [ str (project.IssueCount |> string) ]
                                        td [ ] [ yield! projectActionButtons ctx project ]
                                    ]
                            ]
                        ]
                    ]
                ]

                let getPagedLink page = 
                    (sprintf "/projects?page=%i&count=%i" page projectListPage.Count)

                div [ ] [
                    nav [ 
                        _class "pagination is-centered"
                        //attr ("role","navigation)
                    ] [
                    
                        a [ 
                            _class "pagination-previous"
                            if projectListPage.Page > 1 then
                                _href (projectListPage.Page - 1 |> getPagedLink )
                            else
                                _disabled
                        ] [ str "Pervious"]

                        a [ 
                            _class "pagination-previous"
                            if projectListPage.Page < projectListPage.MaxPage then
                                _href (projectListPage.Page + 1 |> getPagedLink )
                            else
                                _disabled
                        
                        ] [ str "Next" ]


                    
                        ul [ _class "pagination-list" ] [
                            if (projectListPage.Page > 2) then
                                li [ ] [ 
                                    a [ 
                                        _class "pagination-link" 
                                        _href (getPagedLink 1)
                                    ] [ 
                                        str "1" ]
                                ]

                            if (projectListPage.Page > 3) then
                                li [ ] [ span [ _class "pagination-ellipsis" ] [ rawText "&hellip;" ] ]
                        
                            if (projectListPage.Page > 2) then
                                li [ ] [ 
                                    a [ 
                                        _class "pagination-link" 
                                        _href (projectListPage.Page - 1 |> getPagedLink)
                                    ] [ 
                                        str ((projectListPage.Page - 1) |> string) ]
                                ]

                            li [ ] [ 
                                a [ 
                                    _class "pagination-link is-current" 
                                    _href (projectListPage.Page |> getPagedLink)
                                ] [ 
                                    str (projectListPage.Page |> string) ]
                            ]

                            if (projectListPage.Page < projectListPage.MaxPage) then
                                li [ ] [ 
                                    a [ 
                                        _class "pagination-link" 
                                        _href (projectListPage.Page + 1 |> getPagedLink)
                                    ] [ 
                                        str ((projectListPage.Page + 1) |> string) ]
                                ]

                            if (projectListPage.Page < projectListPage.MaxPage - 2) then
                                li [ ] [ span [ _class "pagination-ellipsis" ] [ rawText "&hellip;" ] ]


                            if (projectListPage.Page < projectListPage.MaxPage - 1) then
                                li [ ] [ 
                                    a [ 
                                        _class "pagination-link"
                                        _href (getPagedLink projectListPage.MaxPage)
                                    ] [ 
                                        str (projectListPage.MaxPage |> string) ]
                                ]

                        ]
                    ]
            
                ]
            ]


        let projectListLayout ctx projectList =
            (projectListView ctx projectList)
            |> projectPageLayout ctx
            |> App.layout ctx





    module ProjectCreatePage =



        let private projectCreateView (ctx:HttpContext) (model:ProjectCreateModel) =
            [
                projectFormCard ctx "Create Project" [
                    form [ 
                        _method "POST" 
                        _autocomplete "off"
                    ] [
                           
                        div [ _class "field" ] [ 
                            label [ _class "label" ] [ 
                                str "Title" 
                            ]
                            div [ _class "control has-icons-left" ] [ 
                                input [ _class "input"
                                        _type "text"
                                        _name "Title"
                                        _autocomplete "new-password"
                                        _placeholder "Text input"
                                        _value model.Title ]
                                span [ _class "icon is-small is-left" ] [ 
                                    i [ _class "fa fa-project" ] [ ] 
                                ]
                                   
                            ]
                               
                        ]


                        div [ _class "field" ] [ 
                            label [ _class "label" ] [ 
                                str "Decription" 
                            ]
                            div [ _class "control has-icons-left" ] [ 
                                textarea [ 
                                        _class "input"
                                        _type "text"
                                        _name "Description"
                                        _autocomplete "new-password"
                                        _placeholder "Text input"
                                        _value model.Description ] [ ]
                                span [ _class "icon is-small is-left" ] [ 
                                    i [ _class "fa fa-project" ] [ ] 
                                ]
                                   
                            ]
                               
                        ]


                        div [ _class "field is-grouped" ] [
                            div [ _class "control" ] [
                                button [ 
                                    _class "button is-link"
                                    _type "submit"
                                       
                                ] [ str "Save" ]
                            ]
                            div [ _class "control" ] [
                                a [ 
                                    _class "button is-link is-light" 
                                    _href "/projects"
                                ] [ str "Cancel" ]
                            ]
                        ]
                    ]
                ]
            ]


        let projectCreateLayout ctx model =
            (projectCreateView ctx model)
            |> projectPageLayout ctx
            |> App.layout ctx

