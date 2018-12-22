# Bolero Simple Template

[![Build status](https://ci.appveyor.com/api/projects/status/2mo3n85bv4d5iv56?svg=true)](https://ci.appveyor.com/project/IntelliFactory/bolero-template)


This template can be used as a base to create a [Bolero](https://github.com/intellifactory/bolero) application.

To learn more, you can check [the documentation](https://github.com/intellifactory/bolero/wiki).

## Requirements

To get started, you need the following installed:

* .NET Core SDK 2.1.403 or newer. Download it [here](https://www.microsoft.com/net/download/dotnet-core/2.1).

## Creating a project based on this template

To create a project based on this template, first install the template to your local dotnet:

```
dotnet new -i Bolero.Templates
```

Then, you can create a project like so:

```
dotnet new bolero-app -o YourAppName
```

This will create a project in a new folder named `YourAppName`.

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
dotnet run -p src/YourAppName.Server

# Or if you created the project with --server=false:
dotnet run -p src/YourAppName.Client
```

## Project structure

* `src/YourAppName.Client` is the project that gets compiled to WebAssembly, and contains your client-side code.

    * `Startup.fs` sets up Blazor to get the application started.

    * `Main.fs` contains the main body of the page.

* `src/YourAppName.Server` is the host ASP.NET Core application, and contains your server-side code.

## Learn more about Bolero

To learn more about Bolero, you can check [the documentation](https://github.com/intellifactory/bolero/wiki).
