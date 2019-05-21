namespace Bolero.Template.Server

open System
open System.IO
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Server
open Bolero.Template
//#if (hotreload_actual)
open Bolero.Templating.Server
//#endif

//#if (!minimal)
type BookService(env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.Main.BookService>()

    let books =
        Path.Combine(env.ContentRootPath, "data/books.json")
        |> File.ReadAllText
        |> Json.Deserialize<Client.Main.Book[]>
        |> ResizeArray

    override this.Handler =
        {
            getBooks = Remote.authorize <| fun _ () -> async {
                return books.ToArray()
            }

            addBook = Remote.authorize <| fun _ book -> async {
                books.Add(book)
            }

            removeBookByIsbn = Remote.authorize <| fun _ isbn -> async {
                books.RemoveAll(fun b -> b.isbn = isbn) |> ignore
            }

            signIn = Remote.withContext <| fun http (username, password) -> async {
                if password = "password" then
                    do! http.AsyncSignIn(username, TimeSpan.FromDays(365.))
                    return Some username
                else
                    return None
            }

            signOut = Remote.withContext <| fun http () -> async {
                return! http.AsyncSignOut()
            }

            getUsername = Remote.authorize <| fun http () -> async {
                return http.User.Identity.Name
            }
        }
//#endif

type Startup() =

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =
        services
//#if (!minimal)
            .AddAuthorization()
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .Services
            .AddRemoting<BookService>()
//#endif
//#if (hotreload_actual)
#if DEBUG
            .AddHotReload(templateDir = "../Bolero.Template.Client")
#endif
//#endif
        |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        app
//#if (!minimal)
            .UseAuthentication()
            .UseRemoting()
//#endif
//#if (hotreload_actual)
#if DEBUG
            .UseHotReload()
#endif
//#endif
            .UseBlazor<Client.Startup>()
        |> ignore

module Program =

    [<EntryPoint>]
    let main args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .Build()
            .Run()
        0
