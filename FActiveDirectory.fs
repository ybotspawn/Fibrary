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

        // Private Properties
        let __getIntProperty__(itemTest : string, anotherProperty: string) = 
            -1
       
        
        let __getLastLogon__(samAccountName : string) =
            //http://fsharpforfunandprofit.com/posts/control-flow-expressions/
            // we need to keep in mind if has to have a return
            // so
            // if ping response is false then return a min datetime
            // else return the actual value
            
            let activeDomainControllers = this.getAllDomainControllers 

            // map the dcs to the function for the users lastLogon
            // lastLogons now has every lastLogon value from the dcs
            let lastLogons = activeDomainControllers |> List.map __getDCResult__ 

            // cast LastLogons to a datetime list and sort; yield top
            DateTime.MinValue      

        // change property to load to a tuple
        let replace oldStr newStr (s:string) = 
            s.Replace(oldValue=oldStr, newValue=newStr)
            // this expression is evaluated standalone
            // consider the following, from http://fsharpforfunandprofit.com/posts/partial-application
            // cant we write our own Domain Controller getdirectorysearcher?
//            let result = 
//                "hello"
//                |> replace "h" "j"
//                |> startswith "j" 
            let tests = [1;2;3]
            let test = tests.in
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
            let dcs = DomainController.FindAll(new DirectoryContext(DirectoryContextType.Domain, "dios-tech.com")) 
            // get all dcs and do a ping to see whose online
            Seq.cast dcs
            |> Seq.filter (fun (dc:DomainController) -> fComputer.Pingable dc.Name )

        // Public Properties
        member this.getObjectBySAMAccount (samAccountName : string) = 
            __getObjectBySAMAccount__ samAccountName
    
        member this.userDisabled (user, ad) =            
            printfn "Return null"

        member this.lastLogon (samAccountName : string) = 
//            let lastLogons = 
//                Seq.cast this.getAllDomainControllers 
//                |> Seq.iter __getDomainControllerSearchResult__ 
// the problem is that you need to also pass in the proeprty and the samaccount, along with the DC; so seq.iter will fail!!!
//            
            let lastLogons = ["1122";"1112";"12223"]
            let castLastLogons = lastLogons |> Seq.map dateTimeFromInt64
            castLastLogons
            ////DateTime.FromFileTime(Int64.Parse('t))
            ////DateTime.FromFileTime(Int64.Parse(SR.Properties.Item( propertyToLoad).Item( 0 ).ToString())).ToString()
    

    //    type private getIntProperty ( objectParam : string, property: string) =
    //        printfn "string property"
    //    let getStringProperty ( directoryEntry : DirectoryEntry, property: string) =
    //        let prop = directoryEntry.Properties.[property].Value.ToString()
    //        printfn "string property"
    //    let getDateProperty ( samAccountName :string, property: string ) = // change this to take a type in as well to determine the kind of object param we are passing, ie sam, de, etc
    //        printfn "lastpwdset"

    // fix my default arguments
    // http://stackoverflow.com/questions/15988913/why-default-arguments-in-f-fsharpoptiont-are-reference-types
    //module ActiveDirectory =
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