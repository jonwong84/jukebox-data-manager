# Authentication

## gRPC Host

The gRPC host uses API key authentication enforced via `AuthInterceptor`, which runs globally on all endpoints.

### How It Works

Clients must include an `x-api-key` metadata header with every request. The interceptor validates the key against the configured `ApiKeys` dictionary. If the key is missing or invalid, the server returns `StatusCode.Unauthenticated`.

The logical name of the matched key (e.g. `service-a`) is used as the `UserId` for audit trail purposes (`CreatedBy` / `UpdatedBy` on all write operations).

### Configuration

API keys are configured as a dictionary under the `ApiKeys` section, where each entry is a `name: secret` pair:

```json
"ApiKeys": {
  "service-a": "your-secret-key-value"
}
```

Multiple keys are supported to allow independent rotation per caller.

### Local Development

Add keys to `appsettings.Development.json`:

```json
"ApiKeys": {
  "grpcurl-local": "dev-key-123"
}
```

To test with grpcurl:

```bash
grpcurl -H "x-api-key: dev-key-123" -plaintext localhost:5037 list
```

### Production

Do not put keys in `appsettings.Production.json`. Inject them as environment variables using .NET's double-underscore convention:

```
ApiKeys__service-a=your-secret-key-value
```

In `local-values.yaml` (gitignored):

```yaml
jukebox-data-manager-grpc:
  env:
    ApiKeys__service-a: "your-secret-key-value"
```

In Helm `values.yaml`, add the key name with an empty value as a placeholder:

```yaml
env:
  ASPNETCORE_ENVIRONMENT: Production
  JUKEBOX_DB_CONNECTION_STRING: ""
  ApiKeys__service-a: ""
```

Rotate a key by updating the environment variable and restarting the pod — no redeployment required if using `kubectl rollout restart`.

---

## REST Host

The REST host uses JWT bearer authentication. All endpoints require a valid token.

### How It Works

Clients must include a JWT in the `Authorization` header:

```
Authorization: Bearer <token>
```

The token is validated against the configured `Authority` and `Audience`. The `sub` claim is used as the `UserId` for audit trail purposes.

### Configuration

```json
"Auth": {
  "Authority": "https://your-idp.example.com",
  "Audience": "your-api-audience"
}
```

These are IdP-agnostic — any standards-compliant JWT issuer (Auth0, Azure AD, Okta, etc.) works by updating these two values.

### Local Development

The dev bypass is enabled in `appsettings.Development.json`:

```json
"Auth": {
  "DevBypass": true
}
```

When enabled, all requests are automatically authenticated as `dev-user` without requiring a token. This is active in the `Development` environment only and is never compiled out — it is conditional on the `Auth:DevBypass` config value.

**Never set `DevBypass: true` in production.**

### Production

Inject `Authority` and `Audience` via environment variables:

```
Auth__Authority=https://your-idp.example.com
Auth__Audience=your-api-audience
```

In `local-values.yaml` (gitignored):

```yaml
jukebox-data-manager-rest:
  env:
    Auth__Authority: "https://your-idp.example.com"
    Auth__Audience: "your-api-audience"
```

In Helm `values.yaml`, add placeholders:

```yaml
env:
  ASPNETCORE_ENVIRONMENT: Production
  JUKEBOX_DB_CONNECTION_STRING: ""
  Auth__Authority: ""
  Auth__Audience: ""
```
