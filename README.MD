# BlazorChat: Developer Documentation

This repository contains a real-time chat application built with Blazor Web App (Auto/Server), Entity Framework Core, and SignalR.

## Prerequisites

Before managing the database, you must have the Entity Framework Core Tools installed on your machine. Run this command in your terminal:

```bash
dotnet tool install --global dotnet-ef
```

## Database Workflow (EF Core)

We use Migrations to keep the database schema in sync across different development environments.

### 1. Initial Setup

If you have just cloned the repository and need to create your local database file (blazorchat.db):

```bash
# Navigate to the Server project
cd BlazorChat.Server

# Apply all migrations to create your local database
dotnet ef database update
```

### 2. Changing the Schema

Whenever you modify an Entity (e.g., adding a new property to User.cs or creating a Message.cs), follow these steps:

```bash
# 1. Generate the migration files (run from Solution root)
dotnet ef migrations add <DescriptionOfChange> --project BlazorChat.Server

# 2. Apply the change to your local database file
dotnet ef database update --project BlazorChat.Server
```

### 3. Reverting Changes

If you created a migration but haven't pushed it to Git yet and want to delete the files and revert the database snapshot:

```bash
dotnet ef migrations remove --project BlazorChat.Server
```

## Running client and server separately

To run the backend and frontend as independent processes (recommended for separate deploys):

- Start the server (API + SignalR hub):

```bash
dotnet run --project BlazorChat/BlazorChat.Server
```

- Start the client (Blazor WebAssembly dev host):

```bash
dotnet run --project BlazorChat/BlazorChat.Client
```

Configuration notes:

- The client reads `ApiBaseUrl` from `wwwroot/appsettings.json` (defaults to `https://localhost:7138/`). Update it to point at your running server host.
- The server exposes the SignalR hub at `/hubs/chat` and has a CORS policy named `AllowClient` that permits origins configured with the `ClientOrigins` setting. Set `ClientOrigins` in the server configuration to the client origin(s) (semicolon-separated) when hosting separately.
- If you use cookie-based auth across different origins, prefer token-based auth (JWT) or configure cookie `Domain`/`SameSite` carefully.

This repository also supports serving the client from the server (single-host) — see earlier project configuration if you prefer that setup.

## Docker: run both services with docker-compose

A docker-compose configuration is included to build and run the server and client together. From the repository root run:

```bash
docker compose up --build
```

This will build two images and start:

- Server: exposed on `http://localhost:7138`
- Client: served by nginx on `http://localhost:5173`

Notes:

- The client Dockerfile injects an `ApiBaseUrl` pointing to the server service (`http://server:7138/`) at build time.
- The server reads `ClientOrigins` from environment to configure CORS; the compose file sets common development origins.
- If you prefer the server to serve the static client instead, revert the `ProjectReference` removal in `BlazorChat.Server.csproj` and restore the previous `Program.cs` mapping logic.

Docker Compose uses Postgres 17 for the database service; update credentials and version if you prefer a different PG release.

Important: migrations

We removed the existing EF Core migrations (they were generated for SQLite). Since this project now targets Postgres, generate new migrations locally and apply them before running the app in production.

From the solution root run (once):

```bash
dotnet tool restore
dotnet ef migrations add InitialCreate --project BlazorChat/BlazorChat.Server --startup-project BlazorChat/BlazorChat.Server
dotnet ef database update --project BlazorChat/BlazorChat.Server --startup-project BlazorChat/BlazorChat.Server
```

This will create new, Postgres-targeted migrations and apply them. The server's startup will also call `Database.Migrate()` in Development to ensure migrations are applied.
