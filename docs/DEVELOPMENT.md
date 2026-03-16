# BlazorChat Development Guide

This document complements the root README with implementation-level details for contributors.

## Solution structure

- `BlazorChat/BlazorChat.Client` - Blazor WebAssembly UI
- `BlazorChat/BlazorChat.Server` - API, auth, SignalR, and persistence
- `BlazorChat/BlazorChat.Shared` - DTOs and contracts shared by client/server

## Runtime flow

1. User opens the Blazor client.
2. Client calls `/api/auth/*` for register/login/logout/me.
3. Auth is maintained with cookie `auth_token`.
4. Authorized requests access protected API routes.
5. Chat page opens a SignalR connection to `/hubs/chat`.
6. Message creation persists via HTTP, then server broadcasts via hub groups.

## Database and seeding behavior

In `Development`, the server startup currently performs a full reset-and-seed:

- `EnsureDeleted()`
- `EnsureCreated()`
- inserts sample users/friendships/server/channels/messages/DM

Implication: local dev data is not persistent across server restarts when environment is Development.

## API reference

All routes below are server routes.

### Auth (`/api/auth`)

- `POST /api/auth/login`
  - Body: `LoginDto`
  - Result: signs in user, returns `MeDto`
- `POST /api/auth/register`
  - Body: `CreateUserDto`
  - Result: creates user, signs in, returns `MeDto`
- `POST /api/auth/logout`
  - Result: clears auth cookie
- `GET /api/auth/me` (authorized)
  - Result: current user info (`MeDto`)

### Servers (`/api/servers`) (authorized)

- `GET /api/servers`
  - List servers where current user is a member
- `POST /api/servers`
  - Body: `CreateServerDto`
  - Creates server, auto-creates `general` channel, adds owner membership
- `GET /api/servers/{id}/channels`
  - Lists channels if requester is server member

### Friendships (`/api/friendships`) (authorized)

- `GET /api/friendships`
  - Accepted friends list
- `GET /api/friendships/pending`
  - Incoming pending requests
- `POST /api/friendships`
  - Body: plain string username
  - Sends friend request to target username
- `PATCH /api/friendships/{requesterId}`
  - Body: boolean `accept`
  - Accept or reject incoming request

### Direct messages (`/api/dms`) (authorized)

- `GET /api/dms`
  - All DM conversations for current user
- `POST /api/dms/{targetUserId}`
  - Opens existing or creates new DM conversation

### Messages (`/api/messages`) (authorized)

- `GET /api/messages?channelId=<id>&count=<n>`
  - Channel message history (membership required)
- `GET /api/messages?directMessageId=<id>&count=<n>`
  - DM message history (participant required)
- `POST /api/messages`
  - Body: `SendMessageDto`
  - Sends message to channel or DM, then broadcasts over SignalR

### Users (`/api/users`) (authorized)

- `GET /api/users/search?q=<term>`
  - Username search for friend add flow (min 2 chars)
- `PATCH /api/users/me/status`
  - Body: `UpdateStatusDto`
  - Updates current user status

## SignalR

- Hub endpoint: `/hubs/chat`
- Broadcast groups used by server:
  - `channel:{channelId}`
  - `dm:{directMessageId}`

## CORS and origins

Server CORS policy name: `AllowClient`.

- Reads `ClientOrigins` from configuration
- Accepts either:
  - array form in JSON
  - semicolon-separated string (useful via environment variables)

Credentials are enabled, which is required for cookie auth across origins.

## Docker notes

- Client container serves static files via nginx on port 80.
- nginx proxies:
  - `/api/*` -> `http://server:7138/api/*`
  - `/hubs/*` -> `http://server:7138/hubs/*` (WebSocket upgrade enabled)
- Server container waits for Postgres availability before starting app.

## Contributor checklist

When changing API contracts:

1. Update DTOs in `BlazorChat.Shared`.
2. Update server controllers/hub as needed.
3. Update client service usage and UI flows.
4. Update this file and README for any behavior changes.

When changing persistence model:

1. Keep Development reset behavior in mind (or replace it if introducing migrations).
2. Verify seed data still aligns with UI assumptions.
3. Validate queries enforcing membership/authorization rules.
