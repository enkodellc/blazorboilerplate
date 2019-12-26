# <img src="https://github.com/enkodellc/blazorboilerplate/blob/master/src/BlazorBoilerplate.Client/wwwroot/images/Blazor-Boilerplate-Title.png" alt="Blazor Boilerplate" style="max-wdith:100%"/>

Blazor is a web framework designed to run in the browser on a WebAssembly-based .NET runtime. Blazor Boilerplate aka Blazor Starter Template is a SPA admin template (WebAssembly / Core-Hosted) built with Blazor with a .NET Core 3.1 Server API. The UI for this application is by Material Design provided mostly by MatBlazor.
Version 0.2.3 and below utilize AspNETCore Authorization / Authentication. Version 0.3.0 and up will be using Identity Server 4.  

[![Build Status](https://enkodellc.visualstudio.com/blazorboilerplate/_apis/build/status/enkodellc.blazorboilerplate?branchName=master)](https://enkodellc.visualstudio.com/blazorboilerplate/_build/latest?definitionId=1&branchName=master)
[![Live Demo](https://img.shields.io/badge/demo-online-green.svg)](https://blazorboilerplate.com)
[![GitHub Stars](https://img.shields.io/github/stars/enkodellc/blazorboilerplate.svg)](https://github.com/enkodellc/blazorboilerplate/stargazers)
[![GitHub Issues](https://img.shields.io/github/issues/enkodellc/blazorboilerplate.svg)](https://github.com/enkodellc/blazorboilerplate/issues)
[![MIT](https://img.shields.io/github/license/SamProf/MatBlazor.svg)](LICENSE)
[![Gitter](https://badges.gitter.im/BlazorBoilerplate/community.svg)](https://gitter.im/blazorboilerplate/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## Goals
- To create a boilerplate with Blazor / Razor components that includes the most common functionality for an app that is lean yet powerful for anyone to start a real world application quickly.
- Avoid many external compnents & libraries which make it difficult to maintain, update, track down code, learn code and issues.
- This repository is community driven. It is not and never will be controlled by a corporation.
- Minimal Javascript. Currently only using them for SignalR for the Forum and MatBlazor / Material Desing. We may use components with JS in them but so far no Javascript has been written specifically for anything in the repository.

# Live demo
[Blazor Boilerplate](https://blazorboilerplate.com) - Kick the tires.  *Note Firewall does block some foreign IP addresses. Swagger UI to view the server API [https://blazorboilerplate.com/swagger/index.html](https://blazorboilerplate.com/swagger/index.html).

## Prerequisites
Don't know what Blazor is? Read [here](https://docs.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-3.0)

Complete all Blazor dependencies.

- [.Net Core SDK 3.1.0-preview4.19579.2](https://dotnet.microsoft.com/download/dotnet-core/3.1)
- Install the Latest Visual Studio 2019 Preview with the ASP.NET and web development workload selected.
- Entity Framework Core on the command-line tools: **dotnet tool install --global dotnet-ef --version 3.1.0-preview4.19579.2**

### How to run
1. Install the latest .NET Core SDK **https://dotnet.microsoft.com/download/dotnet-core/3.1** and the latest **Visual Studio 2019 Preview**.
2. Clone or download.
3. Open the solution in Visual Studio and press F5.
4. To view the API using Swagger UI, Run the solution and go to: [http://localhost:53414/swagger/index.html](http://localhost:53414/swagger/index.html). Live example:
[https://blazorboilerplate.com/swagger/index.html](https://blazorboilerplate.com/swagger/index.html)

## Publish on IIS - What works for me on my Windows Server 2016 & SQL Server 2014 (Enkodellc)
1. Publish both the Client and Server projects to local folder
2. Upload / Copy published Server directory to website folder.
3. Upload / Copy published Client directory ON TOP of to the same root website directory of Server, it will add some files and overwrite some.
4. Install your SSL, use self-signed if you don't have one. Make sure your SSL is in the WebHosting Certificate Store.
5. Configure your appsettings.production.config - Connection String, Thumbprint / SSL. 
6. Login with either the user **[user | user123]** or admin **[admin | admin123]** default accounts.

### Thanks To
- [Blazor](https://blazor.net)
- [BlazorWithIdentity](https://github.com/stavroskasidis/BlazorWithIdentity)
- [MatBlazor](https://github.com/SamProf/MatBlazor)

## Contributing

Please star, watch and fork! We'd greatly appreciate any contribution you make. I am very open to updates and features, though most feature requests 
will be depending on how much community support exists.

## Disclaimer / About the Author

I (Enkodellc) started this repository as I was frustrated with the examples out there that people were charging money for and were in my opinion
incomplete or closed source. I paid for 4-5 of these solutions with an Angular front-end and none of them were what I was looking for. This is my
attempt to create something that developers can start a Blazor project with several great features to build from. 
I have a lot of experience with ASP.Net webforms an new to .NET Core and Blazor. This code is not meant to be perfect or follow every Best Practice. 
It though is my ambition to learn and get feedback on what Best Practices can be implemented. I will be migrating a Webforms app to Blazor so this is my 
opportunity to learn, share, grow and get feedback on what hopefully will be a great Blazor Starter Kit.
  
I have taken small solutions from other repositories and will do my best to recognize those contributions. I am very open to ideas and 
 suggestions. I am not a great developer, but I try. So please take this into consideration when using this repository. If you wish to hire me for 
 consulting or as a contractor please reach out via [email](support@blazorboilerplate.com) or [https://gitter.im/enkodellc](https://gitter.im/enkodellc).

## Completed 
 - Basic Login / User Creation
 - Admin Theme using Material Design / MatBlazor
 - Swagger UI API visualizer 
 - Log Files using Serilog
 - Choose between SQL Lite File or MS SQL Database
 - Email Confirmation of Registered Users
 - Forgot Password Functionality
 - ISoftDelete Interface for Models - Allows for "trash / restore" of data **IsDeleted** property
 - IAuditable Interface for Models - Allows for **CreatedOn, CreatedBy, ModifiedOn, ModifiedBy** properties  
 - Api Audit Trail / Middleware to log Api Requests and Responses
 - Api Response Class to maintain consistent Api Requests and Responses
 - Todo List CRUD Example with N-Tier Layers Not just some fluff that most others do
 - Seed Database & Database Migrations
 - Forum chat and notification system - Thanks <a href="https://github.com/ajgoldenwings" target="_blank">ajgoldenwings</a>
 - Drag and Drop Examples - <a href="https://chrissainty.com/investigating-drag-and-drop-with-blazor/" target="_blank">Chris Sainty Blazor Blog</a>
 - Docker Container Support

## Road map
- User profile & settings management
- User, claims, role, permission and organization unit management
- Azure Hosting Guide
- Create a Nuget Package Template
- Switch from Entity Framework Core to Dapper. So far EF is not my cup of Tea. I think dapper will be stronger and faster just my opinion. I like SQL code, less automagic code with more control. Just my style.

## License
This project is licensed under the terms of the [MIT license](LICENSE).

### Problem Solving Tips
- Make sure you have all pre-requisites installed.
- Keep It Simple Stupid: If you are running into issues with SQL / connection string. First CHECK both appsettings.json (appsettings.production.json for production) and (appsettings.development.json for development). 
- Test out with SQLlite / file db. Then test out with a known good connection string.
- Go back to the Origin: BlazorBoilerplate was built off of [BlazorWithIdentity](https://github.com/stavroskasidis/BlazorWithIdentity) so first step is to run this and try and publish. The reasoning is that this is a very lean project to reduce the amount of code and resources requiring debugging.
- If still failing get on [Gitter BlazorBoilerplate](https://gitter.im/blazorboilerplate/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)
 for Blazor Boilerplate or  [Gitter aspnet/Blazor](https://gitter.im/aspnet/Blazor?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge).
- Debugging is very limited on WebAssembly / Client-side Blazor, which this project is. The server side of the project can easily be debugged, just not there yet on the client-side code.
 Take a look at the "Dual" branch for SSB / CSB options. It may be a bit outdated.

### Docker Support
- Prerequisite: Install [Docker Desktop](https://go.microsoft.com/fwlink/?linkid=847268) 
- Include / Reload **docker-compose** project in solution.
- [Do Docker stuff](https://docs.docker.com/v17.09/docker-for-windows/install/) - I don't have much experience with Docker.

### Azure Support
-  Looking for additional help with Azure documentation and steps.

## News
### 0.6.0 (Breaking Changes - Under Development - DualModeV2 Branch)
- Dual Mode CSB & SSB [View Wiki](https://github.com/enkodellc/blazorboilerplate/wiki/Dual-Mode-CSB---SSB-Tips). Thanks [MarkStega](https://github.com/MarkStega

### 0.5.0 (Under Development)
- MatBlazor 2.0 Breaking Changes
- .NET Core 3.1.0 / v3.1.0-preview4 Blazor Nuget package updates
- Admin Roles / Permissions Management. Thanks [vd3d](https://github.com/vd3d)
- Fix Login EditForm / Double submit of Login. Thanks [MarkStega](https://github.com/MarkStega)
- UserProfile Fixes. Thanks [mobinseven](https://github.com/mobinseven)
- Chrome Cookie updates. Thanks [oneparameter](oneparameter)
    
### 0.4.0
 - Docker Support. Thanks [npraskins](https://github.com/npraskins) & [acid12](https://github.com/acid12)
 - Fixed IAuditable / ShadowProperties for CreatedOn, CreatedBy, ModifiedOn, ModifiedBy.  Thanks [acid12](https://github.com/acid12)
 - Known Issues:
    - Drag and Drop example does not work in FF. Known FF issue.

### 0.3.2 
- Identity Server 4 - Authentication with ASP.Net Identity Authorization with Policies  
- v3.1.0-preview3 / Nuget Package updates
- Authorize / Policy Examples on Users Page
- Known Issues: 
  - IAuditable Shadow Properties not getting UserId
  - Drag and Drop example does not work in FF. Known FF issue.

### 0.3.0 - IS4 (Breaking Changes from 0.2.3)
- Identity Server 4 First Release - Delete your DB! Thanks to [ganmuru](https://github.com/ganmuru)
- User Profile Store Last Page Visited / Return on Login
- .NET Core 3.0.100 / Blazor 3.0.0-preview9.19457.4 update
- Known Issues: 
  - IAuditable Shadow Properties not getting UserId

### 0.2.3 - .Net Core Authentication / Authorization (Stable Version)
- .NET Core 3.0.100 / Blazor 3.0.0-preview9.19457.4 update
- Known Issues: 
  - IAuditable Shadow Properties not getting UserId

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
- Middleware for consistent API Responses and Exception Handling. Thanks [proudmonkey](http://vmsdurano.com/asp-net-core-and-web-api-a-custom-wrapper-for-managing-exceptions-and-consistent-responses/)
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
