namespace Domain

module Common =

    type Aggregate<'state,'command,'event> = {
        handle: 'state option -> 'command -> Result<'event list,string>
        apply: 'state option -> 'event -> Result<'state option,string>
    }

