module Bolero.Template.Client.Main

open Elmish
open Bolero
open Bolero.Html
open Bolero.Json
open Bolero.Remoting
open Bolero.Remoting.Client
//#if (hotreload_actual)
open Bolero.Templating.Client
//#endif
open Bolero.Template.Shared

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
        username: string
        password: string
        signedInAs: option<string>
        signInFailed: bool
    }

let initModel =
    {
        page = Home
        counter = 0
        books = None
        error = None
        username = ""
        password = ""
        signedInAs = None
        signInFailed = false
    }

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | Increment
    | Decrement
    | SetCounter of int
    | GetBooks
    | GotBooks of Book[]
    | SetUsername of string
    | SetPassword of string
    | GetSignedInAs
    | RecvSignedInAs of option<string>
    | SendSignIn
    | RecvSignIn of option<string>
    | SendSignOut
    | RecvSignOut
    | Error of exn
    | ClearError

let update remote message model =
    let onSignIn = function
        | Some _ -> Cmd.ofMsg GetBooks
        | None -> Cmd.none
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
        let cmd = Cmd.ofAsync remote.getBooks () GotBooks Error
        { model with books = None }, cmd
    | GotBooks books ->
        { model with books = Some books }, Cmd.none

    | SetUsername s ->
        { model with username = s }, Cmd.none
    | SetPassword s ->
        { model with password = s }, Cmd.none
    | GetSignedInAs ->
        model, Cmd.ofAuthorized remote.getUsername () RecvSignedInAs Error
    | RecvSignedInAs username ->
        { model with signedInAs = username }, onSignIn username
    | SendSignIn ->
        model, Cmd.ofAsync remote.signIn (model.username, model.password) RecvSignIn Error
    | RecvSignIn username ->
        { model with signedInAs = username; signInFailed = Option.isNone username }, onSignIn username
    | SendSignOut ->
        model, Cmd.ofAsync remote.signOut () (fun () -> RecvSignOut) Error
    | RecvSignOut ->
        { model with signedInAs = None; signInFailed = false }, Cmd.none

    | Error RemoteUnauthorizedException ->
        { model with error = Some "You have been logged out."; signedInAs = None }, Cmd.none
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

let dataPage model (username: string) dispatch =
    Main.Data()
        .Reload(fun _ -> dispatch GetBooks)
        .Username(username)
        .SignOut(fun _ -> dispatch SendSignOut)
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

let signInPage model dispatch =
    Main.SignIn()
        .Username(model.username, fun s -> dispatch (SetUsername s))
        .Password(model.password, fun s -> dispatch (SetPassword s))
        .SignIn(fun _ -> dispatch SendSignIn)
        .ErrorNotification(
            cond model.signInFailed <| function
            | false -> empty
            | true ->
                Main.ErrorNotification()
                    .HideClass("is-hidden")
                    .Text("Sign in failed. Use any username and the password \"password\".")
                    .Elt()
        )
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
            | Home -> homePage model dispatch
            | Counter -> counterPage model dispatch
            | Data ->
                cond model.signedInAs <| function
                | Some username -> dataPage model username dispatch
                | None -> signInPage model dispatch
        )
        .Error(
            cond model.error <| function
            | None -> empty
            | Some err ->
                Main.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()
//#else
let homePage model dispatch =
    div [attr.``class`` "content"] [
        h1 [attr.``class`` "title"] [text "Welcome to Bolero!"]
        p [] [text "This application demonstrates Bolero's major features."]
        ul [] [
            li [] [
                text "The entire application is driven by "
                a [
                    attr.target "_blank"
                    attr.href "https://fsbolero.github.io/docs/Elmish"
                ] [text "Elmish"]
                text "."
            ]
            li [] [
                text "The menu on the left switches pages based on "
                a [
                    attr.target "_blank"
                    attr.href "https://fsbolero.github.io/docs/Routing"
                ] [text "routes"]
                text "."
            ]
            li [] [
                text "The "
                a [router.HRef Counter] [text "Counter"]
                text " page demonstrates event handlers and data binding in "
                a [
                    attr.target "_blank"
                    attr.href "https://fsbolero.github.io/docs/Templating"
                ] [text "HTML templates"]
                text "."
            ]
            li [] [
                text "The "
                a [router.HRef Data] [text "Download data"]
                text " page demonstrates the use of "
                a [
                    attr.target "_blank"
                    attr.href "https://fsbolero.github.io/docs/Remoting"
                ] [text "remote functions"]
                text "."
            ]
            p [] [text "Enjoy writing awesome apps!"]
        ]
    ]

let counterPage model dispatch =
    concat [
        h1 [attr.``class`` "title"] [text "A simple counter"]
        p [] [
            button [
                on.click (fun _ -> dispatch Decrement)
                attr.``class`` "button"
            ] [text "-"]
            input [
                attr.``type`` "number"
                attr.id "counter"
                attr.``class`` "input"
                bind.inputInt model.counter (fun v -> dispatch (SetCounter v))
            ]
            button [
                on.click (fun _ -> dispatch Increment)
                attr.``class`` "button"
            ] [text "+"]
        ]
    ]

let dataPage model (username: string) dispatch =
    concat [
        h1 [attr.``class`` "title"] [
            text "Download data "
            button [
                attr.``class`` "button"
                on.click (fun _ -> dispatch GetBooks)
            ] [text "Reload"]
        ]
        p [] [
            textf "Signed in as %s. " username
            button [
                attr.``class`` "button"
                on.click (fun _ -> dispatch SendSignOut)
            ] [text "Sign out"]
        ]
        table [attr.``class`` "table is-fullwidth"] [
            thead [] [
                tr [] [
                    th [] [text "Title"]
                    th [] [text "Author"]
                    th [] [text "Published"]
                    th [] [text "ISBN"]
                ]
            ]
            tbody [] [
                cond model.books <| function
                | None ->
                    tr [] [
                        td [attr.colspan 4] [text "Downloading book list..."]
                    ]
                | Some books ->
                    forEach books <| fun book ->
                        tr [] [
                            td [] [text book.title]
                            td [] [text book.author]
                            td [] [text (book.publishDate.ToString("yyyy-MM-dd"))]
                            td [] [text book.isbn]
                        ]
            ]
        ]
    ]

let field content = div [attr.``class`` "field"] content
let control content = div [attr.``class`` "control"] content

let inputField fieldLabel inputAttrs =
    field [
        label [attr.``class`` "label"] [text fieldLabel]
        control [input (attr.``class`` "input" :: inputAttrs)]
    ]

let errorNotification errorText closeCallback =
    div [attr.``class`` "notification is-warning"] [
        cond closeCallback <| function
        | None -> empty
        | Some closeCallback -> button [attr.``class`` "delete"; on.click closeCallback] []
        text errorText
    ]

let signInPage model dispatch =
    concat [
        h1 [attr.``class`` "title"] [text "Sign in"]
        form [on.submit (fun _ -> dispatch SendSignIn)] [
            inputField "Username" [
                bind.input model.username (fun s -> dispatch (SetUsername s))
            ]
            inputField "Password" [
                attr.``type`` "password"
                bind.input model.password (fun s -> dispatch (SetPassword s))
            ]
            field [
                control [
                    input [attr.``type`` "submit"; attr.value "Sign in"]
                ]
            ]
            cond model.signInFailed <| function
            | false -> empty
            | true -> errorNotification "Sign in failed. Use any username and the password \"password\"." None
        ]
    ]

let menuItem (model: Model) (page: Page) (itemText: string) =
    li [] [
        a [
            attr.``class`` (if model.page = page then "is-active" else "")
            router.HRef page
        ] [text itemText]
    ]

let view model dispatch =
    div [attr.``class`` "columns"] [
        aside [attr.``class`` "column sidebar is-narrow"] [
            section [attr.``class`` "section"] [
                nav [attr.``class`` "menu"] [
                    ul [attr.``class`` "menu-list"] [
                        menuItem model Home "Home"
                        menuItem model Counter "Counter"
                        menuItem model Data "Download data"
                    ]
                ]
            ]
        ]
        div [attr.``class`` "column"] [
            section [attr.``class`` "section"] [
                cond model.page <| function
                | Home -> homePage model dispatch
                | Counter -> counterPage model dispatch
                | Data ->
                    cond model.signedInAs <| function
                    | Some username -> dataPage model username dispatch
                    | None -> signInPage model dispatch
                div [attr.id "notification-area"] [
                    cond model.error <| function
                    | None -> empty
                    | Some err -> errorNotification err (Some (fun _ -> dispatch ClearError))
                ]
            ]
        ]
    ]
//#endif

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let bookService = this.Remote<BookService>()
        let update = update bookService
        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetSignedInAs) update view
        |> Program.withRouter router
//#if (hotreload_actual)
#if DEBUG
        |> Program.withHotReload
#endif
