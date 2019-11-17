namespace Projections

module UserList =


    open Infrastructure

    type State = {
        UserId:string
        Name:string
        EMail:string
        Groups: string list
    }



    type Messages =
        | UpdateProjection
        | GetUserList of AsyncReplyChannel<State list>


    // the projection will update itself every 3 seconds for it's own.
    // Than it wait'S for 3 seconds on a new message
    // also there is a message, what updates the projects
    let userListProjection =
        MailboxProcessor.Start(fun inbox ->
            async {

                let refreshUsers state =
                    async {
                        
                        EventStore.User.

                        return state
                    }


                let rec agentLoop state =
                    async {
                    
                        let! msg = inbox.TryReceive 3000
                        match msg with
                        | None ->
                            let! newState = refreshUsers state
                            return! agentLoop newState
                        | Some msg ->
                            match msg with
                            | UpdateProjection ->
                                let! newState = refreshUsers state
                                return! agentLoop newState
                            | GetUserList reply ->
                                reply.Reply state
                                return! agentLoop state
                    }


                let! currentState = refreshUsers []
                
                return! agentLoop currentState
            }
        )
