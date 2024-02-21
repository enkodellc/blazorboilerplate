rem dotnet tool install -g dotnet-ef
rem dotnet tool update -g dotnet-ef

rem Microsoft.EntityFrameworkCore.Design package must be preset in the following project
cd .\Server\BlazorBoilerplate.Storage

dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add CreateLocalizationDb -c LocalizationDbContext --verbose --no-build --configuration Debug -o "Migrations/LocalizationDb"
dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add CreateTenantStoreDb -c TenantStoreDbContext --verbose --no-build --configuration Debug -o "Migrations/TenantStoreDb"
dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add CreateApplicationDb -c ApplicationDbContext --verbose --no-build --configuration Debug -o "Migrations/ApplicationDb"
pause
rem The following command revert db to previous migration in this case 20200326012204_DbLogging, just to test new migration on existing populated tables
rem dotnet ef --startup-project ../BlazorBoilerplate.Server/ database update 20200326012204_DbLogging -c ApplicationDbContext --verbose --no-build --configuration Debug_SSB



dotnet ef database --startup-project ../BlazorBoilerplate.Server/ migrations drop CreateLocalizationDb -c LocalizationDbContext --verbose --no-build --configuration Debug -o "Migrations/LocalizationDb"
dotnet ef database --startup-project ../BlazorBoilerplate.Server/ migrations drop CreateTenantStoreDb -c TenantStoreDbContext --verbose --no-build --configuration Debug -o "Migrations/TenantStoreDb"
dotnet ef database --startup-project ../BlazorBoilerplate.Server/ migrations drop CreateApplicationDb -c ApplicationDbContext --verbose --no-build --configuration Debug -o "Migrations/ApplicationDb"