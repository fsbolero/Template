module Bolero.Template.Client.Main

open Elmish
open Bolero
open Bolero.Html
//#if (hotreload_actual)
open Bolero.Templating.Client
//#endif

type Model =
    {
        value: int
    }

let initModel =
    {
        value = 0
    }

type Message =
    | Increment
    | Decrement

let update message model =
    match message with
    | Increment -> { model with value = model.value + 1 }
    | Decrement -> { model with value = model.value - 1 }

//#if (hotreload_actual)
type Button = Template<"wwwroot/button.html">
//#endif

let view model dispatch =
    concat [
//#if (hotreload_actual)
        Button().Text("-").Click(fun _ -> dispatch Decrement).Elt()
//#else
        button [on.click (fun _ -> dispatch Decrement)] [text "-"]
//#endif
        span [] [textf " %i " model.value]
//#if (hotreload_actual)
        Button().Text("+").Click(fun _ -> dispatch Increment).Elt()
//#else
        button [on.click (fun _ -> dispatch Increment)] [text "+"]
//#endif
    ]

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        Program.mkSimple (fun _ -> initModel) update view
//#if (hotreload_actual)
#if DEBUG
        |> Program.withHotReloading
#endif
