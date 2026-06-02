# gRPC API Reference

Host addresses:
- `localhost:5037` — HTTP/2 only (development)
- `localhost:7109` — HTTP/1.1 + HTTP/2 (development, mixed clients)
- Production addresses are configured via Helm values / Kubernetes service

All RPCs require an `x-api-key` metadata header. See [AUTHENTICATION.md](../AUTHENTICATION.md) for key configuration.

---

## Authentication

Every RPC is protected by `AuthInterceptor`, which validates the `x-api-key` metadata header against the `ApiKeys` configuration dictionary. The logical key name becomes the `UserId` recorded on created/updated entities.

```
x-api-key: <api-key-value>
```

On failure the interceptor returns:
- `StatusCode.Unauthenticated` — key missing or not found in configuration

See [AUTHENTICATION.md](../AUTHENTICATION.md) for production key injection.

---

## Exploring the API

gRPC reflection is enabled on all environments. Use `grpcurl` to list services and describe messages:

```bash
# List all services
grpcurl -plaintext -H "x-api-key: dev-key-123" localhost:5037 list

# Describe a service
grpcurl -plaintext -H "x-api-key: dev-key-123" localhost:5037 describe jukebox.ArtistService

# Describe a message type
grpcurl -plaintext -H "x-api-key: dev-key-123" localhost:5037 describe jukebox.Artist
```

---

## ArtistService

### `ListArtists`

Returns a paginated list of artists.

**Request: `ListArtistsRequest`**

| Field | Type | Description |
|---|---|---|
| `page` | int32 | Page number (1-based, default: 1) |
| `page_size` | int32 | Items per page (default: 20) |

**Response: `ListArtistsResponse`**

| Field | Type | Description |
|---|---|---|
| `artists` | repeated `Artist` | Page of results |
| `total_count` | int32 | Total number of artists |
| `page` | int32 | Current page |
| `page_size` | int32 | Items per page |

**Example (`grpcurl`):**

```bash
grpcurl -plaintext \
  -H "x-api-key: dev-key-123" \
  -d '{"page": 1, "page_size": 10}' \
  localhost:5037 \
  jukebox.ArtistService/ListArtists
```

---

### `GetArtist`

**Request: `GetArtistRequest`**

| Field | Type | Description |
|---|---|---|
| `id` | int32 | Artist ID |

**Response: `Artist`**

**Status codes:**
- `NOT_FOUND` — artist does not exist

---

### `CreateArtist`

**Request: `CreateArtistRequest`**

| Field | Type | Required | Description |
|---|---|---|---|
| `name` | string | Yes | Artist name |
| `bio` | string | No | Artist biography |

**Response: `Artist`**

---

### `UpdateArtist`

**Request: `UpdateArtistRequest`**

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | int32 | Yes | Artist ID |
| `name` | string | No | Updated name |
| `bio` | string | No | Updated biography |

**Response: `Artist`**

**Status codes:**
- `NOT_FOUND` — artist does not exist

---

### `DeleteArtist`

**Request: `DeleteArtistRequest`**

| Field | Type | Description |
|---|---|---|
| `id` | int32 | Artist ID |

**Response: `DeleteArtistResponse`** (empty)

**Status codes:**
- `NOT_FOUND` — artist does not exist
- `FAILED_PRECONDITION` — artist has associated songs (delete restricted)

---

## AlbumService

### `ListAlbums`

**Request: `ListAlbumsRequest`**

| Field | Type | Description |
|---|---|---|
| `page` | int32 | Page number (1-based, default: 1) |
| `page_size` | int32 | Items per page (default: 20) |

**Response: `ListAlbumsResponse`**

| Field | Type | Description |
|---|---|---|
| `albums` | repeated `Album` | Page of results |
| `total_count` | int32 | Total number of albums |
| `page` | int32 | Current page |
| `page_size` | int32 | Items per page |

---

### `GetAlbum`

**Request: `GetAlbumRequest`**

| Field | Type | Description |
|---|---|---|
| `id` | int32 | Album ID |

**Response: `Album`**

**Status codes:**
- `NOT_FOUND` — album does not exist

---

### `CreateAlbum`

**Request: `CreateAlbumRequest`**

| Field | Type | Required | Description |
|---|---|---|---|
| `title` | string | Yes | Album title |
| `release_date` | string | No | ISO 8601 date string |
| `is_compilation` | bool | No | Default: false |
| `description` | string | No | Album description |
| `artist_ids` | repeated int32 | No | Associated artist IDs |

**Response: `Album`**

---

### `UpdateAlbum`

**Request: `UpdateAlbumRequest`**

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | int32 | Yes | Album ID |
| `title` | string | No | Updated title |
| `release_date` | string | No | Updated release date |
| `is_compilation` | bool | No | Updated compilation flag |
| `description` | string | No | Updated description |
| `artist_ids` | repeated int32 | No | Updated artist IDs |

**Response: `Album`**

---

### `DeleteAlbum`

**Request: `DeleteAlbumRequest`**

| Field | Type | Description |
|---|---|---|
| `id` | int32 | Album ID |

**Response: `DeleteAlbumResponse`** (empty)

Associated songs have their `album_id` cleared on delete (`SetNull`).

---

## SongService

### `ListSongs`

**Request: `ListSongsRequest`**

| Field | Type | Description |
|---|---|---|
| `page` | int32 | Page number (1-based, default: 1) |
| `page_size` | int32 | Items per page (default: 20) |

**Response: `ListSongsResponse`**

| Field | Type | Description |
|---|---|---|
| `songs` | repeated `Song` | Page of results |
| `total_count` | int32 | Total number of songs |
| `page` | int32 | Current page |
| `page_size` | int32 | Items per page |

---

### `GetSong`

**Request: `GetSongRequest`**

| Field | Type | Description |
|---|---|---|
| `id` | int32 | Song ID |

**Response: `Song`**

**Status codes:**
- `NOT_FOUND` — song does not exist

---

### `CreateSong`

**Request: `CreateSongRequest`**

| Field | Type | Required | Description |
|---|---|---|---|
| `title` | string | Yes | Song title |
| `artist_id` | int32 | Yes | Performing artist ID |
| `album_id` | int32 | No | Album ID |
| `duration_seconds` | int32 | Yes | Duration in seconds |
| `track_number` | int32 | No | Track position on album |
| `bpm` | int32 | No | Beats per minute |
| `genre_ids` | repeated int32 | No | Associated genre IDs |
| `lyrics` | string | No | Song lyrics text |

**Response: `Song`**

---

### `UpdateSong`

**Request: `UpdateSongRequest`**

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | int32 | Yes | Song ID |
| `title` | string | No | Updated title |
| `album_id` | int32 | No | Updated album ID |
| `duration_seconds` | int32 | No | Updated duration |
| `track_number` | int32 | No | Updated track number |
| `bpm` | int32 | No | Updated BPM |
| `genre_ids` | repeated int32 | No | Updated genre IDs |
| `lyrics` | string | No | Updated lyrics |

**Response: `Song`**

---

### `DeleteSong`

**Request: `DeleteSongRequest`**

| Field | Type | Description |
|---|---|---|
| `id` | int32 | Song ID |

**Response: `DeleteSongResponse`** (empty)

Associated lyrics are deleted in cascade.

---

## Message Types

### `Artist`

| Field | Type | Description |
|---|---|---|
| `id` | int32 | |
| `name` | string | |
| `bio` | string | May be empty |
| `created_at` | string | ISO 8601 UTC |
| `created_by` | string | `UserId` of creator |

### `Album`

| Field | Type | Description |
|---|---|---|
| `id` | int32 | |
| `title` | string | |
| `release_date` | string | ISO 8601, may be empty |
| `is_compilation` | bool | |
| `description` | string | May be empty |
| `created_at` | string | ISO 8601 UTC |
| `created_by` | string | `UserId` of creator |
| `artists` | repeated `ArtistRef` | |

### `Song`

| Field | Type | Description |
|---|---|---|
| `id` | int32 | |
| `title` | string | |
| `artist` | `ArtistRef` | |
| `album` | `AlbumRef` | May be absent |
| `duration_seconds` | int32 | |
| `track_number` | int32 | 0 if not set |
| `bpm` | int32 | 0 if not set |
| `genres` | repeated `GenreRef` | |
| `lyrics` | string | May be empty |
| `created_at` | string | ISO 8601 UTC |
| `created_by` | string | `UserId` of creator |

### `ArtistRef` / `AlbumRef` / `GenreRef`

Lightweight reference types used in nested contexts:

| Field | Type |
|---|---|
| `id` | int32 |
| `name` / `title` | string |

---

## Common Status Codes

| gRPC Status | Meaning |
|---|---|
| `OK` | Success |
| `NOT_FOUND` | Resource does not exist |
| `INVALID_ARGUMENT` | Missing required field or invalid value |
| `UNAUTHENTICATED` | Missing or invalid `x-api-key` |
| `FAILED_PRECONDITION` | Operation blocked by a constraint (e.g. deleting an artist with songs) |
| `INTERNAL` | Unexpected server error |
