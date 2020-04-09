rem dotnet tool install -g dotnet-ef
rem dotnet tool update -g dotnet-ef

rem Microsoft.EntityFrameworkCore.Design package must be preset in the following project
cd BlazorBoilerplate.Storage

dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add 4Preview3 -c PersistedGrantDbContext
dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add 4Preview3 -c ConfigurationDbContext

