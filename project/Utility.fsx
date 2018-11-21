#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO.FileSystemOperators

let slnDir = __SOURCE_DIRECTORY__ </> ".."

let dotnet dir env cmd args =
    Printf.kprintf (fun args ->
        let env =
            (Process.createEnvironmentMap(), env)
            ||> List.fold (fun map (k, v) -> Map.add k v map)
        let r =
            DotNet.exec
                (DotNet.Options.withWorkingDirectory dir
                >> DotNet.Options.withEnvironment env)
                cmd args
        for msg in r.Results do
            eprintfn "%s" msg.Message
        if not r.OK then
            failwithf "dotnet %s failed" cmd
    ) args

let shell dir cmd args =
    Printf.kprintf (fun args ->
        match Shell.Exec(cmd, args, dir) with
        | 0 -> ()
        | n -> failwithf "%s %s failed with code %i" cmd args n
    ) args

/// `cache f x` returns `f x` the first time,
/// and re-returns the first result on subsequent calls.
let cache f =
    let res = ref None
    fun x ->
        match !res with
        | Some y -> y
        | None ->
            let y = f x
            res := Some y
            y

let getArgOpt prefix = cache <| fun (o: TargetParameter) ->
    let rec go = function
        | s :: m :: _ when s = prefix -> Some m
        | _ :: rest -> go rest
        | [] -> None
    go o.Context.Arguments

let getArg prefix ``default`` =
    getArgOpt prefix
    >> Option.defaultValue ``default``
