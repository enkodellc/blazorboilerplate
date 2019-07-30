# <img src="https://github.com/enkodellc/blazorboilerplate/blob/master/src/BlazorBoilerplate.Client/wwwroot/images/blazorboilerplate.svg" alt="Blazor Boilerplate" height="75"/> Blazor Boilerplate

Blazor Boilerplate, "Blazor Starter Template" is a SPA admin template (Core-Hosted). The UI is driven by Material Design provided mostly by MatBlazor.  Blazor is a web framework designed to run  in the browser on a WebAssembly-based .NET runtime. 

[![Build Status](https://enkodellc.visualstudio.com/blazorboilerplate/_apis/build/status/enkodellc.blazorboilerplate?branchName=master)](https://enkodellc.visualstudio.com/blazorboilerplate/_build/latest?definitionId=1&branchName=master)
[![Live Demo](https://img.shields.io/badge/demo-online-green.svg)](https://blazorboilerplate.com)
[![GitHub Stars](https://img.shields.io/github/stars/enkodellc/blazorboilerplate.svg)](https://github.com/enkodellc/blazorboilerplate/stargazers)
[![GitHub Issues](https://img.shields.io/github/issues/enkodellc/blazorboilerplate.svg)](https://github.com/enkodellc/blazorboilerplate/issues)
[![MIT](https://img.shields.io/github/license/SamProf/MatBlazor.svg)](LICENSE)
[![Gitter](https://badges.gitter.im/BlazorBoilerplate/community.svg)](https://gitter.im/blazorboilerplate/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## Goals
- To create a boilerplate with Blazor / Razor components that includes the most common functionality for an app that is lean yet powerful for anyone to start their own project rapidly. 
- This repository is community driven. It is not and never will be controlled by a corporation. 
- Javascript Free or a least limited Javascript code. We may us components with JS in them but so far no Javascript has been written specifically for anything in the repository.

# Live demo
[Blazor Boilerplate](https://blazorboilerplate.com) - Kick the tires. The functionality grows every week. 

## Prerequisites
Don't know what Blazor is? Read [here](https://docs.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-3.0)

Complete all Blazor dependencies.

- .NET Core 3.0 Preview 7 SDK 3.0.100-preview7-012821
- Visual Studio 2019 Preview  with the ASP.NET and web development workload selected.
- The Blazor templates on the command-line: **dotnet new -i Microsoft.AspNetCore.Blazor.Templates::3.0.0-preview7.19365.7**
- For Entity Framework Core on the command-line tools: **dotnet tool install --global dotnet-ef --version 3.0.0-preview7.19362.6**

### How to run
1. Install **dotnet-sdk 3.0.100-preview7-012821** and the latest **Visual Studio 2019 Preview**.
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

## Disclaimer / About the Author

I (Enkodellc) started this repository as I was frustrated with the examples out there that people were charging money for and were in my opinion
incomplete or closed source. I paid for 4-5 of these solutions and none of them were what I was looking for. This is my
attempt to create something that developers can start a Blazor project with several great features to build from. 
I have a lot of experience with ASP.Net webforms. I am new to .NET Core and Blazor. 
This code is not meant to be perfect or follow every Best Practice. It though is my ambition to learn and get feedback on what Best Practices
 can be implemented. 
 I have taken small solutions from other repositories and will do my best to recognize those contributions. I am very open to ideas and 
 suggestions. I am not a great developer, but I try. So please take this into consideration when using this repository.

## Completed 
 - Basic Login / User Creation
 - Admin Theme using Material Design / MatBlazor - Free to use.
 - Log Files
 - Choose between SQL Lite File or MS SQL Database
 - Email Confirmation of Registered Users
 - Forgot Password Functionality

## Road map
- User profile & settings management.
- User, role, permission and organization unit management.
- Real time chat and notification system. Slack Communication clone?
- Blog or other real world functionality.
- Audit log report UI.

## License
This project is licensed under the terms of the [MIT license](LICENSE).

## News
### 0.1.8 (In Progress)
- Middleware to log Api Requests and Responses

### 0.1.7
- User Profile Management
- Refactor Email Settings and API
- MatBlazor 1.3.0
- Migrate to .NET Core 3.0

### 0.1.6 
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
