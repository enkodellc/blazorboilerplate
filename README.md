# <img src="https://github.com/enkodellc/blazorboilerplate/blob/master/src/BlazorBoilerplate.Client/wwwroot/images/blazorboilerplate.svg" alt="Blazor Boilerplate" height="75"/> Blazor Boilerplate

Blazor Boilerplate / Starter Template with MatBlazor and IdentityServer (Core-Hosted).  Blazor is a web framework designed to run  in the browser on a WebAssembly-based .NET runtime. 

[![Build Status](https://enkodellc.visualstudio.com/blazorboilerplate/_apis/build/status/enkodellc.blazorboilerplate?branchName=master)](https://enkodellc.visualstudio.com/blazorboilerplate/_build/latest?definitionId=1&branchName=master)
[![Live Demo](https://img.shields.io/badge/demo-online-green.svg)](https://blazorboilerplate.com)
[![GitHub Stars](https://img.shields.io/github/stars/enkodellc/blazorboilerplate.svg)](https://github.com/enkodellc/blazorboilerplate/stargazers)
[![GitHub Issues](https://img.shields.io/github/issues/enkodellc/blazorboilerplate.svg)](https://github.com/enkodellc/blazorboilerplate/issues)
[![MIT](https://img.shields.io/github/license/SamProf/MatBlazor.svg)](LICENSE)
[![Gitter](https://badges.gitter.im/BlazorBoilerplate/community.svg)](https://gitter.im/blazorboilerplate/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## Goal
To create a boilerplate code base using Blazor / MatBlazor & .Net Core that includes most of the basic functionality of a starter kit (similar to AspnetBoilerplate) that is lean yet powerful for anyone to build off. A repository that is not controlled by a corporation but by a community.

# Live demo
[Blazor Boilerplate](https://blazorboilerplate.com) - Kick the tires. The functionality grows every week. 

## Prerequisites

Don't know what Blazor is? Read [here](https://docs.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-3.0)

Complete all Blazor dependencies.

- .NET Core 3.0 Preview 6 SDK (3.0.100-preview6-012264)
- Visual Studio 2019 Preview 4 with the ASP.NET and web development workload selected.
- The latest Blazor extension from the Visual Studio Marketplace.
- The Blazor templates on the command-line: **Install-Package Microsoft.AspNetCore.Blazor.Templates -Version 3.0.0-preview6.19307.2**

### How to run
1. Install **dotnet-sdk-3.0.100-preview6-012264** and the latest **Visual Studio 2019 Preview**.
2. Clone or download.
3. Open the solution in Visual Studio and press F5.
4. Create a user using the `Create Account` button in the login page or login if you have already created a user.

## Publish on IIS
1. Publish both the Client and Server projects.
2. Upload Server project to website folder.
3. Upload Client 'BlazorBoilerplate.Client' directory 

### Thanks To
- [Blazor](https://blazor.net)
- [BlazorWithIdentity](https://github.com/stavroskasidis/BlazorWithIdentity)
- [MatBlazor](https://github.com/SamProf/MatBlazor)

## Contributing

Please star, watch and fork! We'd greatly appreciate any contribution you make. I am very open to updates and features, though most feature requests 
will be depending on how much community support for it is.

## Completed
 
 - Basic Login / User Creation
 - Admin Theme using Material Design / MatBlazor - Free to use.
 - Log Files
 - Choose between SQL Lite File or MS SQL Database
 - Email Confirmation of Registered Users
 - Forgot Password Functionality

## Road map

- Breadcrumbs.
- Setting management UI.
- User, role, permission and organization unit management UI.
- Real time chat and notification system. Slack Communication clone?
- Blog or other real world functionality.
- Audit log report UI.

## License

This project is licensed under the terms of the [MIT license](LICENSE).

## News

### 0.1.6 - In Progress
- Email confirmation on Registration 
  - Configure Email Configuration in server appsettings.json
  - Set "RequireConfirmedEmail" to true in appsettings.json
- Forgot Password
  - Configure Email Configuration in server appsettings.json
- Updated Email Templates for Forgot Password 
  
### 0.1.5
- Added Azure DevOps Pipeline for build status
- Added Update Email Templates
- Implemented New User Registration Email Template
- Implement AuthorizeView for Theme
- Breakout Theme Components 

### 0.1.4
- Add Mailkit for Email - Future work to add Email Templates
- Started API Auth CascadingAuthenticationState / Polices / Claims
- Added SQL Server as a DB option

### 0.1.3
- Update to MatBlazor 1.2.1 - Not yet released so using local copy
- Update Theme / Drawer minify & close
- Fixed Known Issue - UserProfile not loading after login thanks [nstohler](https://github.com/nstohler)
- Secure Pages.
- Anonymous Home Page.

### 0.1.2
- Update to SDK 3.0.100-preview6-012264 - Breaking Changes
- Update to MatBlazor 1.2
- Update Theme
- Known Issue - UserProfile not loading after login
- Registration Form added Form Validation
- Todo Table Fetch from API Example

###  0.1.1
- Updated Theme / Responsive
- Added Serilog Log Files

### 0.1.0
- Initial release
