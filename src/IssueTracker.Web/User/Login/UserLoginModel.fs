module UserLoginModel

    [<CLIMutable>]
    type Model = 
        {
            EMail:string
            Password:string
        }
        with 
            static member Empty = {
                EMail = ""
                Password = ""
            }


