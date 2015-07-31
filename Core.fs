namespace Fibrary

open System
open System.Net
open System.Net.NetworkInformation
open System.Text

module Core =

    let dateTimeFromInt64 ( bigInt : string) :DateTime =
        System.DateTime.FromFileTime(Int64.Parse(bigInt))
    let dateTimetoString(dateTime: DateTime) :String = // Do extension keywords exist in F# so we can access this implictly?
        dateTime.ToString()
