# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["API/EventosVivos.API/EventosVivos.API.csproj", "API/EventosVivos.API/"]
COPY ["MODULES/Evento/Implementation/ModuloEvento.csproj", "MODULES/Evento/Implementation/"]
COPY ["MODULES/Evento/Contract/ModuloEventoContract/ModuloEventoContract.csproj", "MODULES/Evento/Contract/ModuloEventoContract/"]
COPY ["MODULES/Reporte/Implementation/ModuloReporte/ModuloReporte.csproj", "MODULES/Reporte/Implementation/ModuloReporte/"]
COPY ["MODULES/Reporte/Contract/ModuloReporteContract/ModuloReporteContract.csproj", "MODULES/Reporte/Contract/ModuloReporteContract/"]
COPY ["MODULES/Reserva/Implementation/ModuloReserva/ModuloReserva.csproj", "MODULES/Reserva/Implementation/ModuloReserva/"]
COPY ["MODULES/Reserva/Contract/ModuloReservaContract/ModuloReservaContract.csproj", "MODULES/Reserva/Contract/ModuloReservaContract/"]
COPY ["MODULES/Security/Implementation/ModuloSecurity/ModuloSecurity.csproj", "MODULES/Security/Implementation/ModuloSecurity/"]
COPY ["MODULES/Security/Contract/ModuloSecurityContract/ModuloSecurityContract.csproj", "MODULES/Security/Contract/ModuloSecurityContract/"]
COPY ["MODULES/Tarea/ModuloTarea/ModuloTarea.csproj", "MODULES/Tarea/ModuloTarea/"]
COPY ["SHARED/Transversal/Transversal.csproj", "SHARED/Transversal/"]
RUN dotnet restore "./API/EventosVivos.API/EventosVivos.API.csproj"
COPY . .
WORKDIR "/src/API/EventosVivos.API"
RUN dotnet build "./EventosVivos.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EventosVivos.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventosVivos.API.dll"]