namespace Domain.Types


module User =

    open System
    open Domain.Common


    type UserId = private UserId of string

    type HashPair = {
        Hash:string
        Salt:string
    }

    type PasswordHash = private PasswordHash of HashPair

    module UserId =
            
        let create userId =
            if String.IsNullOrWhiteSpace(userId) then
                sprintf "user id must not be empty" 
                |> DomainError
                |> Error
            else
                UserId userId |> Ok


        let value (UserId userId) = userId

        /// use only for event dto convertion
        let fromEventDto userId = UserId userId


    module PasswordHash =
            
        open System;
        open System.Security.Cryptography
        open Microsoft.AspNetCore.Cryptography.KeyDerivation

        let create password =
            if String.IsNullOrEmpty(password) then
                "password must not be empty" 
                |> DomainError
                |> Error
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


        let isValid password passwordHash =
            let hashPair = value passwordHash
            let salt = Convert.FromBase64String(hashPair.Salt)
            let hashed = 
                KeyDerivation.Pbkdf2(
                    password = password,
                    salt = salt,
                    prf = KeyDerivationPrf.HMACSHA1,
                    iterationCount = 10000,
                    numBytesRequested = 256 / 8
                )
            let hashedString = Convert.ToBase64String(hashed)
            hashedString = hashPair.Hash


        /// use only for event dto convertion
        let fromEventDto salt hash = PasswordHash { Hash = hash; Salt = salt }

