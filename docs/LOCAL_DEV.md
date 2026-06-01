# Local Development

## Prerequisites

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | 8.0 | |
| Docker Desktop | Any recent | For SQL Server |
| Visual Studio 2026 | or Rider / VS Code | `.slnx` support required for VS |
| `grpcurl` | Any | Optional — for testing gRPC locally |

---

## 1. Database

The REST and gRPC hosts both require a running SQL Server instance with the `Jukebox` database and schema applied. The schema is managed by the `jukebox-data-access` repo.

### Start SQL Server via Docker

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pass3word3!" \
  -p 1433:1433 --name jukebox-sql -d \
  mcr.microsoft.com/mssql/server:2022-latest
```

### Set the connection string

Set `JUKEBOX_DB_CONNECTION_STRING` as an environment variable before running either host:

**Windows (PowerShell):**
```powershell
$env:JUKEBOX_DB_CONNECTION_STRING = "Server=localhost,1433;Database=Jukebox;User Id=sa;Password=Pass3word3!;TrustServerCertificate=True;"
```

**macOS / Linux:**
```bash
export JUKEBOX_DB_CONNECTION_STRING="Server=localhost,1433;Database=Jukebox;User Id=sa;Password=Pass3word3!;TrustServerCertificate=True;"
```

Apply migrations from the `jukebox-data-access` repo if you haven't already:

```bash
dotnet ef database update \
  --project Jukebox.DataAccess.Migrations \
  --startup-project Jukebox.DataAccess.Migrations
```

---

## 2. NuGet Authentication

The manager solution consumes packages from GitHub Packages. You need a GitHub Personal Access Token (PAT) with `read:packages` scope.

Set it as an environment variable:

```bash
export GITHUB_TOKEN=ghp_your_token_here   # macOS / Linux
$env:GITHUB_TOKEN = "ghp_your_token_here"  # Windows PowerShell
```

The `nuget.config` at the repo root picks this up automatically via `%GITHUB_TOKEN%`.

---

## 3. Running the REST Host

```bash
cd src/Hosts/Jukebox.DataManager.Rest
dotnet run
```

The REST host starts on:
- HTTP: `http://localhost:5035`
- HTTPS: `https://localhost:7157`

### Authentication in development

`appsettings.Development.json` sets `Auth:DevBypass: true`, which activates `DevBypassMiddleware`. This injects a fake `ClaimsPrincipal` with `sub: dev-user` on every request — **no JWT or IdP required**.

You can call any endpoint directly:

```bash
curl http://localhost:5035/artists
```

To test with real JWT auth locally, set `Auth:DevBypass: false` in your local config and provide `Auth:Authority` and `Auth:Audience` pointing to a local or dev IdP instance.

---

## 4. Running the gRPC Host

```bash
cd src/Hosts/Jukebox.DataManager.Grpc
dotnet run
```

The gRPC host starts on:
- `http://localhost:5037` — HTTP/2 only (gRPC)
- `http://localhost:7109` — HTTP/1.1 and HTTP/2 (for mixed clients)

### Authentication in development

All gRPC requests require an `x-api-key` metadata header. `appsettings.Development.json` ships with one pre-configured key:

```json
"ApiKeys": {
  "grpcurl-local": "dev-key-123"
}
```

Pass it with `grpcurl`:

```bash
grpcurl -plaintext \
  -H "x-api-key: dev-key-123" \
  localhost:5037 \
  list
```

To add more dev keys, add entries to `ApiKeys` in `appsettings.Development.json`:

```json
"ApiKeys": {
  "grpcurl-local": "dev-key-123",
  "my-service": "another-secret"
}
```

---

## 5. Running the Tests

```bash
# All tests from repo root
dotnet test

# Specific project
dotnet test src/Hosts/Jukebox.DataManager.Grpc.Test
dotnet test src/Hosts/Jukebox.DataManager.Rest.Test
```

Tests are fully self-contained — they do not require a running database or auth service.

---

## 6. Kubernetes / Helm (Local kind Cluster)

For end-to-end local testing with the full Helm chart, see [HELM_DEPLOYMENT.md](../HELM_DEPLOYMENT.md).

Summary of steps:

1. Start a `kind` cluster
2. Build and load Docker images into kind
3. Create a `local-values.yaml` (gitignored) with your connection string and API keys — see template below
4. `helm upgrade --install` with `local-values.yaml`
5. Port-forward to reach services

### `local-values.yaml` template

```yaml
# This file is gitignored. Do not commit secrets.
jukebox-data-manager-grpc:
  env:
    JUKEBOX_DB_CONNECTION_STRING: "Server=172.18.0.1,1433;Database=Jukebox;User Id=sa;Password=Pass3word3!;TrustServerCertificate=True;Encrypt=False;"
    ApiKeys__grpcurl-local: "dev-key-123"

jukebox-data-manager-rest:
  env:
    JUKEBOX_DB_CONNECTION_STRING: "Server=172.18.0.1,1433;Database=Jukebox;User Id=sa;Password=Pass3word3!;TrustServerCertificate=True;Encrypt=False;"
    Auth__Authority: "https://your-dev-idp.example.com"
    Auth__Audience: "your-api-audience"
```

> **Note:** `172.18.0.1` is the kind cluster's gateway IP that reaches the Docker host's SQL Server. This differs from `localhost` which resolves inside the container.

---

## 7. Common Issues

**`401 Unauthorized` on gRPC calls**
Make sure you're passing `-H "x-api-key: dev-key-123"` and that the key matches an entry in `ApiKeys` in `appsettings.Development.json`.

**`401 Unauthorized` on REST calls**
Check that `Auth:DevBypass` is `true` in `appsettings.Development.json`, and that you're running in the `Development` environment (`ASPNETCORE_ENVIRONMENT=Development`).

**Package restore fails**
Ensure `GITHUB_TOKEN` is set and has `read:packages` scope on `github.com/jonwong84`.

**Cannot connect to SQL Server**
Confirm the Docker container is running (`docker ps`) and the port `1433` is mapped. On Windows, check Windows Firewall isn't blocking the port.
