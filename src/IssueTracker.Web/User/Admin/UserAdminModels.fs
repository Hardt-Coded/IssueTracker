module UserAdminModels

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
        
