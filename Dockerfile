# Imagen base oficial de .NET 8 Runtime para producción
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV PORT=8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV ASPNETCORE_hostBuilder__reloadConfigOnChange=false

# Imagen del SDK para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AURA.csproj", "./"]
RUN dotnet restore "AURA.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "AURA.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AURA.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AURA.dll"]
