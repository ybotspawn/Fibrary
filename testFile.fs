// testFile.fs

//Get last Logons, synchronously
let userSearcher = new DirectorySearcher(new DirectoryEntry("LDAP://OU=Company,dc=ricky,dc=bobby,dc=com"), String.Format("(&(objectClass=user))"))

// lame
for sr in userSearcher.FindAll() do
    let user = new DirectoryEntry(sr.Path)
    printfn "%s" user.Name
    for dc in getAllDomainControllers() do
        printfn "\t%s: %s" dc.Name (getLastLogon (dc, user.Properties.["samAccountName"].Value.ToString()))
printfn "Done."
Console.Read() |> ignore
