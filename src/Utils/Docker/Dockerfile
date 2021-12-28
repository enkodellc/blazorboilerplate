FROM mcr.microsoft.com/dotnet/aspnet:5.0.13-buster-slim AS base
WORKDIR /
EXPOSE 443
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0.402-buster-slim AS build
WORKDIR /
COPY . .
RUN dotnet restore "src/Server/BlazorBoilerplate.Server/BlazorBoilerplate.Server.csproj"
WORKDIR "/src/Server/BlazorBoilerplate.Server"
RUN dotnet build "BlazorBoilerplate.Server.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "BlazorBoilerplate.Server.csproj" -c Release -o /app/publish
RUN dotnet dev-certs https --clean
RUN dotnet dev-certs https -ep /app/publish/aspnetapp.pfx -p Admin123
#if .pfx was provided from certificate authority uncomment the below
#COPY src/Server/BlazorBoilerplate.Server/AuthSample.pfx /app/publish/aspnetapp.pfx

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /app/publish/aspnetapp.pfx ./AuthSample.pfx
ENTRYPOINT ["dotnet", "BlazorBoilerplate.Server.dll"]
