namespace Fibrary

open Fibrary.Core
open Fibrary.FComputer
open System
open System.Diagnostics
open System.Diagnostics.Eventing.Reader
open System.Text

module FEventInterface =
    type public FEventWriter(source:String, log:String) =       
        member this.FEventWriter()=
            match EventLog.SourceExists(this.source) with
            | false -> EventLog.CreateEventSource(this.source, this.log)

        member this.writeEntry(message: String, eventID: int, ?eventType: EventLogEntryType)=
            match eventType.IsNone with
            | true -> EventLog.WriteEntry(this.source, message, EventLogEntryType.Information, eventID)
            | false -> EventLog.WriteEntry(this.source, message, eventType.Value, eventID)

        member this.log = log
        member this.source = source
    type public FEventReader() =
        // lots of refactoring required!!!

        member this.queryEventLog(queryString: String) :EventLogReader=
            let eventLog = new EventLog("Application")
            let eventsQuery = new EventLogQuery("Application", PathType.LogName, "Query String Placeholder")
            let logReader = new EventLogReader(eventsQuery)
            logReader // for F# noobs this means to return this as the results
        member this.queryEventLog(queryString: String, computer: String) :EventLogReader=
            // As a friednd of mine once said "Code is self documenting"
            let eventLog = new EventLog("Application", computer)
            let eventsQuery = new EventLogQuery("Application", PathType.LogName, "Query String Placeholder")
            eventsQuery.Session = new EventLogSession(computer) |> ignore

            let logReader = new EventLogReader(eventsQuery)
            logReader // for F# noobs this means to return this as the results


        member this.queryGenerator() :String =
            // take in some arbitrary data or linq query and turn it into a string query that can be passed into a query mechanism // either xpath or xml can work
            let ty = 5
            "ddd"

        member this.bsTest :EventLogReader= 
            // just all sorts of tripping overmyself tonight
            // so this is the behavior the end user will eventually use.

            // Next Session I will flush out the query generator and maybe add an async method for processing events read
            this.queryEventLog(this.queryGenerator(), "remoteMachine")
        




