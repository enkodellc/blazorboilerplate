FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["/BlazorBoilerplate.Server/BlazorBoilerplate.Server.csproj", "BlazorBoilerplate.Server/"]
COPY ["/BlazorBoilerplate.Shared/BlazorBoilerplate.Shared.csproj", "BlazorBoilerplate.Shared/"]
COPY ["/BlazorBoilerplate.Client/BlazorBoilerplate.Client.csproj", "BlazorBoilerplate.Client/"]
RUN dotnet restore "BlazorBoilerplate.Server/BlazorBoilerplate.Server.csproj"
COPY . .
WORKDIR "/src/BlazorBoilerplate.Server"
RUN dotnet build "BlazorBoilerplate.Server.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "BlazorBoilerplate.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazorBoilerplate.Server.dll"]
