namespace Common

open System.Text.RegularExpressions

module Types =

    open Domain

    type EMail = private EMail of string

    type NotEmptyString = private NotEmptyString of string



    module EMail =
        
        let private pattern = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$"
        let private compiledRegex = 
            Regex(pattern, 
                RegexOptions.Compiled ||| 
                RegexOptions.IgnoreCase ||| 
                RegexOptions.ExplicitCapture)


        let create email =
            if compiledRegex.IsMatch(email) then
                EMail email |> Ok
            else
                "invalid email adress" 
                |> DomainError
                |> Error


        let value (EMail email) = email


        /// use only for event dto convertion
        let fromEventDto email = EMail email


    module NotEmptyString =

        open System

        let create label notEmptyStr =
            if String.IsNullOrWhiteSpace(notEmptyStr) then
                sprintf "%s must not be empty" label
                |> DomainError
                |> Error
            else
                NotEmptyString notEmptyStr |> Ok


        let value (NotEmptyString str) = str

        
        /// use only for event dto convertion
        let fromEventDto str = NotEmptyString str
                
            
        
                
            
            

    

