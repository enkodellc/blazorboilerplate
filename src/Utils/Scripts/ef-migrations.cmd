rem dotnet tool install -g dotnet-ef
rem dotnet tool update -g dotnet-ef

rem Microsoft.EntityFrameworkCore.Design package must be preset in the following project
cd BlazorBoilerplate.Storage

dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add 4Preview3 -c PersistedGrantDbContext --verbose --no-build --configuration Debug_SSB
dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add 4Preview3 -c ConfigurationDbContext --verbose --no-build --configuration Debug_SSB

rem The following command revert db to previous migration in this case 20200326012204_DbLogging, just to test new migration on existing populated tables
rem dotnet ef --startup-project ../BlazorBoilerplate.Server/ database update 20200326012204_DbLogging -c ApplicationDbContext --verbose --no-build --configuration Debug_SSB