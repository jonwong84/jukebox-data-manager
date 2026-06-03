# Jukebox Data Manager

Music metadata management layer for the Jukebox platform. Exposes two hosts:

- **REST** (`Jukebox.DataManager.Rest`) — HTTP/1.1 JSON API for end-user-facing clients, secured with JWT bearer tokens
- **gRPC** (`Jukebox.DataManager.Grpc`) — high-performance RPC API for internal service-to-service communication, secured with API keys

Both hosts sit on top of a shared manager layer (`Jukebox.DataManager.Managers`) and consume the `jukebox-data-access` NuGet packages for all database operations.

Architectural diagram for visual:

![Song metadata API architecture diagram showing a layered system: Clients layer with two client boxes labeled rRPC client generated stub consumer and REST client standard HTTP consumer; API hosts layer with rRPC host procedure-based endpoints and REST host resource-based endpoints; Business logic manager layer labeled Manager layer CRUD orchestration, validation, rules; Data logic access layer labeled Access layer Queries, writes, data mapping; and a SQL database at the bottom labeled SQL database. Arrows connect clients to hosts, hosts to manager, manager to access, and access to the database.](./docs/song_api_architecture_mid.svg)
---

## Repository Structure

```
jukebox-data-manager/
  src/
    Hosts/
      Jukebox.DataManager.Grpc/          # gRPC host
      Jukebox.DataManager.Grpc.Test/     # gRPC host tests (45 tests)
      Jukebox.DataManager.Rest/          # REST host
      Jukebox.DataManager.Rest.Test/     # REST host tests (36 tests)
    Manager/
      Jukebox.DataManager.Managers/      # Manager implementations
      Jukebox.DataManager.Managers.Test/ # Manager tests (34 tests)
      Jukebox.DataManager.Contracts/     # Interfaces and contract types
  charts/
    jukebox-data-manager/                # Umbrella Helm chart
      charts/
        jukebox-data-manager-grpc/       # gRPC sub-chart
        jukebox-data-manager-rest/       # REST sub-chart
  docs/                                  # Extended documentation (forthcoming)
  Jukebox.DataManager.slnx               # Visual Studio 2026 solution file
  HELM_DEPLOYMENT.md                     # Kubernetes / Helm deployment guide
```

---

## Quick Start

**Prerequisites:** .NET 8.0 SDK, Docker (SQL Server), a running `Jukebox` database.

```bash
# Set required environment variable
export JUKEBOX_DB_CONNECTION_STRING="Server=localhost,1433;Database=Jukebox;..."

# Run the REST host
cd src/Hosts/Jukebox.DataManager.Rest
dotnet run

# Run the gRPC host (separate terminal)
cd src/Hosts/Jukebox.DataManager.Grpc
dotnet run
```

See [docs/LOCAL_DEV.md](docs/LOCAL_DEV.md) for full setup including Helm/kind, auth configuration, and dev bypass.

---

## Documentation

| Document | Description |
|---|---|
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Project structure, data flow, key design decisions |
| [docs/LOCAL_DEV.md](docs/LOCAL_DEV.md) | Local development setup, running both hosts, dev auth |
| [docs/API_REFERENCE_REST.md](docs/API_REFERENCE_REST.md) | REST endpoint reference — all routes, auth, request/response |
| [docs/API_REFERENCE_GRPC.md](docs/API_REFERENCE_GRPC.md) | gRPC service reference — all RPCs, metadata, request/response |
| [docs/CICD.md](docs/CICD.md) | CircleCI pipeline jobs, branching strategy, image publishing |
| [AUTHENTICATION.md](AUTHENTICATION.md) | API key and JWT bearer setup, production injection patterns |
| [HELM_DEPLOYMENT.md](HELM_DEPLOYMENT.md) | Helm chart structure, kind cluster setup, deployment commands |

> Extended docs are forthcoming. `AUTHENTICATION.md` and `HELM_DEPLOYMENT.md` are currently the only complete documents.

---

## Technology Stack

| Concern | Choice |
|---|---|
| Runtime | .NET 8.0 |
| REST framework | ASP.NET Core, `Microsoft.AspNetCore.Authentication.JwtBearer` |
| gRPC framework | `Grpc.AspNetCore` |
| ORM / data access | EF Core 8 via `jukebox-data-access` packages |
| Database | SQL Server |
| Containerization | Docker (multi-stage builds) |
| Orchestration | Kubernetes via Helm (umbrella chart) |
| CI/CD | CircleCI |
| Container registry | GitHub Container Registry (`ghcr.io/jonwong84`) |
| Code quality | SonarCloud |

---

## Test Coverage

| Project | Tests |
|---|---|
| `Jukebox.DataManager.Grpc.Test` | 42 |
| `Jukebox.DataManager.Rest.Test` | 36 |
| `Jukebox.DataManager.Managers.Test` | 42 |
| **Total** | **120** |

Run all tests from the repo root:

```bash
dotnet test
```