namespace Fibrary


open Fibrary.Core
open Fibrary.FComputer
open System
open System.Diagnostics
open System.Text

module fEventInterface =
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
    type public FEventReader()=
        let x = 5       
        member this.queryGenerator() :String =
            // take in some arbitrary data or linq query and turn it into a string query that can be passed into a query mechanism // either xpath or xml can work
            let ty = 5
            "ddd"
