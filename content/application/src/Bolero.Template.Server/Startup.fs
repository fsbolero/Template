namespace Bolero.Template.Server

open System.IO
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Bolero
open Bolero.Remoting.Server
open Bolero.Template
//#if (hotreload_actual)
open Bolero.Templating.Server
//#endif

//#if (!minimal)
type BookService(env: IHostingEnvironment) =
    inherit RemoteHandler<Client.Main.BookService>()

    let books =
        Path.Combine(env.ContentRootPath, "data/books.json")
        |> File.ReadAllText
        |> Json.Deserialize<Client.Main.Book[]>
        |> ResizeArray

    override this.Handler =
        {
            getBooks = fun () -> async {
                return books.ToArray()
            }

            addBook = fun book -> async {
                books.Add(book)
            }

            removeBookByIsbn = fun isbn -> async {
                books.RemoveAll(fun b -> b.isbn = isbn) |> ignore
            }
        }
//#endif

type Startup() =

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =
        services
//#if (!minimal)
            .AddRemoting<BookService>()
//#endif
//#if (hotreload_actual)
#if DEBUG
            .AddHotReload(templateDir = "../Bolero.Template.Client")
#endif
//#endif
        |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
        app
//#if (!minimal)
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
