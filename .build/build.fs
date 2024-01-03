module Build

open System.IO
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Utility

let rec private getArgImpl prefix = function
    | s :: m :: _ when s = prefix -> Some m
    | _ :: rest -> getArgImpl prefix rest
    | [] -> None

let getArgOpt prefix = cache <| fun (o: TargetParameter) ->
    getArgImpl prefix o.Context.Arguments

let getArg prefix ``default`` =
    getArgOpt prefix
    >> Option.defaultValue ``default``

let getArgWith prefix ``default`` = cache <| fun (o: TargetParameter) ->
    match getArgImpl prefix o.Context.Arguments with
    | Some x -> x
    | None -> ``default`` o

let getFlag flag = cache <| fun (o: TargetParameter) ->
    List.contains flag o.Context.Arguments

// Command-line parameters
let version = getArgOpt "-v" >> Option.defaultWith (fun () ->
    (dotnetOutput "nbgv" ["get-version"; "-v"; "SemVer2"]).Trim()
)
let cleanTest o = getArg "--clean-test" "false" o |> System.Boolean.TryParse ||> (&&)

// Constants
let contentBaseDir = slnDir </> "content"
let buildOutputDir = slnDir </> "build"
let packageName = "Bolero.Templates"
let packageOutputFile o = buildOutputDir </> $"{packageName}.{version o}.nupkg"
let variantsToTest =
    [
        for pwak, pwav in [("Pwa", "true"); ("NoPwa", "false")] do
            // Server
            for hostk, hostv in [("Bolero", "bolero"); ("Razor", "razor"); ("Html", "html")] do
                for htmlk, reloadv, htmlv in [("Reload", "true", "true"); ("NoReload", "false", "true"); ("NoHtml", "false", "false")] do
                    for minik, miniv in [("Minimal", "true"); ("Full", "false")] do
                        if not (miniv = "true" && htmlv = "true") then
                            $"{minik}.Server{hostk}.{htmlk}.{pwak}", [
                                "--server"
                                $"--minimal={miniv}"
                                $"--hostpage={hostv}"
                                $"--pwa={pwav}"
                                $"--html={htmlv}"
                                $"--hotreload={reloadv}"
                            ]
                            if (hostv = "bolero") then
                                for renderk, renderv in [("IntServer", "InteractiveServer");("IntWasm", "InteractiveWebAssembly");("IntAuto", "InteractiveAuto");] do
                                    $"{minik}.{renderk}.{hostk}.{htmlk}.{pwak}", [
                                        "--server"
                                        $"--minimal={miniv}"
                                        $"--hostpage={hostv}"
                                        $"--pwa={pwav}"
                                        $"--html={htmlv}"
                                        $"--hotreload={reloadv}"
                                        $"--render={renderv}"
                                    ]
            // Client
            for htmlk, htmlv in [("Html", "true"); ("NoHtml", "false")] do
                for minik, miniv in [("Minimal", "true"); ("Full", "false")] do
                    if not (miniv = "true" && htmlv = "true") then
                        $"{minik}.NoServer.{htmlk}.{pwak}", [
                            "--server=false"
                            $"--minimal={miniv}"
                            $"--pwa={pwav}"
                            $"--html={htmlv}"
                        ]
    ]

Target.description "Create the NuGet package containing the templates."
Target.create "pack" <| fun o ->
    Shell.cp_r ".paket" "content/application/.paket"
    Paket.pack <| fun p ->
        { p with
            OutputPath = buildOutputDir
            Version = version o
            ToolType = ToolType.CreateLocalTool()
        }

Target.description "Install the locally built template. Warning: uninstalls any previously installed version."
Target.create "install" <| fun o ->
    if (dotnetOutput "new" ["list"]).Contains("bolero-app") then
        dotnet "new" ["uninstall"; packageName]
    dotnet "new" ["install"; packageOutputFile o; "--force"]

Target.description "Test all the template projects by building them."
Target.create "test-build" <| fun o ->
    // For each template variant, create and build a new project
    let testsDir = slnDir </> "test-build"
    if cleanTest o && Directory.Exists(testsDir) then
        Directory.Delete(testsDir, recursive = true)
    let now = System.DateTime.Now
    let baseDir = testsDir </> now.ToString("yyyy-MM-dd.HH.mm.ss")
    Directory.CreateDirectory(baseDir) |> ignore
    for name, args in variantsToTest do
        // Prepend a letter and change extension to avoid generating
        // identifiers that start with a number.
        let projectName = "Test." + name
        dotnet' baseDir [] "new" [
            yield "bolero-app"
            yield "--nightly"
            yield! args
            yield "-o"
            yield projectName
        ]
        dotnet' (baseDir </> projectName) [] "build" ["-v"; "n"]

Target.description "Run the full release pipeline."
Target.create "release" ignore

// Main dep path with soft dependencies
"pack"
    ==> "install"
    ==> "test-build"
    ==> "release"
|> ignore

Target.runOrDefaultWithArguments "pack"
