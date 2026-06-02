# Architecture

## Overview

Jukebox Data Manager is a stateless, horizontally scalable management layer that sits between clients and the Jukebox database. It exposes the same domain operations (Songs, Artists, Albums) through two host protocols tailored to different consumers:

```
External clients (web, mobile)
        │  JWT bearer (HTTP/1.1)
        ▼
┌─────────────────────────┐
│  Jukebox.DataManager    │
│        .Rest            │  :5035
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│  Jukebox.DataManager    │
│       .Managers         │  (shared, in-process)
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│  Jukebox.DataAccess     │  (NuGet packages)
│   (EF Core / SQL Server)│
└─────────────────────────┘
             ▲
             │
┌────────────┴────────────┐
│  Jukebox.DataManager    │
│        .Grpc            │  :5037 / :7109
└─────────────────────────┘
        │  API key (HTTP/2)
        ▼
Internal services (service-to-service)
```

---

## Project Layers

### `Jukebox.DataManager.Contracts`

Defines all interfaces (`ISongManager`, `IArtistManager`, `IAlbumManager`) and the contract types (request/result records) shared across the manager layer. No implementation details; no framework dependencies.

### `Jukebox.DataManager.Managers`

Implements the manager interfaces. Contains all business logic: input validation, orchestration of repository calls, result mapping. Registered via `AddDataManager()` extension method, which is called by both hosts.

### `Jukebox.DataManager.Grpc`

ASP.NET Core gRPC host. Responsible for:
- Protocol translation (protobuf ↔ manager contracts)
- API key authentication via `AuthInterceptor`
- Exposing `SongServiceImpl`, `ArtistServiceImpl`, `AlbumServiceImpl`
- gRPC reflection (enabled unconditionally for tooling support)

### `Jukebox.DataManager.Rest`

ASP.NET Core Web API host. Responsible for:
- JSON serialization / HTTP routing
- JWT bearer authentication
- `DevBypassMiddleware` for local development without an IdP
- Exposing `SongsController`, `ArtistsController`, `AlbumsController`

---

## Data Flow

### REST request lifecycle

```
HTTP Request
  → DevBypassMiddleware (dev only — injects fake ClaimsPrincipal)
  → UseAuthentication() — validates JWT, populates User
  → UseAuthorization() — enforces [Authorize]
  → Controller action
      → GetUserId() extracts "sub" claim
      → Manager method called
      → Repository called (via jukebox-data-access)
      → Result mapped to HTTP response
```

### gRPC request lifecycle

```
gRPC Request
  → AuthInterceptor
      → reads x-api-key from metadata
      → validates against ApiKeys config dictionary
      → sets context.UserState["userId"] = logical key name
  → Service implementation
      → GetUserId() reads UserState["userId"]
      → Manager method called
      → Repository called (via jukebox-data-access)
      → Result mapped to protobuf response
```

---

## Authentication Design

Two schemes, one per host, chosen to match their consumer profile:

| Host | Scheme | Consumer | UserId source |
|---|---|---|---|
| REST | JWT bearer | End users (via IdP) | `sub` claim |
| gRPC | API key | Internal services | Logical key name from config |

See [AUTHENTICATION.md](../AUTHENTICATION.md) for full details.

---

## Key Design Decisions

1. **Two hosts, one manager layer.** Both hosts call the same `IXxxManager` interfaces. Protocol-specific concerns (auth, serialization) are fully isolated in the host projects. Adding a third protocol (e.g. GraphQL) requires no changes to the manager layer.

2. **gRPC auth via interceptor, not per-RPC.** `AuthInterceptor` is registered globally (`options.Interceptors.Add<AuthInterceptor>()`), so every RPC is protected without any per-method boilerplate. All four handler types (unary, client streaming, server streaming, duplex) are covered.

3. **REST auth is IdP-agnostic.** `Authority` and `Audience` are pure configuration values. Swapping identity providers (Auth0 → Entra ID, etc.) requires no code change.

4. **`UserId` comes from auth context, not the request body.** Both hosts derive `UserId` from their respective auth context after authentication succeeds. This prevents clients from impersonating other users and ensures the audit trail is trustworthy.

5. **Dev bypass via middleware, not config flag in prod.** `DevBypassMiddleware` is registered only when `app.Environment.IsDevelopment()` is true. The `Auth:DevBypass` config flag is an additional guard but the middleware itself never runs in production.

6. **NuGet packages for data access.** `Jukebox.DataAccess`, `Jukebox.DataAccess.Contracts`, and `Jukebox.DataAccess.EntityFramework` are consumed as versioned NuGet packages from GitHub Packages. The manager solution has no project references to the data access solution — only package references. This enforces a clean boundary and enables independent versioning.

7. **Umbrella Helm chart.** A single `helm upgrade` deploys both hosts together with shared values. Sub-charts can still be deployed independently if needed.

8. **API key names as user IDs.** For gRPC, the logical key name (e.g. `service-a`) becomes the `UserId` stored against any created/updated records. This makes it easy to trace which service performed a given operation in the audit log.

9. **gRPC reflection enabled unconditionally.** Reflection is on in all environments to support tooling (`grpcurl`, Postman, etc.) without environment-specific configuration.

10. **`.slnx` solution format.** Uses Visual Studio 2026's new `.slnx` format — lighter XML, no GUIDs, easier to diff and merge.
