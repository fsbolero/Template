# MiniBlazor Simple Template

This template can be used as a base to create a MiniBlazor application.

## Requirements

To get started, you need the following installed:

* .NET Core SDK 2.1.403 with .NET Core Runtime 2.1.3. Download it [here](https://www.microsoft.com/net/download/dotnet-core/2.1).
* On Linux / OSX: Mono 5.x. Download it [here](https://www.mono-project.com/download/stable/).

To learn more, you can check [the documentation](https://github.com/intellifactory/miniblazor/wiki).

## Using this template

Visual Studio Code or Visual Studio is recommended to edit this project.

To compile the project, you can run:

```shell
dotnet build
```

To run it:

```shell
dotnet run -p src/Server
```

## Project structure

* `src/Client` is the project that gets compiled to WebAssembly, and contains your client-side code.

    * `Startup.fs` sets up Blazor to get the application started.

    * `Main.fs` contains the main body of the page.

* `src/Server` is the host ASP.NET Core application, and contains your server-side code.
