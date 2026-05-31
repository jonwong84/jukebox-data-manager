# Helm Deployment Guide

This guide covers deploying Jukebox Data Manager to a local [kind](https://kind.sigs.k8s.io/) (Kubernetes in Docker) cluster using Helm.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [kind](https://kind.sigs.k8s.io/docs/user/quick-start/#installation)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Helm](https://helm.sh/docs/intro/install/)
- A GitHub Personal Access Token (PAT) with `read:packages` scope stored as `GITHUB_TOKEN` in your environment
- A running SQL Server instance accessible on port `1433`

## Environment Variables

The following environment variables must be set on your machine before deploying:

| Variable | Description |
|---|---|
| `GITHUB_TOKEN` | GitHub PAT with `read:packages` scope — used to pull images from ghcr.io |
| `JUKEBOX_DB_CONNECTION_STRING` | Full SQL Server connection string |

To set them permanently (requires an administrator PowerShell session):
```powershell
[System.Environment]::SetEnvironmentVariable("GITHUB_TOKEN", "<your-pat>", "Machine")
[System.Environment]::SetEnvironmentVariable("JUKEBOX_DB_CONNECTION_STRING", "<your-connection-string>", "Machine")
```

Then open a new PowerShell session to pick them up.

## One-Time Setup

### 1. Create the kind cluster

```powershell
kind create cluster
```

### 2. Authenticate with GitHub Container Registry

```powershell
echo $env:GITHUB_TOKEN | docker login ghcr.io -u jonwong84 --password-stdin
```

### 3. Pull the Docker images

```powershell
docker pull ghcr.io/jonwong84/jukebox-datamanager-grpc:latest
docker pull ghcr.io/jonwong84/jukebox-datamanager-rest:latest
```

### 4. Load the images into kind

kind does not pull images from registries directly — they must be loaded from your local Docker daemon:

```powershell
kind load docker-image ghcr.io/jonwong84/jukebox-datamanager-grpc:latest
kind load docker-image ghcr.io/jonwong84/jukebox-datamanager-rest:latest
```

### 5. Create local-values.yaml

Create a `local-values.yaml` file in the repo root with your connection string. This file is gitignored and must never be committed.

```yaml
jukebox-data-manager-grpc:
  env:
    JUKEBOX_DB_CONNECTION_STRING: "<your-connection-string>"

jukebox-data-manager-rest:
  env:
    JUKEBOX_DB_CONNECTION_STRING: "<your-connection-string>"
```

> **Note:** When connecting from inside the kind cluster, `localhost` in your connection string must be replaced with the kind network gateway IP. To find it:
> ```powershell
> docker inspect kind-control-plane --format "{{ .NetworkSettings.Networks.kind.Gateway }}"
> ```
> Use the returned IP (e.g. `172.18.0.1`) in place of `localhost` in the connection string.

## Deploying

### Install

```powershell
helm install jukebox ./charts/jukebox-data-manager -f local-values.yaml
```

### Upgrade (after changes or config updates)

```powershell
helm upgrade jukebox ./charts/jukebox-data-manager -f local-values.yaml
```

### Verify pods are running

```powershell
kubectl get pods
```

Both pods should show `Running` with `READY 1/1`.

## Accessing the Services

The services are exposed as NodePort but require `kubectl port-forward` to reach from your host machine. Run each in a separate terminal and keep them open while testing.

### REST host

```powershell
kubectl port-forward pod/<rest-pod-name> 5035:8080
```

The REST API is then available at `http://localhost:5035`. Example:

```powershell
curl http://localhost:5035/api/artists -UseBasicParsing
```

### gRPC host

```powershell
kubectl port-forward pod/<grpc-pod-name> 5037:5037
```

> To get the current pod names: `kubectl get pods`

## Uninstalling

```powershell
helm uninstall jukebox
```

## Tearing Down the Cluster

```powershell
kind delete cluster
```

> After recreating the cluster you will need to repeat steps 4 onwards — the loaded images do not persist across cluster deletions.
