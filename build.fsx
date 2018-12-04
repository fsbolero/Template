#r "paket: groupref fake //"
#load ".paket/Utility.fsx"

open System.IO
open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO.FileSystemOperators
open Utility

// Command-line parameters
let version = getArg "-v" "0.1.0"

// Constants
let contentBaseDir = slnDir </> "content"
let buildOutputDir = slnDir </> "build"
let packageOutputFile o = buildOutputDir </> sprintf "Bolero.Templates.%s.nupkg" (version o)
let variantsToTest =
    [
        "NoServer", "--server=false"
        "WithServer", "--server=true"
    ]

Target.description "Create the NuGet package containing the templates."
Target.create "pack" <| fun o ->
    Fake.DotNet.Paket.pack <| fun p ->
        { p with
            OutputPath = buildOutputDir
            Version = version o
        }

Target.description "Test all the template projects by building them."
Target.create "test-build" <| fun o ->
    // Install the newly created template
    dotnet slnDir [] "new" "-i %s" (packageOutputFile o)

    // For each template variant, create and build a new project
    let baseDir = Path.GetTempPath()
    for name, args in variantsToTest do
        // Prepend a letter and change extension to avoid generating
        // identifiers that start with a number.
        let projectName = "A" + Path.GetRandomFileName()
        let projectName = Path.ChangeExtension(projectName, name)
        dotnet baseDir [] "new" "bolero-app %s -o %s" args projectName
        dotnet (baseDir </> projectName) [] "build" ""
        Directory.Delete(baseDir </> projectName, true)

Target.description "Update the dependencies (ie. paket.lock) of all template projects."
Target.create "update-deps" <| fun _ ->
    Directory.GetFiles(contentBaseDir, "paket.dependencies", SearchOption.AllDirectories)
    |> Array.Parallel.iter (Path.GetDirectoryName >> fun dir ->
        shell dir ".paket/paket.exe" "update"
    )

Target.description "Run the full release pipeline."
Target.create "release" ignore

// Main dep path with soft dependencies
"update-deps"
    ?=> "pack"
    ==> "test-build"
    ==> "release"

// Extra hard dependencies to ensure release runs everything
"update-deps" ==> "release"

Target.runOrDefaultWithArguments "pack"
