namespace Common

module Domain =

    type Errors =
        | DomainError of string
        | InfrastructureError of exn

    

    type Aggregate<'state,'command,'event, 'error> = {
        Handle: 'state option -> 'command -> Result<'event list,Errors>
        Apply: 'state option -> 'event -> 'state option
        Exec: 'state option -> Result<('event * int64) list,'error> -> 'state option
        ExecWithVersion: 'state option -> Result<('event * int64) list,'error> -> ('state * int64) option
    }


    let private execBase 
        (onEvents: ('event * int64) list -> 'state option) 
        (events: Result<('event * int64) list,'error>) =
        match events with
        | Ok [] ->
            None
        | Ok events ->
            onEvents events
        | Error e ->
            failwith "apply should not has any errors"


    let exec (apply:'state option -> 'event -> 'state option) state events =
        let onEvents (events: ('event * int64) list) =
            let state =
                (state, events)
                ||> List.fold (fun state (event,version) -> event |> apply state)
            state
        
        execBase onEvents events

    let execWithEvents 
        (apply:'state option -> 'event -> 'state option) 
        state
        (events: Result<('event * int64) list,'error>) =
        let onEvents events =
            let state =
                ((state,0L), events)
                ||> List.fold (fun state (event,version) -> 
                    let (state,_) = state
                    event |> apply state, version)
                

            let (state,version) = state
            state |> Option.map ( fun i -> i,version)

        execBase onEvents events
        

    

