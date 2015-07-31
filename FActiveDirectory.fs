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

        let __getDomainControllerSearchResult__ samAccount propertyToLoad (dc : DomainController)= //, samAccount : string, propertyToLoad: string) :string = 
            let searcher = dc.GetDirectorySearcher()
            searcher.Filter = String.Format("(&(samAccountName={0}))", samAccount) |> ignore
            searcher.PropertiesToLoad.Add(propertyToLoad) |> ignore
            searcher.SizeLimit = 1 |> ignore
            let SR = searcher.FindOne()
            SR.ToString()
            //DateTime.FromFileTime(Int64.Parse(SR.Properties.Item( propertyToLoad).Item( 0 ).ToString())).ToString()
            SR.Properties.Item(propertyToLoad).Item(0).ToString()

        let rec __getObjectBySAMAccount__ (samAccountName : string) = // fix this and make it an optional argument
            let searcher = new DirectorySearcher( getDomainRoot(false), String.Format("(sAMAccountName={0})", samAccountName))
            match searcher.FindOne() with
            | null -> new DirectoryEntry() // return an empty DE
            | _ -> searcher.FindOne().GetDirectoryEntry()

        // Public Construction
        member this.getAllDomainControllers () =
            let dcs = DomainController.FindAll(new DirectoryContext(DirectoryContextType.Domain, System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain().Name)) 
            // get all dcs and do a ping to see whose online
            Seq.cast dcs
            |> Seq.filter (fun (dc:DomainController) -> fComputer.Pingable dc.Name )

        // Public Properties
        member this.getObjectBySAMAccount (samAccountName : string) = 
            __getObjectBySAMAccount__ samAccountName
    
        member this.userDisabled (user, ad) =            
            printfn "Return null"

        member this.lastLogon (userPath:String) = 
            let domainControllers = this.getAllDomainControllers()
            let user = new DirectoryEntry(userPath)
            let samAccount = user.Properties.["samAccountName"].Value.ToString()
        
            List.sort [ for dc:DomainController in domainControllers -> getDomainControllerLastLogon(dc, samAccount) ]
                |> List.rev
                |> List.head 

