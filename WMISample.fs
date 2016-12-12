open System
open FSharp.Management

type wmi = WmiProvider<"localhost">

let data = wmi.GetDataContext()

let test =
    data.Win32_Process
    |> Seq.iter (fun process -> printfn "X: %s" process.Name)
