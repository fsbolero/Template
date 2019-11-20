namespace Bolero.Template.Server

open Microsoft.AspNetCore.Mvc.RazorPages
open Microsoft.Extensions.Hosting

type HostModel(env: IHostEnvironment) =
    inherit PageModel()

    static let defaultIsServer = false

    member this.IsServer =
        let mutable queryParam = Unchecked.defaultof<_>
        let mutable parsed = false
        if env.IsDevelopment()
            && this.Request.Query.TryGetValue("server", &queryParam)
            && bool.TryParse(queryParam.[0], &parsed)
        then
            parsed
        else
            defaultIsServer
