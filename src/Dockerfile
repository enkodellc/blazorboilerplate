FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.3-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.201-buster AS build
WORKDIR /src
COPY ["BlazorBoilerplate.Server/BlazorBoilerplate.Server.csproj", "BlazorBoilerplate.Server/"]
COPY ["BlazorBoilerplate.Shared/BlazorBoilerplate.Shared.csproj", "BlazorBoilerplate.Shared/"]
COPY ["BlazorBoilerplate.Client/BlazorBoilerplate.Client.csproj", "BlazorBoilerplate.Client/"]
COPY ["BlazorBoilerplate.CommonUI/BlazorBoilerplate.CommonUI.csproj", "BlazorBoilerplate.CommonUI/"]
COPY ["BlazorBoilerplate.Storage/BlazorBoilerplate.Storage.csproj", "BlazorBoilerplate.Storage/"]
RUN dotnet restore "BlazorBoilerplate.Server/BlazorBoilerplate.Server.csproj"
COPY . .
WORKDIR "/src/BlazorBoilerplate.Server"
ARG BUILD_CONFIG="Release_CSB"
RUN dotnet build "BlazorBoilerplate.Server.csproj" -c $BUILD_CONFIG -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIG="Release_CSB"
RUN dotnet publish "BlazorBoilerplate.Server.csproj" -c $BUILD_CONFIG -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazorBoilerplate.Server.dll"]