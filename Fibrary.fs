namespace Fibrary

open System.DirectoryServices
open System.DirectoryServices.ActiveDirectory
open System

module ActiveDirectory =
    type ActiveDirectoryOptionalArguments() =
        static let defaultDomainRoot() = "place.holder.for.optional.arg.stuff.later.com"
    
    let getCurrentSite =
        System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain().Name

    let getDomainRoot (globalArg : bool) =
        if globalArg then new DirectoryEntry("LDAP://GlobalCatalog")
        else new DirectoryEntry("LDAP://root ou")

    let rec getObjectBySAMAccount (samAccountName : string) =
        let searcher = new DirectorySearcher(getDomainRoot(false), String.Format("(sAMAccountName={0})", samAccountName))
        match searcher.FindOne() with
        | null -> getObjectBySAMAccount("Default or Builtin account")
        | _ -> searcher.FindOne().GetDirectoryEntry()
    
    let getGlobalCatalog (domain : string):
        printfn "Get LDAP://globalcatalog"
    let getRootOrganizationalUnit (domain : string, subOU : string): // how do we want to handle subou overrides
        printfn "Get LDAP://root ou"
