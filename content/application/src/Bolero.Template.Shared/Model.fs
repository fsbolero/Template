namespace Bolero.Template.Shared

//#if (minimal)
// Put in this project all the code that is common between the client and the server:
// common model type definitions, remote service definitions, etc.
//#else
open System
open Bolero.Json

type Book =
    {
        title: string
        author: string
        [<DateTimeFormat "yyyy-MM-dd">]
        publishDate: DateTime
        isbn: string
    }
//#endif
