namespace Domain

module Common =

    type Errors =
        | DomainError of string
        | InfrastructureError of exn

    

    type Aggregate<'state,'command,'event, 'error> = {
        handle: 'state option -> 'command -> Result<'event list,Errors>
        apply: 'state option -> 'event -> 'state option
        exec: 'state option -> Result<('event * int) list option,'error> -> 'state option
        execWithVersion: 'state option -> Result<('event * int) list option,'error> -> ('state * int) option
    }


    let private execBase 
        (onEvents: ('event * int) list -> 'state option) 
        (events: Result<('event * int) list option,'error>) =
        match events with
        | Ok None ->
            None
        | Ok (Some events) ->
            onEvents events
        | Error e ->
            failwith "apply should not has any errors"


    let exec (apply:'state option -> 'event -> 'state option) state events =
        let onEvents (events: ('event * int) list) =
            let state =
                (state, events)
                ||> List.fold (fun state (event,version) -> event |> apply state)
            state

        execBase onEvents events

    let execWithEvents 
        (apply:'state option -> 'event -> 'state option) 
        state
        (events: Result<('event * int) list option,'error>) =
        let onEvents events =
            let state =
                ((state,0), events)
                ||> List.fold (fun state (event,version) -> 
                    let (state,_) = state
                    event |> apply state, version)
                

            let (state,version) = state
            state |> Option.map ( fun i -> i,version)

        execBase onEvents events
        

    

