namespace Domain

open Common.Types

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
            Name:NotEmptyString
            EMail:EMail
            Password:PasswordHash
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
            Password:PasswordHash
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


    type UserState = {
        UserId:UserId
        Name:NotEmptyString
        EMail:EMail
        Password:NotEmptyString
    }





