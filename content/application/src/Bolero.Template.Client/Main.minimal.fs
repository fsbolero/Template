module Bolero.Template.Client.Main

open Elmish
open Bolero
open Bolero.Html
//#if (hotreload_actual)
open Bolero.Templating.Client
//#endif

type Model =
    {
        x: string
    }

let initModel =
    {
        x = ""
    }

type Message =
    | Ping

let update message model =
    match message with
    | Ping -> model

let view model dispatch =
    text "Hello, world!"

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        Program.mkSimple (fun _ -> initModel) update view
//#if (hotreload_actual)
#if DEBUG
        |> Program.withHotReload
#endif
