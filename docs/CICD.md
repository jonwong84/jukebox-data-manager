# CI/CD Pipeline

The `jukebox-data-manager` repository uses CircleCI. The pipeline builds, tests, scans, and publishes Docker images for both the gRPC and REST hosts.

---

## Trigger Branches

| Branch pattern | What runs |
|---|---|
| `main` | Full pipeline — build, test, publish production-tagged images |
| `feature/*` | Full pipeline — build, test, SonarCloud scan, publish beta-tagged images |
| All other branches | No pipeline |

---

## Jobs

```
build-and-test-grpc ─┐
                      ├─ sonar-scan (feature/* only)
build-and-test-rest ─┘
        │
        ├─ publish-grpc-image
        └─ publish-rest-image
```

### `build-and-test-grpc`

Builds and tests `Jukebox.DataManager.Grpc` and `Jukebox.DataManager.Grpc.Test`.

Steps:
1. Restore NuGet packages (authenticates to GitHub Packages via `GITHUB_TOKEN`)
2. `dotnet build`
3. `dotnet test` with `coverlet.msbuild` coverage in OpenCover format
4. Persist coverage report as a workspace artifact for `sonar-scan`

### `build-and-test-rest`

Builds and tests `Jukebox.DataManager.Rest` and `Jukebox.DataManager.Rest.Test`.

Steps: identical to `build-and-test-grpc` for the REST projects.

### `sonar-scan`

Runs SonarCloud analysis. Only triggered on `feature/*` branches.

- Requires both `build-and-test-grpc` and `build-and-test-rest` to complete first
- Attaches the workspace to read both coverage reports
- SonarCloud project key: `jonwong84_jukebox-data-manager`
- Authenticated via `SONAR_TOKEN` CircleCI environment variable

### `publish-grpc-image`

Builds and pushes the gRPC Docker image to GitHub Container Registry.

- Requires `build-and-test-grpc` (and `sonar-scan` on `feature/*`)
- Build arg: `GITHUB_TOKEN` (for NuGet restore inside the Docker build)
- Registry: `ghcr.io/jonwong84/jukebox-datamanager-grpc`

### `publish-rest-image`

Builds and pushes the REST Docker image to GitHub Container Registry.

- Requires `build-and-test-rest` (and `sonar-scan` on `feature/*`)
- Registry: `ghcr.io/jonwong84/jukebox-datamanager-rest`

---

## Image Tagging

Both images are tagged consistently:

| Branch | Tags applied |
|---|---|
| `feature/*` | `beta-<git-sha>`, `latest` |
| `main` | `<git-sha>`, `latest` |

The `latest` tag always points to the most recently built image on that branch type. The SHA tag provides an immutable reference for rollbacks and Helm deployments.

---

## Environment Variables (CircleCI)

Set these in your CircleCI project settings:

| Variable | Used by | Description |
|---|---|---|
| `GITHUB_TOKEN` | All jobs, Docker build | NuGet restore from GitHub Packages; GHCR push |
| `SONAR_TOKEN` | `sonar-scan` | SonarCloud authentication |

---

## Docker Images

### Multi-stage build

Both Dockerfiles use a multi-stage build:

1. **`build` stage** — `mcr.microsoft.com/dotnet/sdk:8.0`
   - Accepts `GITHUB_TOKEN` as a `--build-arg`
   - Writes a `nuget.config` that authenticates to GitHub Packages using the token
   - `dotnet restore` → `dotnet publish -c Release`

2. **`runtime` stage** — `mcr.microsoft.com/dotnet/aspnet:8.0`
   - Copies published output from the build stage
   - No SDK, no secrets — token is not present in the final image

### Exposed ports

| Image | Ports |
|---|---|
| `jukebox-datamanager-grpc` | `5037` (HTTP/2), `7109` (HTTP/1.1+2) |
| `jukebox-datamanager-rest` | `5035` (HTTP/1.1) |

---

## Deployment

After images are published, deployment to Kubernetes is handled via Helm. See [HELM_DEPLOYMENT.md](../HELM_DEPLOYMENT.md) for full details.

The typical production deploy flow after merging to `main`:

```bash
# Pull the latest SHA from the pipeline output or GHCR, then:
helm upgrade --install jukebox-data-manager ./charts/jukebox-data-manager \
  --set jukebox-data-manager-grpc.image.tag=<sha> \
  --set jukebox-data-manager-rest.image.tag=<sha> \
  -f local-values.yaml
```

---

## Code Quality

SonarCloud analysis runs on every `feature/*` branch. The dashboard is available at:

```
https://sonarcloud.io/project/overview?id=jonwong84_jukebox-data-manager
```

Coverage is collected from both test projects via `coverlet.msbuild` in OpenCover format and uploaded as part of the SonarCloud scan.
