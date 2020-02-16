module ProjectModels

    open Common

    type ProjectListEntryModel = {
        ProjectId:string
        Title:string
        Description:string
        IssueCount:int
    }
    with 
        static member Empty = {
            ProjectId=""
            Title=""
            Description=""
            IssueCount=0
        }


    type ProjectListPage = Paging<ProjectListEntryModel>




    [<CLIMutable>]
    type ProjectCreateModel = {
        Title:string
        Description:string
    }

