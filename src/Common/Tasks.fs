module TaskHelpers

    open FSharp.Control.Tasks.V2
    open System.Threading.Tasks


    module Task =

       let inline bind (f:'a -> Task<'b>) (t:Task<'a>) = 
           task {
               let! r1 = t
               let! r2 = f(r1)
               return r2
           }

       let inline map (f:'a -> 'b) (t:Task<'a>) = 
           task { 
               let! r = t
               return f(r) 
           }


    module TaskResult =


        let inline map (f:'a -> 'b) (t:Task<Result<'a,'e>>) =
            task {
                let! r = t
                let mappedR = r |> Result.map(f)
                return mappedR
            }


        let inline map2 (f:('a * 'b) -> 'c) ((t1:Task<Result<'a,'e>>),(t2:Task<Result<'b,'e>>)) =
            task {
                let! r1 = t1
                let! r2 = t2
                match r1,r2 with
                | Ok r1, Ok r2 -> 
                    let res = f(r1,r2)
                    return Ok res
                | Error e1, _ -> return Error e1
                | _, Error e2 -> return Error e2
       
            }


        let inline bind (f:'a -> Task<'b>) (t:Task<Result<'a,'e>>) =
            task {
                let! r = t
                match r with
                | Ok r ->
                    let! mappedR = f(r)
                    return Ok mappedR
                | Error e ->
                    return Error e
            }


        let inline mapError (f:'e -> 'e2) (t:Task<Result<'a,'e>>) =
            task {
                let! r = t
                return r |> Result.mapError f
            }


    module TaskOption =


        let inline bind (f:'a -> Task<'b>) (t:Task<'a option>) =
            task {
                let! r = t
                match r with
                | None ->   
                    return None
                | Some r ->
                    let! result = f(r)
                    return Some result
            }


        let inline map (f:'a -> 'b) (t:Task<'a option>) =
            task {
                let! r = t
                return r |> Option.map(fun r -> f(r))
            }




        let inline resultOptionTaskBind (f:'a -> Task<'b>) (t:Task<Result<'a,'e> option>) =
            task {
                let! r = t
                match r with
                | Some r -> 
                    match r with
                    | Ok r ->
                        let! mappedR = f(r)
                        return Ok mappedR |> Some
                    | Error e ->
                        return Error e |> Some
                | None -> return None
            }

