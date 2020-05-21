# <img src="https://github.com/enkodellc/blazorboilerplate/blob/master/src/BlazorBoilerplate.CommonUI/wwwroot/images/Blazor-Boilerplate-Title.png" alt="Blazor Boilerplate" style="max-wdith:100%"/>

Blazor is a web framework designed to run in the browser on a WebAssembly-based .NET runtime. Blazor Boilerplate aka Blazor Starter Template is a SPA admin template that is able to BOTH (WebAssembly / Core-Hosted) and Server-Side Blazor with a .NET Core 3.1 Server. The UI for this application is by Material Design provided mostly by MatBlazor.
Version 0.2.3 and below utilize AspNETCore Authorization / Authentication. Version 0.3.0 and up will be using Identity Server 4. Version 0.6.0 and up are capable of both CSB and SSB!    

## Repository Notes
- Read the news below to stay up to date on the repo. We will try to keep the latest major changes on a different branch and have the more stable / tested version on the master branch.
- Development Branch is the latest dev code for future release.
- Fluxor - Branched from version 0.6.0 with an example with [Blazor Fluxor](https://github.com/enkodellc/blazorboilerplate/tree/blazorfluxor) *Note: not really being maintained at this time

[![Build Status](https://enkodellc.visualstudio.com/blazorboilerplate/_apis/build/status/enkodellc.blazorboilerplate?branchName=master)](https://enkodellc.visualstudio.com/blazorboilerplate/_build/latest?definitionId=1&branchName=master)
[![Live Demo](https://img.shields.io/badge/demo-online-green.svg)](https://blazorboilerplate.com)
[![GitHub Stars](https://img.shields.io/github/stars/enkodellc/blazorboilerplate.svg)](https://github.com/enkodellc/blazorboilerplate/stargazers)
[![GitHub Issues](https://img.shields.io/github/issues/enkodellc/blazorboilerplate.svg)](https://github.com/enkodellc/blazorboilerplate/issues)
[![MIT](https://img.shields.io/github/license/SamProf/MatBlazor.svg)](LICENSE)
[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.me/enkodellc)
[![Gitter](https://badges.gitter.im/BlazorBoilerplate/community.svg)](https://gitter.im/blazorboilerplate/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## Goals
- This repository is community driven. It is not and never will be controlled by a corporation. It's success is dependent on people using it, reviewing it, offering suggestions and most importantly contributing. Please join the [gitter discussion](https://gitter.im/blazorboilerplate/community) 
- To create a boilerplate with Blazor / Razor components that includes the most common functionality to start a real world application quickly.
- Avoid many external components & libraries which can make it difficult to maintain, update, track down code, learn code and issues.
- Minimal Javascript. Currently only using them for SignalR for the Forum and MatBlazor / Material Design. We may use components with JS in them but so far no Javascript has been written specifically for anything in the repository.


# Live demo
- [Blazor Boilerplate - CSB/WASM](https://blazorboilerplate.com) - Kick the tires.  *Note Firewall does block some foreign IP addresses. Swagger UI to view the server API [https://blazorboilerplate.com/swagger/index.html](https://blazorboilerplate.com/swagger/index.html).
- [Demo for Development Branch - SSB](https://blazor-server.quarella.net/)
- [Demo for Development Branch - CSB](https://blazor-wasm.quarella.net/)

## Prerequisites
Don't know what Blazor is? Read [here](https://docs.microsoft.com/en-us/aspnet/core/blazor)

Complete all Blazor dependencies.

- The latest [.Net Core SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1)
- Install the Latest Visual Studio 2019 with the ASP.NET and web development workload selected.
- Entity Framework Core on the command-line tools: **dotnet tool install --global dotnet-ef**

### How to run
1. Install the latest .NET Core SDK **https://dotnet.microsoft.com/download/dotnet-core/3.1** and the latest **Visual Studio 2019 (v16.6)**
2. Clone or download.
3. Open the solution in Visual Studio and press F5.
4. To view the API using Swagger UI, Run the solution and go to: [http://localhost:53414/swagger/index.html](http://localhost:53414/swagger/index.html). Live example:
[https://blazorboilerplate.com/swagger/index.html](https://blazorboilerplate.com/swagger/index.html)

## Publish on IIS - What works for me on my Windows Server 2016 & SQL Server 2014 (Enkodellc)
1. Publish BlazorBoilerplate.Server project to your IIS website folder using CSB or SSB.
2. Install your SSL. Make sure your SSL is in the **WebHosting** Certificate Store.
    - A free certificate from [Let's Encrypt](https://letsencrypt.org/) will work. 
    - For steps 2 & 3 the utility [win-acme](https://github.com/win-acme/win-acme) installs the
certificate on your server, performs renewal and configure your IIS Website Bindings to have https binding with the SSL certificate set and Port 443 for default.

4. Configure your IIS Website Bindings to have https binding with the SSL certificate set and Port 443 for default.
3. Configure / create appsettings.production.config. Set  Connection String, Thumbprint / SSL. Thumbprint example:  **143fbd7bc36e78b1bcf9a53c13336eaebe33353a**
4. Login with either the user **[user | user123]** or admin **[admin | admin123]** default accounts.

### Thanks To
- [Blazor](https://blazor.net)
- [BlazorWithIdentity](https://github.com/stavroskasidis/BlazorWithIdentity)
- [MatBlazor](https://github.com/SamProf/MatBlazor)

## Contributing

Please star, watch and fork! We'd greatly appreciate any contribution you make. I am very open to updates and features, though most feature requests will be depending on how much community support exists. 

## Disclaimer / About the Author

I (Enkodellc) started this repository as I was frustrated with the examples out there that people were charging money for and were in my opinion
incomplete or closed source. I paid for 4-5 of these solutions with an Angular front-end / .Net Core back-end and none of them were what I was looking for. This is my
attempt to create something that developers can start a Blazor project with several great features to build from. 
I have a lot of experience with ASP.Net webforms an new to .NET Core and Blazor. This code is not meant to be perfect or follow every best practice. 
It though is my ambition to learn and get feedback on what best practices can be implemented. I will be migrating a Webforms app to Blazor so this is my 
opportunity to learn, share, grow, and get feedback on what hopefully will be a great Blazor Starter Kit.
  
I have taken small solutions from other repositories and will do my best to recognize those contributions. I am very open to ideas and 
 suggestions. I am not a great developer, but I try. So please take this into consideration when using this repository. If you wish to hire me for 
 consulting or as a contractor please reach out via [email](support@blazorboilerplate.com) or [https://gitter.im/enkodellc](https://gitter.im/enkodellc). I have taken well  over 1,000 hours  to create, maintain, and answer questions. Please [donate](https://www.paypal.me/enkodellc) to support my efforts.

## Completed 
 - Basic Login / User Creation
 - Admin Theme using Material Design / MatBlazor
 - Swagger UI API visualizer - [View the live API](https://blazorboilerplate.com/swagger/index.html)
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
 - Dual Mode (CSB / SSB) - Client Side / Webassembly & Sever Side. Thanks [MarkStega](https://github.com/MarkStega)
 - Error Log to Database with Serilog & SQL. Thanks [np-at](https://github.com/np-at)
 
## Road map / Possible future features in no particular order
- Multi-Tenant - This is most likely the next major revision 0.9.0 - *restructure* branch. Looks like it might be in place by end of April.
- Caching / speed improvement? Review possible optimizations for Blazor to make sure BB is running as efficient as can be.
- Create a Nuget Packages Template. There has been several request for BB to be broken into packages. I unfortunately do not have the resources to this at this time. There is discussions with some of the contributors to do a professional version.
- Possibly switch from Entity Framework Core to Dapper. So far EF is not my cup of Tea. I think dapper will be stronger and faster just my opinion. I like SQL code, less automagic code with more control. Just my style.
- Localization - Looking for any contributors who have some expertise in this. Previw 4 is tentative for .resx support.
- Branch for [PWA  (Progressive Web App)](https://devblogs.microsoft.com/aspnet/blazor-webassembly-3-2-0-preview-2-release-now-available/) for testing & possible implementation 

## License
This project is licensed under the terms of the [MIT license](LICENSE).

### Problem Solving Tips
- If you are having issues wih authentication or any other strange behavior try using Incognito mode / different browser. 
- Make sure you have all pre-requisites installed.
- Keep It Simple Stupid: If you are running into issues with SQL / connection string. First CHECK both appsettings.json (appsettings.production.json for production) and (appsettings.development.json for development). 
- Test out with SQLlite / file db. Then test out with a known good connection string.
- Go back to the Origin: BlazorBoilerplate was built off of [BlazorWithIdentity](https://github.com/stavroskasidis/BlazorWithIdentity) so first step is to run this and try and publish. The reasoning is that this is a very lean project to reduce the amount of code and resources requiring debugging.
- If still failing get on [Gitter BlazorBoilerplate](https://gitter.im/blazorboilerplate/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)
 for Blazor Boilerplate or  [Gitter aspnet/Blazor](https://gitter.im/aspnet/Blazor?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge).
- Debugging is very limited on WebAssembly / Client-side Blazor. Use Debug_SSB for debugging the UI. Just be aware of browser caching issues when switching modes.
 The server side of the project can easily be debugged, just not there yet on the client-side code.
- If you are getting compiler errors try and close VS delete your .vs directory in the solution folder. If that doesn't work delete the solution and redownload the repo.
 
### Postgres Support
*Note this might be out of date.. Delete Existing Migrations in the BlazorBoilerplate.Server/Migrations Folder and then create your own migrations:  
  -`dotnet ef --startup-project ..\BlazerBoilerplate.Server migrations add InitialApplicationDbMigration --context ApplicationDbContext -o Migrations\ApplicationDb`  
  -`dotnet ef --startup-project ..\BlazorBoilerplate.Server\ migrations add InitialConfigurationDbMigration --context ConfigurationDbContext  -o Migrations\ConfigurationDb`  
  -`dotnet ef --startup-project ..\BlazorBoilerplate.Server\ migrations add PersistedGrantDbContext --context PersistedGrantDbContext -o Migrations\PersistedGrantDb`  

### Docker Support
- Prerequisite: Install [Docker Desktop](https://go.microsoft.com/fwlink/?linkid=847268) 
- Include / Reload **docker-compose** project in solution.
- [Do Docker stuff](https://docs.docker.com/v17.09/docker-for-windows/install/) - I don't have much experience with Docker.

### Azure Support
- [Azure Hosting Wiki](https://github.com/enkodellc/blazorboilerplate/wiki/Hosting-Blazor-boilerplate-on-Microsoft-Azure) 
- *Note that Azure isn't as up to date with their SDK as Blazor Boilerplate so you might have to use an older version

## News

### 0.9.0 Under Development (Development branch Major Project Refactor - Thanks GioviQ)
- Localization Support. Thanks [GioviQ](https://github.com/GioviQ) 
- DotNet Template Solution. Thanks [GioviQ](https://github.com/GioviQ) 
- Update to Identity Server. Thanks [GioviQ](https://github.com/GioviQ) 
- [Demo for Development Branch - SSB](https://blazor-server.quarella.net/)
- [Demo for Development Branch - CSB](https://blazor-wasm.quarella.net/)

### 1.0.0 - Master branch
- Nuget Package updates Blazor 3.2 - Production!

### 0.8.2 - Master branch
- Nuget Package updates 

### 0.8.1 Stable - Master branch
- Blazor WebAssembly 3.2.0 Preview 5
- Nuget Package updates 

### 0.8.0 (Major Project Refactor - Thanks DanielBunting)
- Refactor Project Architecture. Thanks [DanielBunting](https://github.com/DanielBunting) 
- Add Initial Tests. Thanks [DanielBunting](https://github.com/DanielBunting) 
- Project Code Review. Thanks [GioviQ](https://github.com/GioviQ) 
- Revised AuthorizationPolicyProvider. Thanks [mobinseven](https://github.com/mobinseven) 
- Server-side Multi-Tenant V.1 (Not working / No UI) - Thanks [mobinseven](https://github.com/mobinseven) 
- SQL Server Error Logging with Serilog Thanks [np-at](https://github.com/np-at)
- Added Ultramapper for Dto restore / clone. Thanks [GioviQ](https://github.com/GioviQ) 

### 0.7.0 (Breaking Changes)
- .NET Core 3.2 Preview 1- Microsoft & Other Nuget package updates - Program.cs refactored for CSB
- MatBlazor 2.1.2
- Server-Side Blazor Auth Cookie Issue [#138](https://github.com/enkodellc/blazorboilerplate/issues/138) -  Thanks [marcotana](https://github.com/marcotana)
- Exit from Login dialog [#139](https://github.com/enkodellc/blazorboilerplate/issues/139) -  Thanks [Oleg26Dev](https://github.com/Oleg26Dev)
- Focus on Login Form Entry - Thanks [responsive-edge](https://github.com/responsive-edge)
- Azure Hosting Wiki and Project update - Thanks [soomon](https://github.com/soomon)
- Known Issue with new project structure - Breaks CSB Debugging (shift + alt + d) - Expect fix in .Net Core 3.2 preview 2. Use SSB for debugging.

### 0.6.1 (No Major Breaking Changes)
- .NET Core 3.1.1 - Microsoft Nuget package updates - security patches no code changes 
- Known Issue with new project structure - Breaks CSB Debugging (shift + alt + d) - Expect fix in .Net Core 3.2 preview 2. Use SSB for debugging.

### 0.6.0 (Major Breaking Changes)
- Dual Mode CSB & SSB [View Wiki](https://github.com/enkodellc/blazorboilerplate/wiki/Dual-Mode-CSB---SSB-Tips). Thanks [MarkStega](https://github.com/MarkStega)
    - There are changes to solution structure for Dual Mode. Switching modes can be tricky with the browser cache so if you see something strange use incognito mode or a different browser. 
    Best solution is to pick your version and stick with it, then expect to clear cache when switching. Read the Wiki!
    - Known Issue with new project structure - Breaks CSB (shift + alt + d) Debugging - Expect fix in .Net Core 3.2 preview 2 . Use SSB for debugging.

### 0.5.0
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
