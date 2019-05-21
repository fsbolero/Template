namespace Bolero.Template.Client

open Microsoft.AspNetCore.Blazor.Hosting
open Microsoft.AspNetCore.Components.Builder
open Microsoft.Extensions.DependencyInjection
open Bolero.Remoting.Client

type Startup() =

    member __.ConfigureServices(services: IServiceCollection) =
        services.AddRemoting()

    member __.Configure(app: IComponentsApplicationBuilder) =
        app.AddComponent<Main.MyApp>("#main")

module Program =

    [<EntryPoint>]
    let Main args =
        BlazorWebAssemblyHost.CreateDefaultBuilder()
            .UseBlazorStartup<Startup>()
            .Build()
            .Run()
        0
