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
    let root = getDomainRoot(false)

    let searcher = new DirectorySearcher(root, String.Format("(sAMAccountName={0})", samAccountName))
    match searcher.FindOne() with
    | null -> getObjectBySAMAccount("Default or Builtin account")
    | _ -> searcher.FindOne().GetDirectoryEntry()
