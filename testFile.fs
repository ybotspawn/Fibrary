// testFile.fs

let main() = 
    // instead of spelling out the root directory entry below, people could simply pass the directoryentry from one of the results from getAllDomainControllers method in fActiveDirectory
    let userSearcher = new DirectorySearcher(new DirectoryEntry("LDAP://OU=Company,dc=ricky,dc=bobby,dc=com"), String.Format("(&(objectCategory=person)(objectClass=user))"), PageSize=10000)

    let start = DateTime.Now
    let users = Seq.cast (userSearcher.FindAll() )
    let userLastLogons = 
        Async.Parallel [ for sr:SearchResult in users -> async { return (new DirectoryEntry(sr.Path), lastLogon(sr.Path)) } ]
        |> Async.RunSynchronously

    for user,lastLogon in userLastLogons do
        let samAccount = user.Properties.["samAccountName"].Value.ToString()
        let userLastLogon = lastLogon.ToString()
        printfn "%s -> %s" samAccount userLastLogon
    let total = DateTime.Now - start
    let totalDateTime = total.ToString()
    
    printfn "Done in %s." totalDateTime
    Console.Read() |> ignore
main()
