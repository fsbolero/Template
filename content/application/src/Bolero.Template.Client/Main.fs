module Bolero.Template.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Json
open Bolero.Remoting
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
    }

and Book =
    {
        title: string
        author: string
        [<DateTimeFormat "yyyy-MM-dd">]
        publishDate: DateTime
        isbn: string
    }

let initModel =
    {
        page = Home
        counter = 0
        books = None
        error = None
    }

/// Remote service definition.
type BookService =
    {
        getBooks: unit -> Async<Book[]>
        addBook: Book -> Async<unit>
        removeBookByIsbn: string -> Async<unit>
    }

    interface IRemoteService with
        member this.BasePath = "/books"

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | Increment
    | Decrement
    | SetCounter of int
    | GetBooks
    | GotBooks of Book[]
    | Error of exn

let update bookService message model =
    match message with
    | SetPage page ->
        let cmd =
            match page with
            | Data when Option.isNone model.books -> Cmd.ofMsg GetBooks
            | _ -> Cmd.none
        { model with page = page }, cmd
    | Increment ->
        { model with counter = model.counter + 1 }, Cmd.none
    | Decrement ->
        { model with counter = model.counter - 1 }, Cmd.none
    | SetCounter value ->
        { model with counter = value }, Cmd.none
    | GetBooks ->
        let cmd = Cmd.ofAsync bookService.getBooks () GotBooks Error
        { model with books = None }, cmd
    | GotBooks books ->
        { model with books = Some books }, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">

let home model dispatch =
    Main.Home().Elt()

let counter model dispatch =
    Main.Counter()
        .Decrement(fun _ -> dispatch Decrement)
        .Increment(fun _ -> dispatch Increment)
        .Value(model.counter, fun v -> dispatch (SetCounter v))
        .Elt()

let data model dispatch =
    Main.Data()
        .Reload(fun _ -> dispatch GetBooks)
        .Rows(cond model.books <| function
            | None ->
                Main.EmptyData().Elt()
            | Some books ->
                forEach books <| fun book ->
                    tr [] [
                        td [] [text book.title]
                        td [] [text book.author]
                        td [] [text (book.publishDate.ToString("yyyy-MM-dd"))]
                        td [] [text book.isbn]
                    ])
        .Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view model dispatch =
    Main()
        .Menu(concat [
            menuItem model Home "Home"
            menuItem model Counter "Counter"
            menuItem model Data "Download data"
        ])
        .Body(
            cond model.page <| function
            | Home -> home model dispatch
            | Counter -> counter model dispatch
            | Data -> data model dispatch
        )
        .Error(
            cond model.error <| function
            | None -> empty
            | Some err -> div [attr.classes ["notification"; "is-warning"]] [text err]
        )
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let bookService = this.Remote<BookService>()
        let update = update bookService
        Program.mkProgram (fun _ -> initModel, Cmd.none) update view
        |> Program.withRouter router
//#if (hotreload_actual)
#if DEBUG
        |> Program.withHotReloading
#endif
