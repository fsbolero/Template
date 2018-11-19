#r "paket: groupref fake //"
#load "Utility.fsx"

open System.IO
open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO.FileSystemOperators
open Utility

let forEachProject f =
    slnDir </> "content"
    |> Directory.GetDirectories
    |> Array.Parallel.iter f

Target.description "Create the NuGet package containing the templates."
Target.create "pack" <| fun o ->
    Fake.DotNet.Paket.pack <| fun p ->
        { p with
            OutputPath = slnDir </> "build"
            Version = getArg o "-v" "0.1.0"
        }

Target.description "Test all the template projects by building them."
Target.create "test-build" <| fun _ ->
    forEachProject <| fun dir ->
        Directory.GetFiles(dir, "*.sln")
        |> Seq.iter (dotnet dir [] "build" "%s")

Target.description "Update the dependencies (ie. paket.lock) of all template projects."
Target.create "update-deps" <| fun _ ->
    forEachProject <| fun dir ->
        shell dir ".paket/paket.exe" "update"

Target.description "Run the full release pipeline."
Target.create "release" ignore

// Main dep path with soft dependencies
"update-deps"
    ?=> "test-build"
    ?=> "pack"
    ==> "release"

// Extra hard dependencies to ensure release runs everything
"update-deps" ==> "release"
"test-build" ==> "release"

Target.runOrDefaultWithArguments "pack"
