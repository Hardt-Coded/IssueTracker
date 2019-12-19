namespace Common.Projections

module FileStorage =

    open Newtonsoft.Json
    open System.IO

        
    let storeProjection filename (data:'a list) =
        let json = JsonConvert.SerializeObject(data)
        File.WriteAllText(filename,json)


    let loadProjection<'a> filename =
        if File.Exists(filename) then
            let json = File.ReadAllText(filename)
            JsonConvert.DeserializeObject<'a list>(json)
        else
            []

                  
    





    

