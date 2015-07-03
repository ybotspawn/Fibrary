namespace Fibrary

open System.DirectoryServices
open System.DirectoryServices.ActiveDirectory
open System

// fix my default arguments
// http://stackoverflow.com/questions/15988913/why-default-arguments-in-f-fsharpoptiont-are-reference-types
module ActiveDirectory =
//    let __getAllDomainControllers =
//        
//        printfn "getting all domain controllers"
//         private List<DomainController> _getDCLoop()
//        {
//            List<DomainController> dcs = new List<DomainController>();
//            while (true)
//            {
//                try
//                {
//                    var dc = DomainController.FindOne(new DirectoryContext(DirectoryContextType.Domain, SITE));
//                    if (dcs.Contains(dc))
//                        break;
//                    else
//                        dcs.Add(dc);                    
//                }
//                catch (Exception aoe)
//                {
//                    // log warning, probably ok to ignore
//                }
//            }
//
//            return dcs;
//        }
// try
//            {
//                DCC = DomainController.FindAll(new DirectoryContext(DirectoryContextType.Domain, SITE));
//                var dcs = (from DomainController dc in DCC select dc).ToList();
//                return dcs;
//            }
//            catch (ActiveDirectoryOperationException aoe)
//            {
//                return null;
//            }


//Construction
let getRootOrganizationUnit (domain : string) = // how do we want to handle subou overrides? let the implementer override by building their directoryentry directly
    // check a given value somewhere for the override value?
    new DirectoryEntry(String.Format("LDAP://DC={0}", domain.Replace(".", ",DC=")))

let getCurrentSite =
    System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain().Name

let getGlobalCatalog (domain : string) =
    new DirectoryEntry(String.Format(@"GC://{0}", getCurrentSite))
    //let gc = GlobalCatalog.FindOne(new DirectoryContext(DirectoryContextType.Domain))

let getDomainRoot (globalArg : bool) =
    if globalArg then getGlobalCatalog(getCurrentSite)
    else getRootOrganizationUnit(getCurrentSite)

let getAllDomainControllers =
    printfn "Getting all dc's"

// Properties
let rec getObjectBySAMAccount (samAccountName : string) = // fix this and make it an optional argument
    let searcher = new DirectorySearcher( getDomainRoot(false), String.Format("(sAMAccountName={0})", samAccountName))
    match searcher.FindOne() with
    | null -> new DirectoryEntry() // return an empty DE
    | _ -> searcher.FindOne().GetDirectoryEntry()

let userDisabled (user, ad) = 
    printfn "Return null"
    
let getIntProperty ( objectParam : string, property: string) =
    printfn "string property"
let getStringProperty ( directoryEntry : DirectoryEntry, property: string) =
    let prop = directoryEntry.Properties.[property].Value.ToString()
    printfn "string property"
let getDateProperty ( samAccountName :string, property: string ) = // change this to take a type in as well to determine the kind of object param we are passing, ie sam, de, etc
    printfn "lastpwdset"
//    public string getStringProperty(String distinguishedName, String propertyValue)
//    {
//        if (!distinguishedName.StartsWith(@"LDAP://"))
//            distinguishedName = String.Format(@"LDAP://{0}", distinguishedName);
//        String property = "undefined";
//        DirectoryEntry tempDe = null;
//        try
//        {
//            tempDe = new DirectoryEntry(distinguishedName);
//            property = tempDe.Properties[propertyValue].Value.ToString();
//
//        }
//        catch (Exception e)
//        {
//            // log event here
//        }
//        tempDe.Close();
//        return property;
//    }
    