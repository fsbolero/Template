module Bolero.Template._1.Client.Main

open System
//#if (!server)
open System.Net.Http
open System.Net.Http.Json
open Microsoft.AspNetCore.Components
//#endif
open Elmish
open Bolero
open Bolero.Html
//#if (server)
open Bolero.Remoting
open Bolero.Remoting.Client
//#endif
//#if (hotreload_actual)
open Bolero.Templating.Client
//#endif

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/counter">] Counter
    | [<EndPoint "/data">] Data

/// The Elmish application's model.
type Model =
    {
        page: Page
        counter: int
        books: Book[] option
        error: string option
//#if (server)
        username: string
        password: string
        signedInAs: option<string>
        signInFailed: bool
//#endif
    }

and Book =
    {
        title: string
        author: string
        publishDate: DateTime
        isbn: string
    }

let initModel =
    {
        page = Home
        counter = 0
        books = None
        error = None
//#if (server)
        username = ""
        password = ""
        signedInAs = None
        signInFailed = false
//#endif
    }

//#if (server)
/// Remote service definition.
type BookService =
    {
        /// Get the list of all books in the collection.
        getBooks: unit -> Async<Book[]>

        /// Add a book in the collection.
        addBook: Book -> Async<unit>

        /// Remove a book from the collection, identified by its ISBN.
        removeBookByIsbn: string -> Async<unit>

        /// Sign into the application.
        signIn : string * string -> Async<option<string>>

        /// Get the user's name, or None if they are not authenticated.
        getUsername : unit -> Async<string>

        /// Sign out from the application.
        signOut : unit -> Async<unit>
    }

    interface IRemoteService with
        member this.BasePath = "/books"
//#endif

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | Increment
    | Decrement
    | SetCounter of int
    | GetBooks
    | GotBooks of Book[]
//#if (server)
    | SetUsername of string
    | SetPassword of string
    | GetSignedInAs
    | RecvSignedInAs of option<string>
    | SendSignIn
    | RecvSignIn of option<string>
    | SendSignOut
    | RecvSignOut
//#endif
    | Error of exn
    | ClearError

//#if (server)
let update remote message model =
    let onSignIn = function
        | Some _ -> Cmd.ofMsg GetBooks
        | None -> Cmd.none
//#else
let update (http: HttpClient) message model =
//#endif
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    | Increment ->
        { model with counter = model.counter + 1 }, Cmd.none
    | Decrement ->
        { model with counter = model.counter - 1 }, Cmd.none
    | SetCounter value ->
        { model with counter = value }, Cmd.none

    | GetBooks ->
//#if (server)
        let cmd = Cmd.OfAsync.either remote.getBooks () GotBooks Error
//#else
        let getBooks() = http.GetFromJsonAsync<Book[]>("/books.json")
        let cmd = Cmd.OfTask.either getBooks () GotBooks Error
//#endif
        { model with books = None }, cmd
    | GotBooks books ->
        { model with books = Some books }, Cmd.none

//#if (server)
    | SetUsername s ->
        { model with username = s }, Cmd.none
    | SetPassword s ->
        { model with password = s }, Cmd.none
    | GetSignedInAs ->
        model, Cmd.OfAuthorized.either remote.getUsername () RecvSignedInAs Error
    | RecvSignedInAs username ->
        { model with signedInAs = username }, onSignIn username
    | SendSignIn ->
        model, Cmd.OfAsync.either remote.signIn (model.username, model.password) RecvSignIn Error
    | RecvSignIn username ->
        { model with signedInAs = username; signInFailed = Option.isNone username }, onSignIn username
    | SendSignOut ->
        model, Cmd.OfAsync.either remote.signOut () (fun () -> RecvSignOut) Error
    | RecvSignOut ->
        { model with signedInAs = None; signInFailed = false }, Cmd.none

    | Error RemoteUnauthorizedException ->
        { model with error = Some "You have been logged out."; signedInAs = None }, Cmd.none
//#endif
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

//#if (html)
type Main = Template<"wwwroot/main.html">

let homePage model dispatch =
    Main.Home().Elt()

let counterPage model dispatch =
    Main.Counter()
        .Decrement(fun _ -> dispatch Decrement)
        .Increment(fun _ -> dispatch Increment)
        .Value(model.counter, fun v -> dispatch (SetCounter v))
        .Elt()

//#if (server)
let dataPage model (username: string) dispatch =
//#else
let dataPage model dispatch =
//#endif
    Main.Data()
        .Reload(fun _ -> dispatch GetBooks)
//#if (server)
        .Username(username)
        .SignOut(fun _ -> dispatch SendSignOut)
//#endif
        .Rows(cond model.books <| function
            | None ->
                Main.EmptyData().Elt()
            | Some books ->
                forEach books <| fun book ->
                    tr {
                        td { book.title }
                        td { book.author }
                        td { book.publishDate.ToString("yyyy-MM-dd") }
                        td { book.isbn }
                    })
        .Elt()

//#if (server)
let signInPage model dispatch =
    Main.SignIn()
        .Username(model.username, fun s -> dispatch (SetUsername s))
        .Password(model.password, fun s -> dispatch (SetPassword s))
        .SignIn(fun _ -> dispatch SendSignIn)
        .ErrorNotification(
            cond model.signInFailed <| function
            | false -> empty()
            | true ->
                Main.ErrorNotification()
                    .HideClass("is-hidden")
                    .Text("Sign in failed. Use any username and the password \"password\".")
                    .Elt()
        )
        .Elt()
//#endif

let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view model dispatch =
    Main()
        .Menu(concat {
            menuItem model Home "Home"
            menuItem model Counter "Counter"
            menuItem model Data "Download data"
        })
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | Counter -> counterPage model dispatch
            | Data ->
//#if (server)
                cond model.signedInAs <| function
                | Some username -> dataPage model username dispatch
                | None -> signInPage model dispatch
//#else
                dataPage model dispatch
//#endif
        )
        .Error(
            cond model.error <| function
            | None -> empty()
            | Some err ->
                Main.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()
//#else
let homePage model dispatch =
    div {
        attr.``class`` "content"
        h1 { attr.``class`` "title"; "Welcome to Bolero!" }
        p { "This application demonstrates Bolero's major features." }
        ul {
            li {
                "The entire application is driven by "
                a {
                    attr.target "_blank"
                    attr.href "https://fsbolero.github.io/docs/Elmish"
                    "Elmish"
                }
                "."
            }
            li {
                "The menu on the left switches pages based on "
                a {
                    attr.target "_blank"
                    attr.href "https://fsbolero.github.io/docs/Routing"
                    "routes"
                }
                "."
            }
            li {
                "The "
                a { router.HRef Counter; "Counter" }
                " page demonstrates event handlers and data binding in "
                a {
                    attr.target "_blank"
                    attr.href "https://fsbolero.github.io/docs/Templating"
                    "HTML templates"
                }
                "."
            }
            li {
                "The "
                a { router.HRef Data; "Download data" }
//#if (server)
                " page demonstrates the use of "
                a {
                    attr.target "_blank"
                    attr.href "https://fsbolero.github.io/docs/Remoting"
                    "remote functions"
                }
                "."
//#else
                " page demonstrates the use of HTTP requests to the server."
//#endif
            }
            p { "Enjoy writing awesome apps!" }
        }
    }

let counterPage model dispatch =
    concat {
        h1 { attr.``class`` "title"; "A simple counter" }
        p {
            button {
                on.click (fun _ -> dispatch Decrement)
                attr.``class`` "button"
                "-"
            }
            input {
                attr.``type`` "number"
                attr.id "counter"
                attr.``class`` "input"
                bind.input.int model.counter (fun v -> dispatch (SetCounter v))
            }
            button {
                on.click (fun _ -> dispatch Increment)
                attr.``class`` "button"
                "+"
            }
        }
    }

//#if (server)
let dataPage model (username: string) dispatch =
//#else
let dataPage model dispatch =
//#endif
    concat {
        h1 {
            attr.``class`` "title"
            "Download data "
            button {
                attr.``class`` "button"
                on.click (fun _ -> dispatch GetBooks)
                "Reload"
            }
        }
//#if (server)
        p {
            $"Signed in as {username}. "
            button {
                attr.``class`` "button"
                on.click (fun _ -> dispatch SendSignOut)
                "Sign out"
            }
        }
//#endif
        table {
            attr.``class`` "table is-fullwidth"
            thead {
                tr {
                    th { "Title" }
                    th { "Author" }
                    th { "Published" }
                    th { "ISBN" }
                }
            }
            tbody {
                cond model.books <| function
                | None ->
                    tr {
                        td { attr.colspan 4; "Downloading book list..." }
                    }
                | Some books ->
                    forEach books <| fun book ->
                        tr {
                            td { book.title }
                            td { book.author }
                            td { book.publishDate.ToString("yyyy-MM-dd") }
                            td { book.isbn }
                        }
            }
        }
    }

let errorNotification errorText closeCallback =
    div {
        attr.``class`` "notification is-warning"
        cond closeCallback <| function
        | None -> empty()
        | Some closeCallback -> button { attr.``class`` "delete"; on.click closeCallback }
        text errorText
    }

//#if (server)
let field (content: Node) = div { attr.``class`` "field"; content }
let control (content: Node) = div { attr.``class`` "control"; content }

let inputField (fieldLabel: string) (inputAttrs: Attr) =
    field (concat {
        label { attr.``class`` "label"; fieldLabel }
        control (input { attr.``class`` "input"; inputAttrs })
    })

let signInPage model dispatch =
    concat {
        h1 { attr.``class`` "title"; "Sign in" }
        form {
            on.submit (fun _ -> dispatch SendSignIn)
            inputField "Username" (
                bind.input.string model.username (fun s -> dispatch (SetUsername s))
            )
            inputField "Password" (attrs {
                attr.``type`` "password"
                bind.input.string model.password (fun s -> dispatch (SetPassword s))
            })
            field (
                control (
                    input { attr.``type`` "submit"; attr.value "Sign in" }
                )
            )
            cond model.signInFailed <| function
            | false -> empty()
            | true -> errorNotification "Sign in failed. Use any username and the password \"password\"." None
        }
    }

//#endif
let menuItem (model: Model) (page: Page) (itemText: string) =
    li {
        a {
            attr.``class`` (if model.page = page then "is-active" else "")
            router.HRef page
            itemText
        }
    }

let view model dispatch =
    div {
        attr.``class`` "columns"
        aside {
            attr.``class`` "column sidebar is-narrow"
            section {
                attr.``class`` "section"
                nav {
                    attr.``class`` "menu"
                    ul {
                        attr.``class`` "menu-list"
                        menuItem model Home "Home"
                        menuItem model Counter "Counter"
                        menuItem model Data "Download data"
                    }
                }
            }
        }
        div {
            attr.``class`` "column"
            section {
                attr.``class`` "section"
                cond model.page <| function
                | Home -> homePage model dispatch
                | Counter -> counterPage model dispatch
                | Data ->
//#if (server)
                    cond model.signedInAs <| function
                    | Some username -> dataPage model username dispatch
                    | None -> signInPage model dispatch
//#else
                    dataPage model dispatch
//#endif
                div {
                    attr.id "notification-area"
                    cond model.error <| function
                    | None -> empty()
                    | Some err -> errorNotification err (Some (fun _ -> dispatch ClearError))
                }
            }
        }
    }
//#endif

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override _.CssScope = CssScopes.MyApp

//#if (!server)
    [<Inject>]
    member val HttpClient = Unchecked.defaultof<HttpClient> with get, set

//#endif
    override this.Program =
//#if (server)
        let bookService = this.Remote<BookService>()
        let update = update bookService
        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetSignedInAs) update view
//#else
        let update = update this.HttpClient
        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetBooks) update view
//#endif
        |> Program.withRouter router
//#if (hotreload_actual)
#if DEBUG
        |> Program.withHotReload
#endif
//#endif
