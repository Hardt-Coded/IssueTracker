namespace Domain

module Common =

    type Errors =
        | DomainError of string
        | InfrastructureError of exn

    type Aggregate<'state,'command,'event> = {
        handle: 'state option -> 'command -> Result<'event list,Errors>
        apply: 'state option -> 'event -> Result<'state option,Errors>
    }

    

