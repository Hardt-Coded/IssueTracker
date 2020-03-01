namespace Project

module Services =
    
    open System
    open System.Threading.Tasks
    open FSharp.Control.Tasks.V2    
    open Project.Domain
    open Common.Domain
    open Project.EventStore
    open Project.Dtos
    open Project.Types


    module Project =


        open Common.Services


        let private getCurrentProjectState (eventStore:ProjectEventStore) id =
            task {
                let! userEvents =
                    eventStore.ReadEvents id
                
                return userEvents |> aggregate.Exec None
            }


        let private createProject (eventStore:ProjectEventStore) (command:CommandArguments.CreateProject) = 
            (CreateProject command) 
            |> handleCommand aggregate eventStore (fun (CreateProject x) -> x.ProjectId) None


        let private deleteProject (eventStore:ProjectEventStore) (command:CommandArguments.DeleteProject) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId

                return! 
                    (DeleteProject command) 
                    |> handleCommand aggregate eventStore (fun (DeleteProject x) -> x.ProjectId) project
            }


        let private changeProjectState (eventStore:ProjectEventStore) (command:CommandArguments.ChangeProjectState) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId
                
                return! 
                    (ChangeProjectState command) 
                    |> handleCommand aggregate eventStore (fun (ChangeProjectState x) -> x.ProjectId) project
            }


        let private changeProjectTitle (eventStore:ProjectEventStore) (command:CommandArguments.ChangeProjectTitle) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId
                
                return! 
                    (ChangeProjectTitle command) 
                    |> handleCommand aggregate eventStore (fun (ChangeProjectTitle x) -> x.ProjectId) project
            }


        let private changeProjectDescription (eventStore:ProjectEventStore) (command:CommandArguments.ChangeProjectDescription) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId
                
                return! 
                    (ChangeProjectDescription command) 
                    |> handleCommand aggregate eventStore (fun (ChangeProjectDescription x) -> x.ProjectId) project
            }


        let private createIssue (eventStore:ProjectEventStore) (command:CommandArguments.CreateIssue) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId

                return! 
                    (IssueCommand <| CreateIssue command) 
                    |> handleCommand aggregate eventStore (fun (IssueCommand (CreateIssue x)) -> x.ProjectId) project
            }  


        let private deleteIssue (eventStore:ProjectEventStore) (command:CommandArguments.DeleteIssue) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId

                return! 
                    (IssueCommand <| DeleteIssue command) 
                    |> handleCommand aggregate eventStore (fun (IssueCommand (DeleteIssue x)) -> x.ProjectId) project
            }  


        let private changeIssueState (eventStore:ProjectEventStore) (command:CommandArguments.ChangeIssueState) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId

                return! 
                    (IssueCommand <| ChangeIssueState command) 
                    |> handleCommand aggregate eventStore (fun (IssueCommand (ChangeIssueState x)) -> x.ProjectId) project
            }  


        let private assignIssueToUser (eventStore:ProjectEventStore) (command:CommandArguments.AssignIssueToUser) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId

                return! 
                    (IssueCommand <| AssignIssueToUser command) 
                    |> handleCommand aggregate eventStore (fun (IssueCommand (AssignIssueToUser x)) -> x.ProjectId) project
            }  


        let private changeIssueTitle (eventStore:ProjectEventStore) (command:CommandArguments.ChangeIssueTitle) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId

                return! 
                    (IssueCommand <| ChangeIssueTitle command) 
                    |> handleCommand aggregate eventStore (fun (IssueCommand (ChangeIssueTitle x)) -> x.ProjectId) project
            }  


        let private addIssueComment (eventStore:ProjectEventStore) (command:CommandArguments.AddIssueComment) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId

                return! 
                    (IssueCommand <| AddIssueComment command) 
                    |> handleCommand aggregate eventStore (fun (IssueCommand (AddIssueComment x)) -> x.ProjectId) project
            }  


        let private changeIssueDescription (eventStore:ProjectEventStore) (command:CommandArguments.ChangeIssueDescription) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId

                return! 
                    (IssueCommand <| ChangeIssueDescription command) 
                    |> handleCommand aggregate eventStore (fun (IssueCommand (ChangeIssueDescription x)) -> x.ProjectId) project
            }  


        let private changeIssueComment (eventStore:ProjectEventStore) (command:CommandArguments.ChangeIssueComment) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId

                return! 
                    (IssueCommand <| ChangeIssueComment command) 
                    |> handleCommand aggregate eventStore (fun (IssueCommand (ChangeIssueComment x)) -> x.ProjectId) project
            }  


        let private deleteIssueComment (eventStore:ProjectEventStore) (command:CommandArguments.DeleteIssueComment) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId

                return! 
                    (IssueCommand <| DeleteIssueComment command) 
                    |> handleCommand aggregate eventStore (fun (IssueCommand (DeleteIssueComment x)) -> x.ProjectId) project
            }  


        let private addIssueAttachment (eventStore:ProjectEventStore) (command:CommandArguments.AddIssueAttachment) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId

                return! 
                    (IssueCommand <| AddIssueAttachment command) 
                    |> handleCommand aggregate eventStore (fun (IssueCommand (AddIssueAttachment x)) -> x.ProjectId) project
            }  


        let private removeIssueAttachment (eventStore:ProjectEventStore) (command:CommandArguments.RemoveIssueAttachment) = 
            task {
                let! project = getCurrentProjectState eventStore command.ProjectId

                return! 
                    (IssueCommand <| RemoveIssueAttachment command) 
                    |> handleCommand aggregate eventStore (fun (IssueCommand (RemoveIssueAttachment x)) -> x.ProjectId) project
            }  



        type ProjectService = {
            CreateProject:              CommandArguments.CreateProject->            Task<Result<unit,Errors list>>
            DeleteProject:              CommandArguments.DeleteProject->            Task<Result<unit,Errors list>>
            ChangeProjectState:         CommandArguments.ChangeProjectState->       Task<Result<unit,Errors list>>
            ChangeProjectTitle:         CommandArguments.ChangeProjectTitle->       Task<Result<unit,Errors list>>
            ChangeProjectDescription:   CommandArguments.ChangeProjectDescription-> Task<Result<unit,Errors list>>
            CreateIssue:                CommandArguments.CreateIssue->              Task<Result<unit,Errors list>>
            DeleteIssue:                CommandArguments.DeleteIssue->              Task<Result<unit,Errors list>>
            ChangeIssueState:           CommandArguments.ChangeIssueState->         Task<Result<unit,Errors list>>
            AssignIssueToUser:          CommandArguments.AssignIssueToUser->        Task<Result<unit,Errors list>>
            ChangeIssueTitle:           CommandArguments.ChangeIssueTitle->         Task<Result<unit,Errors list>>
            AddIssueComment:            CommandArguments.AddIssueComment->          Task<Result<unit,Errors list>>
            ChangeIssueDescription:     CommandArguments.ChangeIssueDescription->   Task<Result<unit,Errors list>>
            ChangeIssueComment:         CommandArguments.ChangeIssueComment->       Task<Result<unit,Errors list>>
            DeleteIssueComment:         CommandArguments.DeleteIssueComment->       Task<Result<unit,Errors list>>
            AddIssueAttachment:         CommandArguments.AddIssueAttachment->       Task<Result<unit,Errors list>>
            RemoveIssueAttachment:      CommandArguments.RemoveIssueAttachment->    Task<Result<unit,Errors list>>
        }


        let createProjectService userEventStore =
            {
                CreateProject               = createProject userEventStore
                DeleteProject               = deleteProject userEventStore
                ChangeProjectState          = changeProjectState userEventStore
                ChangeProjectTitle          = changeProjectTitle userEventStore
                ChangeProjectDescription    = changeProjectDescription userEventStore
                CreateIssue                 = createIssue userEventStore
                DeleteIssue                 = deleteIssue userEventStore
                ChangeIssueState            = changeIssueState userEventStore
                AssignIssueToUser           = assignIssueToUser userEventStore
                ChangeIssueTitle            = changeIssueTitle userEventStore
                AddIssueComment             = addIssueComment userEventStore
                ChangeIssueDescription      = changeIssueDescription userEventStore
                ChangeIssueComment          = changeIssueComment userEventStore
                DeleteIssueComment          = deleteIssueComment userEventStore
                AddIssueAttachment          = addIssueAttachment userEventStore
                RemoveIssueAttachment       = removeIssueAttachment userEventStore
            }


    


