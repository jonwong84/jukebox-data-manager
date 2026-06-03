# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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