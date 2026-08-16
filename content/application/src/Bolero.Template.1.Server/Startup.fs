module Bolero.Template._1.Server.Program

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Hosting.StaticWebAssets
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Bolero
open Bolero.Remoting.Server
open Bolero.Server
open Bolero.Template._1
//#if (hotreload_actual)
open Bolero.Templating.Server
//#endif

#nowarn 20 // Ignore the return value of app and builder methods

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

//#if (isInteractive)
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents()
//#elseif (hostpage == "razor")
    builder.Services.AddMvc().AddRazorRuntimeCompilation()
//#else
    builder.Services.AddMvc()
//#endif
    builder.Services.AddServerSideBlazor()
    builder.Services.AddAuthorization()
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie()
//#if (!minimal)
    builder.Services.AddBoleroRemoting<BookService>()
//#endif
//#if (isInteractive)
    builder.Services.AddBoleroComponents()
//#elseif (hostpage != "html")
    builder.Services.AddBoleroHost(server = RENDER_SERVER)
//#endif
//#if (hotreload_actual)
#if DEBUG
    builder.Services.AddHotReload(templateDir = __SOURCE_DIRECTORY__ + "/../Bolero.Template.1.Client")
#endif
//#endif

    let app = builder.Build()

#if DEBUG
    StaticWebAssetsLoader.UseStaticWebAssets(app.Environment, app.Configuration)
#endif

    if app.Environment.IsDevelopment() then
        app.UseWebAssemblyDebugging()

//#if (!isInteractive)
    app.UseBlazorFrameworkFiles()
//#endif
    app.UseAuthentication()
    app.UseStaticFiles()
    app.UseRouting()
    app.UseAuthorization()
//#if (isInteractive)
    app.UseAntiforgery()
//#endif
//#if (hotreload_actual)
#if DEBUG
    app.UseHotReload()
#endif
//#endif

    app.MapStaticAssets()
    app.MapBoleroRemoting()
//#if (isInteractive)
    app.MapRazorComponents<Index.Page>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof<Client.Main.MyApp>.Assembly)
//#elseif (hostpage == "razor")
    app.MapBlazorHub()
    app.MapFallbackToPage("/_Host")
//#elseif (hostpage == "bolero")
    app.MapBlazorHub()
    app.MapFallbackToBolero(Index.page)
//#elseif (hostpage == "html")
    app.MapControllers()
    app.MapFallbackToFile("index.html")
//#endif

    app.Run()
    0
