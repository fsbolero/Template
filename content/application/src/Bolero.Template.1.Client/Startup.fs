namespace Bolero.Template._1.Client

open Microsoft.AspNetCore.Components.WebAssembly.Hosting
//#if (server)
open Bolero.Remoting.Client
//#else
open Microsoft.Extensions.DependencyInjection
open System
open System.Net.Http
//#endif

module Program =

    [<EntryPoint>]
    let Main args =
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
//#if (!isInteractive)
        builder.RootComponents.Add<Main.MyApp>("#main")
//#endif
//#if (server)
        builder.Services.AddBoleroRemoting(builder.HostEnvironment) |> ignore
//#else
        builder.Services.AddScoped<HttpClient>(fun _ ->
            new HttpClient(BaseAddress = Uri builder.HostEnvironment.BaseAddress)) |> ignore
//#endif
        builder.Build().RunAsync() |> ignore
        0
