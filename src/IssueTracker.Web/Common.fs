module Common

    
    [<AutoOpen>]
    module Helpers =
       

        let inline parseInt (s:string) =
            let (b,v) = System.Int32.TryParse(s)
            if b then Some v else None



    [<AutoOpen>]
    module DataTypes =
        
        type Paging<'a> ={
                Page:int
                Count:int
                MaxPage:int
                Data:'a list
            }
            with
                static member Empty data =
                    {
                        Page = 0
                        Count = 0
                        MaxPage = 0
                        Data = data
                    }