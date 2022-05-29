Get-ChildItem ..\..\ -include bin,obj -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse }
dotnet new --uninstall ..\..\..\
dotnet new -i ..\..\..\
pause