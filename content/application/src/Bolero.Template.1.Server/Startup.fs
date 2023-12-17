module Bolero.Template._1.Server.Program

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Bolero
open Bolero.Remoting.Server
open Bolero.Server
open Bolero.Template._1
//#if (hotreload_actual)
open Bolero.Templating.Server
//#endif

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

//#if (hostpage == "razor")
    builder.Services.AddMvc().AddRazorRuntimeCompilation() |> ignore
//#else
    builder.Services.AddMvc() |> ignore
//#endif
    builder.Services.AddServerSideBlazor() |> ignore
//#if (!minimal)
    builder.Services.AddAuthorization()
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie()
    |> ignore
    builder.Services.AddBoleroRemoting<BookService>() |> ignore
//#endif
//#if (hostpage != "html")
    builder.Services.AddBoleroHost() |> ignore
//#endif
//#if (hotreload_actual)
#if DEBUG
    builder.Services.AddHotReload(templateDir = __SOURCE_DIRECTORY__ + "/../Bolero.Template.1.Client") |> ignore
#endif
//#endif

    let app = builder.Build()

    if app.Environment.IsDevelopment() then
        app.UseWebAssemblyDebugging()

    app
        .UseAuthentication()
        .UseStaticFiles()
        .UseRouting()
        .UseAuthorization()
        .UseBlazorFrameworkFiles()
    |> ignore

//#if (hotreload_actual)
#if DEBUG
    app.UseHotReload()
#endif
//#endif
    app.MapBoleroRemoting() |> ignore
//#if (hostpage == "razor")
    app.MapBlazorHub() |> ignore
    app.MapFallbackToPage("/_Host") |> ignore
//#elseif (hostpage == "bolero")
    app.MapBlazorHub() |> ignore
    app.MapFallbackToBolero(Index.page) |> ignore
//#elseif (hostpage == "html")
    app.MapControllers() |> ignore
    app.MapFallbackToFile("index.html") |> ignore
//#endif
    |> ignore

    app.Run()
    0
