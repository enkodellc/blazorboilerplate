# <img src="https://github.com/enkodellc/blazorboilerplate/blob/master/src/BlazorBoilerplate.Client/wwwroot/images/Blazor-Boilerplate-Title.png" alt="Blazor Boilerplate" style="max-wdith:100%"/>

Blazor is a web framework designed to run  in the browser on a WebAssembly-based .NET runtime. Blazor Boilerplate aka Blazor Starter Template is a SPA admin template (Core-Hosted) built with Blazor with a .NET Core 3 Server API. The UI for this application is by Material Design provided mostly by MatBlazor.  


[![Build Status](https://enkodellc.visualstudio.com/blazorboilerplate/_apis/build/status/enkodellc.blazorboilerplate?branchName=master)](https://enkodellc.visualstudio.com/blazorboilerplate/_build/latest?definitionId=1&branchName=master)
[![Live Demo](https://img.shields.io/badge/demo-online-green.svg)](https://blazorboilerplate.com)
[![GitHub Stars](https://img.shields.io/github/stars/enkodellc/blazorboilerplate.svg)](https://github.com/enkodellc/blazorboilerplate/stargazers)
[![GitHub Issues](https://img.shields.io/github/issues/enkodellc/blazorboilerplate.svg)](https://github.com/enkodellc/blazorboilerplate/issues)
[![MIT](https://img.shields.io/github/license/SamProf/MatBlazor.svg)](LICENSE)
[![Gitter](https://badges.gitter.im/BlazorBoilerplate/community.svg)](https://gitter.im/blazorboilerplate/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## Goals
- To create a boilerplate with Blazor / Razor components that includes the most common functionality for an app that is lean yet powerful for anyone to start their own project rapidly. 
- This repository is community driven. It is not and never will be controlled by a corporation. 
- Minimal Javascript. Currently only using them for SignalR for the Forum. We may us components with JS in them but so far no Javascript has been written specifically for anything in the repository.

# Live demo
[Blazor Boilerplate](https://blazorboilerplate.com) - Kick the tires. The functionality grows every week. *Note Firewall does block some foreign IP addresses.

## Prerequisites
Don't know what Blazor is? Read [here](https://docs.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-3.0)

Complete all Blazor dependencies.

- .NET Core 3.0 Preview 9 SDK SDK 3.0.0-rc1.19457.4
- Visual Studio 2019 Preview  with the ASP.NET and web development workload selected.
- The Blazor templates on the command-line: **dotnet new -i Microsoft.AspNetCore.Blazor.Templates::3.0.0-rc1.19457.4**
- For Entity Framework Core on the command-line tools: **dotnet tool install --global dotnet-ef --version 3.0.0-rc1.19457.4**

### How to run
1. Install **dotnet-sdk 3.0.0-rc1.19457.4** and the latest **Visual Studio 2019 Preview**.
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
incomplete or closed source. I paid for 4-5 of these solutions with an Angular front-end and none of them were what I was looking for. This is my
attempt to create something that developers can start a Blazor project with several great features to build from. 
I have a lot of experience with ASP.Net webforms an new to .NET Core and Blazor. This code is not meant to be perfect or follow every Best Practice. 
It though is my ambition to learn and get feedback on what Best Practices can be implemented. 
 I have taken small solutions from other repositories and will do my best to recognize those contributions. I am very open to ideas and 
 suggestions. I am not a great developer, but I try. So please take this into consideration when using this repository.

## Completed 
 - Basic Login / User Creation
 - Admin Theme using Material Design / MatBlazor
 - Log Files using Serilog
 - Choose between SQL Lite File or MS SQL Database
 - Email Confirmation of Registered Users
 - Forgot Password Functionality
 - Api Audit Trail / Middleware to log Api Requests and Responses
 - Api Response Class to maintain consistent Api Requests and Responses
 - Todo List CRUD Example with N-Tier Layers Not just some fluff that most others do
 - Seed Database
 - Forum chat and notification system - Thanks <a href="https://github.com/ajgoldenwings" target="_blank">ajgoldenwings</a>
 - Drag and Drop Examples - <a href="https://chrissainty.com/investigating-drag-and-drop-with-blazor/" target="_blank">Chris Sainty Blazor Blog</a>

## Road map
- Switch from Entity Framework Core to Dapper. So far EF is not my cup of Tea. I think dapper will be stronger and faster
- User profile & theme options management
- User, claims, role, permission and organization unit management
- Azure Hosting Guide
- Docker Container 
- Create a Nuget Package Template

## License
This project is licensed under the terms of the [MIT license](LICENSE).

## News

### 0.3.0 - IS4 Branch (Breaking Changes - Under Development)
- Identity Server 4 - Delete your DB Thanks to [ganmuru](https://github.com/ganmuru)
- Known Issues: 
  - IAuditable Shadow Properties not getting UserId
  - User Profile broken  

### 0.2.2
- Drag and Drop Examples
- .NET Core 3.0.0-rc1.19457.4 update
- Known Issues: 
  - IAuditable Shadow Properties not getting UserId

### 0.2.1 
- Update to .NET Core 3.0 Preview 9
- User Management Screen (CRUD) & User Password Reset - Thanks [npraskins](https://github.com/npraskins) 
- Confirmation Delete Dialog Stylize & Implementation
- Known Issues: 
  - IAuditable Shadow Properties not getting UserId
  
### 0.2.0 
- CRUD Todo List example
- Restructure Project Refactor BlazorBoilerplate.Shared for N-Tier Design
- Automapper for Client (dto) / Server Models
- Removed old Migrations. Recommend to delete your database to start new.
- Implement ShadowProperties for Auditable, SoftDelete Interfaces for Models (In Progress)
- DB Seed Data
- Forum chat and notification system - Thanks <a href="https://github.com/ajgoldenwings" target="_blank">ajgoldenwings</a>
- Known Issues: 
  - IAuditable Shadow Properties not getting UserId

### 0.1.9
- Update to SDK 3.0.0-preview8-28405-07
- Added IpAddress and UserId to Middleware ApiLogging
- Review / Fix VS code Warnings and Information notice. Clean up code.
- UserProfile - Beta
- MatBlazor 1.6.0

### 0.1.8
- Middleware to log Api Requests and Responses for auditing and debugging. Thanks [salslab](https://github.com/salslab/AspNetCoreApiLoggingSample)
- Middleware for consistent API Responses and Exception Handlin. Thanks [proudmonkey](http://vmsdurano.com/asp-net-core-and-web-api-a-custom-wrapper-for-managing-exceptions-and-consistent-responses/)
- Email Pop3 / IMAP retrieval. Thanks [npraskins](https://github.com/npraskins)
- Responsive Navigation / Closed / Full / Minified / Minified & Hover effect
- Added MatNavMenu PR for MatBlazor - Ver 1.5
- Updated demo site for new MatNavMenu and rest of 0.1.8 code

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
- Fixed Known Issue - UserProfile not loading after login. Thanks [nstohler](https://github.com/nstohler)
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
