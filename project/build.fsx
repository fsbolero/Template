#r "paket: groupref fake //"
#load "Utility.fsx"

open System.IO
open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO.FileSystemOperators
open Utility

let contentBaseDir = slnDir </> "content"

let forEachProject f =
    contentBaseDir
    |> Directory.GetDirectories
    |> Array.Parallel.iter f

let forEachPaketDirectory f =
    Directory.GetFiles(contentBaseDir, "paket.dependencies", SearchOption.AllDirectories)
    |> Array.Parallel.iter (Path.GetDirectoryName >> f)

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
    forEachPaketDirectory <| fun dir ->
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
