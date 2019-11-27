module UserAdminModels

    open Common



    type UserListEntryModel = {
            UserId:string
            Name:string
            EMail:string
            Groups:string
        }
        with 
            static member Empty = {
                UserId=""
                Name=""
                EMail=""
                Groups=""
            }


    type UserListPage = Paging<UserListEntryModel>


    [<CLIMutable>]
    type UserChangeNameModel = {
        UserId:string
        Name:string
    }

    [<CLIMutable>]
    type UserChangeEMailModel = {
        UserId:string
        EMail:string
    }

    [<CLIMutable>]
    type UserChangePasswordModel = {
        UserId:string
        Password:string
    }

    [<CLIMutable>]
    type UserAddToGroupModel = {
        UserId:string
        Group:string
    }

    [<CLIMutable>]
    type UserRemoveFromGroupModel = {
        UserId:string
        CurrentGroups:string list
        Group:string
    }

    [<CLIMutable>]
    type UserDeleteModel = {
        UserId:string
    }

    [<CLIMutable>]
    type UserCreateModel = {
        Name:string
        EMail:string
        Password:string
    }
        
