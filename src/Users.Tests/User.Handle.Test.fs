namespace Users.Tests.Domain

open System
open Xunit
open Swensen.Unquote
open Users.Domain
open Users.Types
open Common.Types
open Common.Domain

module HandleTest =
    

    [<Fact>]
    let ``create user should return a user created event if all is good`` () =
        let command = CreateUser {
            UserId = "myawesomeUser01"
            EMail = "mymail@test.com"
            Name = "Daniel"
            Password = "secret"
        }

        let result = Users.Domain.aggregate.Handle None command
        
        match result, command with
        | Error e, _ ->
            failwith (sprintf "test error: %A" e)
        | Ok [ UserCreated result ], CreateUser command ->
            test <@ String.IsNullOrEmpty(UserId.value result.UserId) |> not @>
            test <@ (NotEmptyString.value result.Name) = command.Name @>
            test <@ (EMail.value result.EMail) = command.EMail @>
            test <@ PasswordHash.isValid command.Password result.PasswordHash @>
            // the password hash must check on a seperate way
        | Ok _, _ ->
            failwith "test error: result do not contain expected UserCreated Event"
                
            
        
