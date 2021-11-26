rem dotnet tool install -g dotnet-ef
rem dotnet tool update -g dotnet-ef

rem Microsoft.EntityFrameworkCore.Design package must be preset in the following project
cd ..\..\Server\BlazorBoilerplate.Storage

dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add CreateLocalizationDb -c LocalizationDbContext --verbose --no-build --configuration Debug -o "Migrations/LocalizationDb"
dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add CreateTenantStoreDb -c TenantStoreDbContext --verbose --no-build --configuration Debug -o "Migrations/TenantStoreDb"
dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add CreateApplicationDb -c ApplicationDbContext --verbose --no-build --configuration Debug -o "Migrations/ApplicationDb"
dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add CreatePersistedGrantDb -c PersistedGrantDbContext --verbose --no-build --configuration Debug -o "Migrations/PersistedGrantDb"
dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add CreateConfigurationDb -c ConfigurationDbContext --verbose --no-build --configuration Debug -o "Migrations/ConfigurationDb"
pause
rem The following command revert db to previous migration in this case 20200326012204_DbLogging, just to test new migration on existing populated tables
rem dotnet ef --startup-project ../BlazorBoilerplate.Server/ database update 20200326012204_DbLogging -c ApplicationDbContext --verbose --no-build --configuration Debug_SSB