# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.2] - 2026-09-23

### Fixed
- Pinned `jw-cicd-tools` to an immutable commit SHA instead of the mutable `v0.2.0` tag in the `resolve-version` job, and removed the unused `global-v1` context from that job to reduce secret exposure

## [1.2.1] - 2026-09-20

### Fixed
- Added `org.opencontainers.image.source` label to both the gRPC and REST host Dockerfiles so newly published GHCR packages correctly link to this repository

## [1.2.0] - 2026-09-19

### Added
- Enabled SwaggerUI for interactive API documentation
- CORS policy allowing requests from the local Angular dev server (`http://localhost:4200`)

## [1.1.1] - 2026-06-18

### Updated
- Minor update to Readme

## [1.1.0] - 2026-06-11

### Added
- New CRUD operations for Genre to allow for managing genres

## [1.0.2] - 2026-06-03

### Updated
- CircleCI publishing corrected to generate datetime tag instead of hash

## [1.0.1] - 2026-06-02

### Added
- Validation for non-null, empty names and titles
- Fixed CircleCI issue where SonarScan step fails if a PR is not open for a feature branch

## [1.0.0] - 2026-06-02

### Added
- Initial release of Jukebox.DataManager
- `Jukebox.DataManager.Grpc` — gRPC services for data access operations
- `Jukebox.DataManager.Rest` - REST API controllers for data access operations
- `Jukebox.DataManager.Contracts` - Data transfer objects (DTOs) and service contracts
- `Jukebox.DataManager.Managers` - Core business logic for managing song data
