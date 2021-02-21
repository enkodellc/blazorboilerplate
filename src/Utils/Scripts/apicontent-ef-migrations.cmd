rem dotnet tool install -g dotnet-ef
dotnet tool update -g dotnet-ef

rem Microsoft.EntityFrameworkCore.Design package must be preset in the following project
cd ..\..\Server\BlazorBoilerplate.Storage

dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add CreateApiContentDb -c ApiContentDbContext --verbose --configuration Debug -o "Migrations/ApiContentDb"
dotnet ef --startup-project ../BlazorBoilerplate.Server/ database update CreateApiContentDb -c ApiContentDbContext

pause
rem to reset to original: 
REM dotnet ef --startup-project ../BlazorBoilerplate.Server/ database update 0 -c ApiContentDbContext
rem dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations remove -c ApiContentDbContext
REM to apply the update
REM dotnet ef --startup-project ../BlazorBoilerplate.Server/ database update -c ApiContentDbContext
rem to undo after updating vnext
REM dotnet ef --startup-project ../BlazorBoilerplate.Server/ database update CreateApiContentDb -c ApiContentDbContext
