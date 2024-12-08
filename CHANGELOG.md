# Changelog

## 0.24

* [#50](https://github.com/fsbolero/Template/issues/50) Replace option `--server` with `--render` to decide the render mode. Possible values are:
    * `LegacyServer` for classic server-side mode.
    * `LegacyWebAssembly` for classic client-side mode.
    * `WebAssembly` for client-only WebAssembly mode without a server project.
    * `InteractiveServer` for server-side interactive render mode (see https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-8.0).
    * `InteractiveWebAssembly` for client-side interactive render mode.
    * `InteractiveAuto` for automatic interactive render mode (client-side if available, otherwise server-side while downloading the client-side runtime in the background).
* [#56](https://github.com/fsbolero/Template/issues/56) Update to paket 9.0.2

## 0.23

* [#41](https://github.com/fsbolero/Template/issues/41) Fix issue where, when creating a project containing a dash, the solution file uses an underscore in the project path.
* [#45](https://github.com/fsbolero/Template/issues/45) Add launchSettings that enable WebAssembly debugging.
* [#52](https://github.com/fsbolero/Template/issues/52) Fix namespace used in Razor host page.

## 0.22

* Use endpoint routing for remote services.

## 0.21

* [#39](https://github.com/fsbolero/Template/issues/39) Add link to Blazor-generated `CLIENT_ASSEMBLY_NAME.styles.css`.

* [#40](https://github.com/fsbolero/Template/issues/40) Enable WebAssembly debugging in the server project.

## 0.20

* Update to Bolero 0.20's computation expression-based syntax.

## 0.18

* Use GitHub Packages as source for `--nightly` rather than AppVeyor.

* Fix `--paket`.
