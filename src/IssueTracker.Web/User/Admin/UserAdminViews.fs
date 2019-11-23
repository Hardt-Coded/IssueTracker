module UserAdminViews

    open Giraffe.GiraffeViewEngine
    open UserAdminModels

    let userListView (userList:UserListEntryModel list) ctx =
        [
            div [ ] [
                table [ _class "table" ] [
                    thead [ ] [
                        th [ ] [ str "E-Mail" ]
                        th [ ] [ str "Name" ]
                        th [ ] [ str "Gruppen" ]
                        th [ ] [ str "" ]
                    ]
                    tbody [ ] [
                        for i in userList do
                            tr [ ] [
                                td [ ] [ str i.EMail ]
                                td [ ] [ str i.Name ]
                                td [ ] [ str i.Groups ]
                                td [ ] [ ]
                            ]
                    ]
                ]
            ]
        ]



    let userListLayout userList ctx =
        App.layout (userListView userList ctx) ctx
        
    