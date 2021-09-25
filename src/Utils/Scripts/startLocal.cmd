<<<<<<< Updated upstream
cd ..\..\Server\BlazorBoilerplate.Server
dotnet run --launch-profile Kestrel
=======
cd ..\..\Server\SSDCPortal.Server\bin\debug\net5.0
start SSDCPortal.Server.exe --environment "Development"
pause
taskkill /IM SSDCPortal.Server.exe
cd ..\..\..\..\..\Utils\Scripts

#dotnet run --project ../../../../SSDCPortal.Serve
>>>>>>> Stashed changes
