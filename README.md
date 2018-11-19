# Bolero Simple Template

This template can be used as a base to create a Bolero application.

To learn more, you can check [the documentation](https://github.com/intellifactory/bolero/wiki).

## Requirements

To get started, you need the following installed:

* .NET Core SDK 2.1.403 with .NET Core Runtime 2.1.3. Download it [here](https://www.microsoft.com/net/download/dotnet-core/2.1).
* On Linux / OSX: Mono 5.x. Download it [here](https://www.mono-project.com/download/stable/).

## Creating a project based on this template

To create a project based on this template, first install the template to your local dotnet:

```
dotnet new -i Bolero.Templates
```

Then, you can create a project like so:

```
dotnet new bolero-app -o your-app-name
```

This will create a project in a new folder named `your-app-name`.

## Template options

You can use the following options to customize the project being created:

* `--server=true|false`:

    If `true`, the solution includes a `Server` project, which is an ASP.NET Core server that hosts the application.
    
    If `false`, the solution only contains the `Client` project that is compiled to WebAssembly.
    
    The default is `server=true`.

## Using this template

Visual Studio Code or Visual Studio is recommended to edit this project.

To compile the project, you can run:

```shell
dotnet build
```

To run it:

```shell
dotnet run -p src/Server

# Or if you created the project with --server=false:
dotnet run -p src/Client
```

## Project structure

* `src/Client` is the project that gets compiled to WebAssembly, and contains your client-side code.

    * `Startup.fs` sets up Blazor to get the application started.

    * `Main.fs` contains the main body of the page.

* `src/Server` is the host ASP.NET Core application, and contains your server-side code.

## Learn more about Bolero

To learn more about Bolero, you can check [the documentation](https://github.com/intellifactory/bolero/wiki).
