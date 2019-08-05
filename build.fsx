#r "paket: groupref fake //"
#load ".paket/Utility.fsx"

open System.IO
open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO.FileSystemOperators
open Utility

// Command-line parameters
let version = getArgOpt "-v" >> Option.defaultWith (fun () ->
    CreateProcess.fromRawCommand ".paket/nbgv" ["get-version"; "-v"; "SemVer2"]
    |> CreateProcess.redirectOutput
    |> Proc.run
    |> fun r -> r.Result.Output.Trim()
)
let cleanTest o = getArg "--clean-test" "false" o |> System.Boolean.TryParse ||> (&&)
let forceVersion = getArgOpt "--force-version"

// Constants
let contentBaseDir = slnDir </> "content"
let buildOutputDir = slnDir </> "build"
let packageOutputFile o = buildOutputDir </> sprintf "Bolero.Templates.%s.nupkg" (version o)
let variantsToTest =
    [
        "Server.Reload", "--server --hotreload --minimal=false"
        "Server.NoReload", "--server --hotreload=false --minimal=false"
        "Minimal.Server.Reload", "--server --hotreload --minimal"
        "Minimal.Server.NoReload", "--server --hotreload=false --minimal"
        "Minimal.NoServer.NoReload", "--server=false --hotreload=false --minimal"
    ]

Target.description "Create the NuGet package containing the templates."
Target.create "pack" <| fun o ->
    Fake.DotNet.Paket.pack <| fun p ->
        { p with
            OutputPath = buildOutputDir
            Version = version o
            ToolPath = ".paket/paket"
        }

Target.description "Test all the template projects by building them."
Target.create "test-build" <| fun o ->
    // Install the newly created template
    dotnet slnDir [] "new" "-i %s" (packageOutputFile o)

    // For each template variant, create and build a new project
    let testsDir = __SOURCE_DIRECTORY__ </> "test-build"
    if cleanTest o && Directory.Exists(testsDir) then
        Directory.Delete(testsDir, recursive = true)
    let now = System.DateTime.Now
    let baseDir = testsDir </> now.ToString("yyyy-MM-dd.HH.mm.ss")
    Directory.CreateDirectory(baseDir) |> ignore
    for name, args in variantsToTest do
        // Prepend a letter and change extension to avoid generating
        // identifiers that start with a number.
        let projectName = "Test." + name
        dotnet baseDir [] "new" "bolero-app --nightly %s -o %s" args projectName
        dotnet (baseDir </> projectName) [] "build" "-v n"

Target.description "Run the full release pipeline."
Target.create "release" ignore

Target.create "pre" <| fun _ ->
    shell "." "git" "checkout .paket/Paket.Restore.targets"

// Main dep path with soft dependencies
"pre"
    ==> "pack"
    ==> "test-build"
    ==> "release"

Target.runOrDefaultWithArguments "pack"
