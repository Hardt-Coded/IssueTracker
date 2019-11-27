module UserAdminViews

    open Giraffe.GiraffeViewEngine
    open UserAdminModels
    open Microsoft.AspNetCore.Http


    let userPageLayout (ctx:HttpContext) content =
        [
            div [
                _style "margin-top:20px"
            ] [
                section [ _class "hero is-info - welcom -is-small" ] [
                    div [ _class "hero-body" ] [
                        div [ _class "container" ] [
                            h1 [ _class "title" ] [
                                str "User Managment"
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


    let userFormCard (ctx:HttpContext) title content =
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

    let userFormMessage (ctx:HttpContext) colorCss title text returnUrl =
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
                                           
                                str text

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


    let userFormError (ctx:HttpContext) message =
        App.layout 
            ctx
            (userPageLayout ctx [
                userFormMessage ctx "is-danger" "An Error has occured!" message "/users"
            ])
        


    let userFormSuccess (ctx:HttpContext) message =
        App.layout 
            ctx
            (userPageLayout ctx [
                userFormMessage ctx "is-success" "Successful!" message "/users"
            ])




    module UserListPage =

        let private getUserLink id page =
            sprintf "/users/%s/%s" id page


        let private userActionButton (ctx:HttpContext) (user:UserListEntryModel) text urlId =
            a [ 
                _class "dropdown-item"
                _href (getUserLink user.UserId urlId)
            ] [ str text ]


        let private userActionButtons (ctx:HttpContext) (user:UserListEntryModel) =
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
                            userActionButton ctx user "Change Name" "changeName" 
                            userActionButton ctx user "Change EMail" "changeEMail"
                            userActionButton ctx user "Change Password" "changePassword"
                            userActionButton ctx user "Add to Group" "addToGroup"
                            userActionButton ctx user "Remove from Group" "removefromGroup"
                            userActionButton ctx user "Delete" "delete"
                        ]
                    ]
                ]
            
            ]


        let private userListView (ctx:HttpContext) (userListPage:UserListPage)  =
            [
                div [ _style "margin-bottom:20px" ] [
                    a [ 
                        _class "button is-success" 
                        _href "/users/create"
                    ] [ 
                        span [ _class "icon is-small" ] [ 
                            i [ _class "fa fa-plus" ] [ ] 
                            
                        ]
                        span [ ] [ str "Add New User" ]
                        
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
                                th [  ] [ str "E-Mail" ]
                                th [  ] [ str "Name" ]
                                th [  ] [ str "Gruppen" ]
                                th [  ] [ str "" ]
                            ]
                            tbody [ ] [
                                for user in userListPage.Data do
                                    tr [ ] [
                                        td [ ] [ str user.EMail ]
                                        td [ ] [ str user.Name ]
                                        td [ ] [ str user.Groups ]
                                        td [ ] [ yield! userActionButtons ctx user ]
                                    ]
                            ]
                        ]
                    ]
                ]

                let getPagedLink page = 
                    (sprintf "/users?page=%i&count=%i" page userListPage.Count)

                div [ ] [
                    nav [ 
                        _class "pagination is-centered"
                        //attr ("role","navigation)
                    ] [
                    
                        a [ 
                            _class "pagination-previous"
                            if userListPage.Page > 1 then
                                _href (userListPage.Page - 1 |> getPagedLink )
                            else
                                _disabled
                        ] [ str "Pervious"]

                        a [ 
                            _class "pagination-previous"
                            if userListPage.Page < userListPage.MaxPage then
                                _href (userListPage.Page + 1 |> getPagedLink )
                            else
                                _disabled
                        
                        ] [ str "Next" ]


                    
                        ul [ _class "pagination-list" ] [
                            if (userListPage.Page > 2) then
                                li [ ] [ 
                                    a [ 
                                        _class "pagination-link" 
                                        _href (getPagedLink 1)
                                    ] [ 
                                        str "1" ]
                                ]

                            if (userListPage.Page > 3) then
                                li [ ] [ span [ _class "pagination-ellipsis" ] [ rawText "&hellip;" ] ]
                        
                            if (userListPage.Page > 2) then
                                li [ ] [ 
                                    a [ 
                                        _class "pagination-link" 
                                        _href (userListPage.Page - 1 |> getPagedLink)
                                    ] [ 
                                        str ((userListPage.Page - 1) |> string) ]
                                ]

                            li [ ] [ 
                                a [ 
                                    _class "pagination-link is-current" 
                                    _href (userListPage.Page |> getPagedLink)
                                ] [ 
                                    str (userListPage.Page |> string) ]
                            ]

                            if (userListPage.Page < userListPage.MaxPage) then
                                li [ ] [ 
                                    a [ 
                                        _class "pagination-link" 
                                        _href (userListPage.Page + 1 |> getPagedLink)
                                    ] [ 
                                        str ((userListPage.Page + 1) |> string) ]
                                ]

                            if (userListPage.Page < userListPage.MaxPage - 2) then
                                li [ ] [ span [ _class "pagination-ellipsis" ] [ rawText "&hellip;" ] ]


                            if (userListPage.Page < userListPage.MaxPage - 1) then
                                li [ ] [ 
                                    a [ 
                                        _class "pagination-link"
                                        _href (getPagedLink userListPage.MaxPage)
                                    ] [ 
                                        str (userListPage.MaxPage |> string) ]
                                ]

                        ]
                    ]
            
                ]
            ]


        let userListLayout ctx userList =
            (userListView ctx userList)
            |> userPageLayout ctx
            |> App.layout ctx


    module UserChangeNamePage =



        let private userChangeNameView (ctx:HttpContext) (model:UserChangeNameModel) =
            [
                userFormCard ctx "Change Name" [
                    form [ _method "POST" ] [
                        input [ 
                            _type "hidden"
                            _name "UserId"
                            _value model.UserId
                        ] 
                        div [ _class "field" ] [ 
                            label [ _class "label" ] [ 
                                str "New Name" 
                            ]
                            div [ _class "control has-icons-left" ] [ 
                                input [ _class "input"
                                        _type "text"
                                        _name "Name"
                                        _placeholder "Text input"
                                        _value model.Name ]
                                span [ _class "icon is-small is-left" ] [ 
                                    i [ _class "fa fa-user" ] [ ] 
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
                                    _href "/users"
                                ] [ str "Cancel" ]
                            ]
                        ]
                    ]
                ]
            ]


        let userChangeNameLayout ctx model =
            (userChangeNameView ctx model)
            |> userPageLayout ctx
            |> App.layout ctx


    module UserChangeEMailPage =



        let private userChangeEMailView (ctx:HttpContext) (model:UserChangeEMailModel) =
            [
                userFormCard ctx "Change EMail Address" [
                    form [ _method "POST" ] [
                        input [ 
                            _type "hidden"
                            _name "UserId"
                            _value model.UserId
                        ] 
                        div [ _class "field" ] [ 
                            label [ _class "label" ] [ 
                                str "New EMail" 
                            ]
                            div [ _class "control has-icons-left" ] [ 
                                input [ _class "input"
                                        _type "text"
                                        _name "EMail"
                                        _placeholder "Text input"
                                        _value model.EMail ]
                                span [ _class "icon is-small is-left" ] [ 
                                    i [ _class "fa fa-user" ] [ ] 
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
                                    _href "/users"
                                ] [ str "Cancel" ]
                            ]
                        ]
                    ]
                ]
            ]


        let userChangeEMailLayout ctx model =
            (userChangeEMailView ctx model)
            |> userPageLayout ctx
            |> App.layout ctx



    module UserChangePasswordPage =



        let private userChangePasswordView (ctx:HttpContext) (model:UserChangePasswordModel) =
            [
                userFormCard ctx "Change Password" [
                    form [ _method "POST" ] [
                        input [ 
                            _type "hidden"
                            _name "UserId"
                            _value model.UserId
                        ] 
                        div [ _class "field" ] [ 
                            label [ _class "label" ] [ 
                                str "New Password" 
                            ]
                            div [ _class "control has-icons-left" ] [ 
                                input [ _class "input"
                                        _type "password"
                                        _name "Password"
                                        _placeholder "Text input"
                                        _value model.Password ]
                                span [ _class "icon is-small is-left" ] [ 
                                    i [ _class "fa fa-user" ] [ ] 
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
                                    _href "/users"
                                ] [ str "Cancel" ]
                            ]
                        ]
                    ]
                ]
            ]


        let userChangePasswordLayout ctx model =
            (userChangePasswordView ctx model)
            |> userPageLayout ctx
            |> App.layout ctx




    module UserAddToGroupPage =



        let private userAddToGroupView (ctx:HttpContext) (model:UserAddToGroupModel) =
            [
                userFormCard ctx "Add To Group" [
                    form [ _method "POST" ] [
                        input [ 
                            _type "hidden"
                            _name "UserId"
                            _value model.UserId
                        ] 
                        div [ _class "field" ] [ 
                            label [ _class "label" ] [ 
                                str "Group" 
                            ]
                            div [ _class "control has-icons-left" ] [ 
                                input [ _class "input"
                                        _type "text"
                                        _name "Group"
                                        _placeholder "Text input"
                                        _value model.Group ]
                                span [ _class "icon is-small is-left" ] [ 
                                    i [ _class "fa fa-user" ] [ ] 
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
                                    _href "/users"
                                ] [ str "Cancel" ]
                            ]
                        ]
                    ]
                ]
            ]


        let userAddToGroupLayout ctx model =
            (userAddToGroupView ctx model)
            |> userPageLayout ctx
            |> App.layout ctx




    module UserRemoveFromGroupPage =



        let private userRemoveFromGroupView (ctx:HttpContext) (model:UserRemoveFromGroupModel) =
            [
                userFormCard ctx "Remove From Group" [
                    form [ _method "POST" ] [
                        input [ 
                            _type "hidden"
                            _name "UserId"
                            _value model.UserId
                        ] 
                        div [ _class "field" ] [ 
                            label [ _class "label" ] [ 
                                str "Group" 
                            ]
                            div [ _class "control has-icons-left" ] [ 
                                div [ _class "select" ] [
                                    select [ 
                                        _value model.Group
                                        _name "Group"
                                    ] [
                                        option [ _value "" ] [ str "" ]
                                        for group in model.CurrentGroups do
                                            option [ _value group ] [ str group ]
                                    ]
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
                                    _href "/users"
                                ] [ str "Cancel" ]
                            ]
                        ]
                    ]
                ]
            ]


        let userRemoveFromGroupLayout ctx model =
            (userRemoveFromGroupView ctx model)
            |> userPageLayout ctx
            |> App.layout ctx




    module UserDeletePage =



        let private userDeleteView (ctx:HttpContext) (model:UserDeleteModel) =
            [
                userFormCard ctx "Delete User" [
                    form [ _method "POST" ] [
                        input [ 
                            _type "hidden"
                            _name "UserId"
                            _value model.UserId
                        ] 
                        div [ _class "field" ] [ 
                            label [ _class "label" ] [ 
                                str "Do you want to delete this User?" 
                            ]
                        ]

                        div [ _class "field is-grouped" ] [
                            div [ _class "control" ] [
                                button [ 
                                    _class "button is-link"
                                    _type "submit"
                                    
                                ] [ str "Yes, Delete this User!" ]
                            ]
                            div [ _class "control" ] [
                                a [ 
                                    _class "button is-link is-light" 
                                    _href "/users"
                                ] [ str "Cancel" ]
                            ]
                        ]
                    ]
                ]
            ]


        let userDeleteLayout ctx model =
            (userDeleteView ctx model)
            |> userPageLayout ctx
            |> App.layout ctx



    module UserCreatePage =



           let private userCreateView (ctx:HttpContext) (model:UserCreateModel) =
               [
                   userFormCard ctx "Create User" [
                       form [ _method "POST" ] [
                           
                           div [ _class "field" ] [ 
                               label [ _class "label" ] [ 
                                   str "Name" 
                               ]
                               div [ _class "control has-icons-left" ] [ 
                                   input [ _class "input"
                                           _type "text"
                                           _name "Name"
                                           _placeholder "Text input"
                                           _value model.Name ]
                                   span [ _class "icon is-small is-left" ] [ 
                                       i [ _class "fa fa-user" ] [ ] 
                                   ]
                                   
                               ]
                               
                           ]

                           div [ _class "field" ] [ 
                               label [ _class "label" ] [ 
                                   str "EMail" 
                               ]
                               div [ _class "control has-icons-left" ] [ 
                                   input [ _class "input"
                                           _type "password"
                                           _name "EMail"
                                           _placeholder "Text input"
                                           _value model.EMail ]
                                   span [ _class "icon is-small is-left" ] [ 
                                       i [ _class "fa fa-envelope" ] [ ] 
                                   ]
                                   
                               ]
                               
                           ]

                           div [ _class "field" ] [ 
                               label [ _class "label" ] [ 
                                   str "Password" 
                               ]
                               div [ _class "control has-icons-left" ] [ 
                                   input [ _class "input"
                                           _type "text"
                                           _name "Password"
                                           _placeholder "Text input"
                                           _value model.Password ]
                                   span [ _class "icon is-small is-left" ] [ 
                                       i [ _class "fa fa-lock" ] [ ] 
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
                                       _href "/users"
                                   ] [ str "Cancel" ]
                               ]
                           ]
                       ]
                   ]
               ]


           let userCreateLayout ctx model =
               (userCreateView ctx model)
               |> userPageLayout ctx
               |> App.layout ctx

        
    