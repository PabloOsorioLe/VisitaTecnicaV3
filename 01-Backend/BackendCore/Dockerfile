# Etapa base: runtime de ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Etapa de build: SDK para compilar la app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar todo el contenido del proyecto al contenedor
COPY . .

# Publicar la app en modo Release dentro de /app/out
RUN dotnet publish -c Release -o /app/out

# Etapa final: copiar publicación y configurar punto de entrada
FROM base AS final
WORKDIR /app
COPY --from=build /app/out .

# Cambia 'BackendCore.dll' por el nombre correcto de tu proyecto DLL
ENTRYPOINT ["dotnet", "BackendCore.dll"]
