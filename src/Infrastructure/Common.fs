

namespace Infrastructure

open Microsoft.WindowsAzure.Storage.Table
open System.Collections
open System.Collections.Generic

module Common =

    type DictionaryTableEntity () =
        inherit TableEntity () 
        let mutable _properties = 
            Dictionary<string,EntityProperty>() :> IDictionary<string,EntityProperty>

        override this.ReadEntity(properties,operationContext) =
            _properties <- properties

        override this.WriteEntity(operationContext) =
            _properties

        member this.GetProperties () = 
            _properties
            


