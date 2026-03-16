# BlazorChat

Real-time chat app built with Blazor WebAssembly, ASP.NET Core, EF Core, PostgreSQL, and SignalR.

## What is in this repository

- `BlazorChat.Client` - Blazor WebAssembly frontend (MudBlazor UI)
- `BlazorChat.Server` - ASP.NET Core Web API + SignalR hub + EF Core
- `BlazorChat.Shared` - shared DTOs and hub contracts

Target framework for all projects: `net10.0`.

## Architecture at a glance

- Auth: cookie authentication (`auth_token`), issued by server auth endpoints.
- API base path: `/api/*`.
- SignalR hub: `/hubs/chat`.
- Client-to-server networking:
  - Client uses current browser origin as `HttpClient.BaseAddress`.
  - In Docker, nginx proxies `/api/*` and `/hubs/*` from client container to server container.
- Database: PostgreSQL (default in compose: `postgres:17`).

## Quick start (Docker, recommended)

From repository root:

```bash
docker compose up --build
```

Services:

- Client: `http://localhost:5173`
- Server: `http://localhost:7138`
- Postgres: internal service `postgres:5432`

Stop:

```bash
docker compose down
```

Reset DB volume:

```bash
docker compose down -v
```

## Run locally without Docker

Prerequisites:

- .NET SDK 10
- PostgreSQL instance reachable by the server

1. Configure server connection string in `BlazorChat/BlazorChat.Server/appsettings.json` or environment variable `ConnectionStrings__DefaultConnection`.
2. From repository root run server:

```bash
dotnet run --project BlazorChat/BlazorChat.Server
```

3. In another terminal run client:

```bash
dotnet run --project BlazorChat/BlazorChat.Client
```

4. Open the client URL shown by the client host (typically `http://localhost:5173`).

## Development data behavior (important)

When `ASPNETCORE_ENVIRONMENT=Development`, the server currently does this on startup:

1. Deletes the existing database schema
2. Recreates schema with `EnsureCreated()`
3. Seeds demo users, friendships, server/channels, and messages

This means any previous data is wiped each time the server starts in Development.

Seeded users:

- `alice@example.com` / `devpassword`
- `bob@example.com` / `devpassword`
- `carol@example.com` / `devpassword`

## Configuration

Key server settings:

- `ConnectionStrings:DefaultConnection` - PostgreSQL connection string
- `ClientOrigins` - allowed origins for CORS with credentials (array or `;`-separated string)

Default compose values:

- DB: `Host=postgres;Port=5432;Database=blazorchat;Username=postgres;Password=example`
- Allowed origins: `http://localhost:5173;http://client:80`

## API overview

Main route groups:

- `/api/auth` - register, login, logout, current user
- `/api/servers` - list/create servers, list channels
- `/api/friendships` - friends, pending requests, request/respond
- `/api/dms` - list/open direct message conversations
- `/api/messages` - fetch/send channel or DM messages
- `/api/users` - user search, status updates

For a concise endpoint reference, see `docs/DEVELOPMENT.md`.

## Notes for production hardening

Current code is developer-oriented. Before production use, at minimum:

- Replace destructive Development DB initialization with migrations.
- Move secrets and connection strings to secure environment configuration.
- Review cookie settings (Secure, SameSite, domain) for deployed origins.
- Add structured observability, health checks, and backup strategy.

## Useful commands

Build all projects:

```bash
dotnet build BlazorChat.slnx
```

Restore local tools (if you use EF CLI from tool manifest):

```bash
dotnet tool restore
```
