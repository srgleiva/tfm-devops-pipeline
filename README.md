# TFM · Aplicación de caso de estudio

Aplicación web sencilla (lista de tareas) que sirve como **caso de estudio** del Trabajo Fin de Máster *Pipeline de CI/CD con pruebas automatizadas y dashboard de métricas de entrega* (Máster Universitario en Desarrollo y Operaciones, UNIR).

La aplicación no es el objeto del TFM: su función es dar al pipeline algo real que compilar, probar, empaquetar y desplegar.

## Origen

El código de partida procede del ejemplo oficial de Microsoft **ASP.NET Core Minimal API + PostgreSQL + Vite** del repositorio [microsoft/aspire-samples](https://github.com/microsoft/aspire-samples/tree/main/samples/vite-csharp-postgres) (commit `4b20948`, 11 de noviembre de 2026), publicado con licencia MIT por la .NET Foundation. Se conserva su licencia en el archivo `LICENSE`.

Cambios realizados sobre el original:

- Se ha eliminado la dependencia de .NET Aspire (`apphost.mts`, configuración de Aspire y valores por defecto de servicio con OpenTelemetry). La aplicación se ejecuta ahora con Docker Compose o directamente con `dotnet run` y `npm run dev`.
- La conexión a PostgreSQL se configura con la cadena de conexión estándar `ConnectionStrings:db`.
- Se han añadido comprobaciones de salud en `/health/live` y `/health/ready`, esta última con verificación de la base de datos.
- Se han añadido `Dockerfile` (construcción en varias etapas, imagen única que sirve API y frontend) y `docker-compose.yml`.
- Se expone `Program` para las pruebas de integración con `WebApplicationFactory`.

Todo lo relativo al pipeline (pruebas, workflows de GitHub Actions, infraestructura y dashboard) es desarrollo propio del TFM.

## Tecnologías

- **Backend:** .NET 10, ASP.NET Core Minimal API, Entity Framework Core, PostgreSQL.
- **Frontend:** React 19, TypeScript, Vite, Tailwind CSS.

## Estructura

```
api/         API de .NET (endpoints en api/Extensions/TodoEndpoints.cs)
frontend/    Interfaz en React (pruebas de Vitest en src/, pruebas E2E en e2e/)
tests/       Pruebas unitarias e de integración de la API (xUnit)
.github/     Workflows de GitHub Actions (CI, CodeQL) y Dependabot
Dockerfile   Imagen de producción (API + frontend compilado)
docker-compose.yml
```

## Cómo ejecutarla

### Opción 1: todo con Docker

Requiere Docker Desktop.

```
docker compose up --build
```

La aplicación queda en http://localhost:8080.

### Opción 2: desarrollo local

Requiere Docker Desktop, el SDK de .NET 10 y Node.js 22 o superior.

```
docker compose up -d db          # base de datos PostgreSQL

cd api
dotnet run                        # API en http://localhost:5234 (documentación en /scalar)

cd ../frontend
npm install
npm run dev                       # interfaz en http://localhost:5173
```

## Pruebas

| Tipo | Herramienta | Comando |
|---|---|---|
| Unitarias (API) | xUnit | `dotnet test tests/Api.UnitTests` |
| Integración (API + PostgreSQL) | xUnit, WebApplicationFactory, Testcontainers | `dotnet test tests/Api.IntegrationTests` (requiere Docker) |
| Unitarias y de componentes (frontend) | Vitest, Testing Library | `cd frontend && npm test` |
| Extremo a extremo | Playwright | `docker compose up -d` y `cd frontend && npm run test:e2e` |

## API

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/todos?offset=0&limit=50` | Lista de tareas paginada |
| GET | `/api/todos/{id}` | Una tarea |
| POST | `/api/todos` | Crea una tarea (`{ "title": "..." }`) |
| PUT | `/api/todos/{id}` | Modifica título o estado (`{ "title": "...", "completed": true }`) |
| DELETE | `/api/todos/{id}` | Elimina una tarea |
| GET | `/health/live`, `/health/ready` | Comprobaciones de salud |
