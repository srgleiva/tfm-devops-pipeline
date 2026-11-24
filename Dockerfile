# Imagen única: la API de .NET sirve también el frontend compilado (carpeta wwwroot).

# 1) Compilación del frontend (React + Vite)
FROM node:24-alpine AS frontend
WORKDIR /src/frontend
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

# 2) Compilación y publicación de la API (.NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY api/Api.csproj api/
RUN dotnet restore api/Api.csproj
COPY api/ api/
RUN dotnet publish api/Api.csproj -c Release -o /app --no-restore

# 3) Imagen final, solo con el entorno de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS final
WORKDIR /app
COPY --from=build /app ./
COPY --from=frontend /src/frontend/dist ./wwwroot
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Api.dll"]
