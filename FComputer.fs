namespace Fibrary

open System.Net.NetworkInformation

module FComputer =

    type public FComputer()=
    
        // remember that (value:type) means to pass in an object of type
        // :type means return value
        member this.Pingable (computer : string) :bool=
            let p = new Ping()
            // this needs to have a try catch finally
            p.Send(computer).Status.Equals(IPStatus.Success)