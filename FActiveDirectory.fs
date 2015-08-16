namespace Fibrary

open System.DirectoryServices
open System.DirectoryServices.ActiveDirectory
open System
open Fibrary.Core
open Fibrary.FComputer
open System.Net
open System.Net.NetworkInformation
open System.Text

module FActiveDirectory =
    type public FActiveDirectory() =
        // this should reference a static instance
        let fComputer = new FComputer()
        
        type AdsUserFlagsEnum = Script = 1 | AccountDisabled = 2 | HomeDirectoryRequired = 8 | AccountLockedOut = 16 | PasswordNotRequired = 32 | PasswordCannotChange = 64 | EncryptedTextPasswordAllowed = 128 | TempDuplicateAccount = 256 | NormalAccount = 512 | InterDomainTrustAccount = 2048 | WorkstationTrustAccount = 4096 | ServerTrustAccount = 8192 | PasswordDoesNotExpire = 65536 | MnsLogonAccount = 131072 | SmartCardRequired = 262144 | TrustedForDelegation = 524288 | AccountNotDelegated = 1048576 | UseDesKeyOnly = 2097152 | DontRequirePreauth = 4194304 | PasswordExpired = 8388608 | TrustedToAuthenticateForDelegation = 16777216 | NoAuthDataRequired = 33554432
        
        // Private Construction Methods
        let getRootOrganizationUnit (domain : string) = // how do we want to handle subou overrides? let the implementer override by building their directoryentry directly
            // check a given value somewhere for the override value?
            new DirectoryEntry(String.Format("LDAP://DC={0}", domain.Replace(".", ",DC=")))
        
        //let gc = GlobalCatalog.FindOne(new DirectoryContext(DirectoryContextType.Domain))
        let getGlobalCatalog (domain : string) =
            new DirectoryEntry(String.Format(@"GC://{0}", System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain().Name))

        let getDomainRoot (globalArg : bool) =
            if globalArg then getGlobalCatalog(System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain().Name)
            else getRootOrganizationUnit(System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain().Name)

        let getDomainControllerLastLogon (dc: DomainController, samAccount: String) =
            try
                let searcher = new DirectorySearcher(new DirectoryEntry("LDAP://" + dc.Name), String.Format("(&(samAccountName={0}))", samAccount),[|"lastLogon"|])
                dateTimeFromInt64(searcher.FindOne().Properties.Item("lastLogon").Item(0).ToString())
            with
                | :? System.ArgumentOutOfRangeException as aooRex -> DateTime.MinValue
                | :? System.NullReferenceException as nRex -> DateTime.MinValue

        // Private Properties
        let __getIntProperty__(itemTest : string, anotherProperty: string) = 
            -1
       
       let getObjectDomainControllerPropertyValue (dc: DomainController, idKey:string, idValue: string, propertyToLoad: string)= // could still generalize this weith custom callbacks
        try
            let searcher = new DirectorySearcher(new DirectoryEntry("LDAP://" + dc.Name), String.Format("(&({0}={1}))", idKey, idValue),[|propertyToLoad|])
                //let user = new DirectoryEntry(value)
            searcher.FindOne().Properties.Item(propertyToLoad).Item(0).ToString()
        with
            | :? System.ArgumentOutOfRangeException as aooRex -> String.Empty

        let rec __getObjectBySAMAccount__ (samAccountName : string) = // fix this and make it an optional argument
            let searcher = new DirectorySearcher( getDomainRoot(false), String.Format("(sAMAccountName={0})", samAccountName))
            match searcher.FindOne() with
            | null -> new DirectoryEntry() // return an empty DE
            | _ -> searcher.FindOne().GetDirectoryEntry()

        // Public Construction
        
        let lastLogon (userPath:String, domainControllers: seq<DomainController>) = // pass in the seq<DomainController> collection to properly encapsulate this in the fActiveDirectory.fs
            try
                let user = new DirectoryEntry(userPath)
                let samAccount = user.Properties.["samAccountName"].Value.ToString()
                Console.Write("+")
                List.sort [ for dc:DomainController in domainControllers -> getDomainControllerLastLogon(dc, samAccount) ]
                    |> List.rev
                    |> List.head
            with 
                | :? System.Runtime.InteropServices.COMException as commex -> DateTime.MinValue
        
        member this.getAllDomainControllers () =
            let dcs = DomainController.FindAll(new DirectoryContext(DirectoryContextType.Domain, System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain().Name)) 
            // get all dcs and do a ping to see whose online
            Seq.cast dcs
            |> Seq.filter (fun (dc:DomainController) -> fComputer.Pingable dc.Name )

        // Public Properties
        let getAccountDisabled (de: DirectoryEntry) :bool=
            try
                let userFlags = de.Properties.["userAccountControl"].Value :?> AdsUserFlagsEnum
                userFlags.HasFlag(AdsUserFlagsEnum.AccountDisabled)
            with
                | :? System.DirectoryServices.DirectoryServicesCOMException as dsComEx -> false
        let getCreationDate (samAccount: String) =
            try
                let searcher = new DirectorySearcher(String.Format("(&(samAccountName={0}))", samAccount),[|"whenCreated"|])
                DateTime.Parse(searcher.FindOne().Properties.Item("whenCreated").Item(0).ToString())
            with
                | :? System.ArgumentOutOfRangeException as aooRex -> DateTime.MinValue
                | :? System.NullReferenceException as nRex -> DateTime.MinValue
                | :? System.Runtime.InteropServices.COMException as commex -> DateTime.MinValue
        let getChangedDate (samAccount: String) =
            try
                let searcher = new DirectorySearcher(String.Format("(&(samAccountName={0}))", samAccount),[|"whenChanged"|])
                DateTime.Parse(searcher.FindOne().Properties.Item("whenChanged").Item(0).ToString())
            with
                | :? System.ArgumentOutOfRangeException as aooRex -> DateTime.MinValue
                | :? System.NullReferenceException as nRex -> DateTime.MinValue
                | :? System.Runtime.InteropServices.COMException as commex -> DateTime.MinValue
        let getExpirationDate (samAccount: String) =
            try
                let searcher = new DirectorySearcher(String.Format("(&(samAccountName={0}))", samAccount),[|"accountExpires"|])
                dateTimeFromInt64(searcher.FindOne().Properties.Item("accountExpires").Item(0).ToString())
            with
                | :? System.ArgumentOutOfRangeException as aooRex -> DateTime.MinValue
                | :? System.NullReferenceException as nRex -> DateTime.MinValue
                | :? System.Runtime.InteropServices.COMException as commex -> DateTime.MinValue
        let getLastLogonTimestamp (samAccount: String) =
            try
                let searcher = new DirectorySearcher(String.Format("(&(samAccountName={0}))", samAccount),[|"lastLogonTimestamp"|])
                dateTimeFromInt64(searcher.FindOne().Properties.Item("lastLogonTimestamp").Item(0).ToString())
            with
                | :? System.ArgumentOutOfRangeException as aooRex -> DateTime.MinValue
                | :? System.NullReferenceException as nRex -> DateTime.MinValue
                | :? System.Runtime.InteropServices.COMException as commex -> DateTime.MinValue
        
        member this.getObjectBySAMAccount (samAccountName : string) = 
            __getObjectBySAMAccount__ samAccountName
    
        member this.userDisabled (user, ad) =            
            printfn "Return null"

        member this.lastLogon (userPath:String, domainControllers: seq<DomainController>) = 
            try
                let user = new DirectoryEntry(userPath)
                let samAccount = user.Properties.["samAccountName"].Value.ToString()
                List.sort [ for dc:DomainController in domainControllers -> getDomainControllerLastLogon(dc, samAccount) ]
                    |> List.rev
                    |> List.head
            with
                | :? System.Runtime.InteropServices.COMException as commex -> DateTime.MinValue

