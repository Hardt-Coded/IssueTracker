namespace Domain


//open Result
open Common.Types
open System

module User =
    
    module CommandArguments =

        type CreateUser = {
            Name:string
            EMail:string
            Password:string
        }

        type DeleteUser = {
            UserId:string
        }

        type ChangeEMail = {
            UserId:string
            EMail:string
        }

        type ChangePassword = {
            UserId:string
            Password:string
        }

        type AddToGroup = {
            UserId:string
            Group:string
        }

        type RemoveFromGroup = {
            UserId:string
            Group:string
        }


    type Command = 
        | CreateUser of CommandArguments.CreateUser
        | DeleteUser of CommandArguments.DeleteUser
        | ChangeEMail of CommandArguments.ChangeEMail
        | ChangePassword of CommandArguments.ChangePassword
        | AddToGroup of CommandArguments.AddToGroup
        | RemoveFromGroup of CommandArguments.RemoveFromGroup


    [<AutoOpen>]
    module Types =

        open System

        type UserId = private UserId of string

        type HashPair = {
            Hash:string
            Salt:string
        }

        type PasswordHash = private PasswordHash of HashPair

        module UserId =
            
            let create userId =
                if String.IsNullOrWhiteSpace(userId) then
                    sprintf "user id must not be empty" |> Error
                else
                    UserId userId |> Ok


            let value (UserId userId) = userId


        module PasswordHash =
            
            open System;
            open System.Security.Cryptography
            open Microsoft.AspNetCore.Cryptography.KeyDerivation

            let create password =
                if String.IsNullOrEmpty(password) then
                    "password must not be empty" |> Error
                else
                    let salt = Array.zeroCreate 16
                    use rng = RandomNumberGenerator.Create()
                    rng.GetBytes(salt)
                    let saltString = Convert.ToBase64String(salt)
                    let hashed = 
                        KeyDerivation.Pbkdf2(
                            password = password,
                            salt = salt,
                            prf = KeyDerivationPrf.HMACSHA1,
                            iterationCount = 10000,
                            numBytesRequested = 256 / 8
                        )
                    let hashedString = Convert.ToBase64String(hashed)
                    PasswordHash { Hash = hashedString; Salt = saltString } |> Ok

                

            let value (PasswordHash hashpair) = hashpair


    module EventArguments =

        type UserCreated = {
            UserId:UserId
            Name:NotEmptyString
            EMail:EMail
            PasswordHash:PasswordHash
        }

        type UserDeleted = {
            UserId:UserId
        }

        type EMailChanged = {
            UserId:UserId
            EMail:EMail
        }

        type PasswordChanged = {
            UserId:UserId
            PasswordHash:PasswordHash
        }

        type AddedToGroup = {
            UserId:UserId
            Group:NotEmptyString
        }

        type RemovedFromGroup = {
            UserId:UserId
            Group:NotEmptyString
        }


    type Event = 
        | UserCreated of EventArguments.UserCreated
        | UserDeleted of EventArguments.UserDeleted
        | EMailChanged of EventArguments.EMailChanged
        | PasswordChanged of EventArguments.PasswordChanged
        | AddedToGroup of EventArguments.AddedToGroup
        | RemovedFromGroup of EventArguments.RemovedFromGroup


    type State = {
        UserId:UserId
        Name:NotEmptyString
        EMail:EMail
        PasswordHash:PasswordHash
        Groups: NotEmptyString list
    }


    let handle (state:State option) command : Result<Event list,string> =
        match state,command with
        | None, CreateUser args ->
                result {
                    let! name = NotEmptyString.create "Name" args.Name
                    let! email = EMail.create args.EMail
                    let! passwordHash = PasswordHash.create args.Password
                    let! userId = UserId.create <| Guid.NewGuid().ToString("N")
                    let userCreated : EventArguments.UserCreated = {
                        UserId = userId
                        Name = name
                        EMail = email
                        PasswordHash = passwordHash
                    }
                    return [ UserCreated userCreated ]
                }
        | Some _, CreateUser _ ->
            "you can not have a create user event, when a user alread existis" |> Error
        | Some state, DeleteUser args ->
            result {
                let! userId = UserId.create args.UserId
                // does it makes any sense to check, if the user id matches?
                // i tend to no, but I leave it here
                if (userId <> state.UserId) then
                    return! "the userId does not match" |> Error
                else
                    return [ UserDeleted { UserId = userId} ]
            }
        | Some _, ChangeEMail args ->
            result {
                let! userId = UserId.create args.UserId
                let! email = EMail.create args.EMail
                return [ EMailChanged { UserId = userId; EMail = email } ]
            }
        | Some state, ChangePassword args ->
            [] |> Ok
        | Some state, AddToGroup args ->
            [] |> Ok
        | Some state, RemoveFromGroup args ->
            [] |> Ok
        | None, _ ->
            "you can not have any other event excpect of the created event on an empty state" |> Error
                
                
            





