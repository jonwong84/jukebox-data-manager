# REST API Reference

Base URL: `http://localhost:5035` (development) / `http://<host>:5035` (production)

All endpoints require a valid JWT bearer token in the `Authorization` header, except in development with `Auth:DevBypass: true`.

```
Authorization: Bearer <jwt_token>
```

---

## Authentication

The REST host uses JWT bearer authentication. Tokens are issued by an external identity provider (IdP) configured via `Auth:Authority` and `Auth:Audience`.

The `sub` claim in the token is used as the `UserId` for all audit trail fields (`CreatedBy`, `UpdatedBy`).

See [AUTHENTICATION.md](../AUTHENTICATION.md) for full details including dev bypass and production configuration.

---

## Artists

### List Artists

```
GET /artists
```

Returns a paginated list of artists.

**Query parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `page` | int | No | Page number (1-based, default: 1) |
| `pageSize` | int | No | Items per page (default: 20) |

**Response `200 OK`:**

```json
{
  "items": [
    {
      "id": 1,
      "name": "Radiohead",
      "bio": "English rock band formed in Abingdon, Oxfordshire.",
      "createdAt": "2024-01-15T10:30:00Z",
      "createdBy": "user-abc"
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20
}
```

---

### Get Artist

```
GET /artists/{id}
```

**Path parameters:**

| Parameter | Type | Description |
|---|---|---|
| `id` | int | Artist ID |

**Response `200 OK`:**

```json
{
  "id": 1,
  "name": "Radiohead",
  "bio": "English rock band formed in Abingdon, Oxfordshire.",
  "createdAt": "2024-01-15T10:30:00Z",
  "createdBy": "user-abc"
}
```

**Response `404 Not Found`** — artist does not exist.

---

### Create Artist

```
POST /artists
```

**Request body:**

```json
{
  "name": "Radiohead",
  "bio": "English rock band formed in Abingdon, Oxfordshire."
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `name` | string | Yes | Artist name |
| `bio` | string | No | Artist biography |

**Response `201 Created`:**

```json
{
  "id": 1,
  "name": "Radiohead",
  "bio": "English rock band formed in Abingdon, Oxfordshire.",
  "createdAt": "2024-01-15T10:30:00Z",
  "createdBy": "user-abc"
}
```

---

### Update Artist

```
PUT /artists/{id}
```

**Path parameters:**

| Parameter | Type | Description |
|---|---|---|
| `id` | int | Artist ID |

**Request body:**

```json
{
  "name": "Radiohead",
  "bio": "Updated biography."
}
```

**Response `200 OK`** — returns updated artist.

**Response `404 Not Found`** — artist does not exist.

---

### Delete Artist

```
DELETE /artists/{id}
```

**Response `204 No Content`** — artist deleted.

**Response `404 Not Found`** — artist does not exist.

> **Note:** Deleting an artist that has associated songs will fail with `409 Conflict` due to the `DeleteBehavior.Restrict` constraint on the `Artist → Songs` relationship.

---

## Albums

### List Albums

```
GET /albums
```

**Query parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `page` | int | No | Page number (1-based, default: 1) |
| `pageSize` | int | No | Items per page (default: 20) |

**Response `200 OK`:**

```json
{
  "items": [
    {
      "id": 1,
      "title": "OK Computer",
      "releaseDate": "1997-06-16T00:00:00Z",
      "isCompilation": false,
      "createdAt": "2024-01-15T10:30:00Z",
      "createdBy": "user-abc",
      "artists": [
        { "id": 1, "name": "Radiohead" }
      ]
    }
  ],
  "totalCount": 15,
  "page": 1,
  "pageSize": 20
}
```

---

### Get Album

```
GET /albums/{id}
```

**Response `200 OK`:**

```json
{
  "id": 1,
  "title": "OK Computer",
  "releaseDate": "1997-06-16T00:00:00Z",
  "isCompilation": false,
  "description": "Third studio album by Radiohead.",
  "createdAt": "2024-01-15T10:30:00Z",
  "createdBy": "user-abc",
  "artists": [
    { "id": 1, "name": "Radiohead" }
  ]
}
```

**Response `404 Not Found`** — album does not exist.

---

### Create Album

```
POST /albums
```

**Request body:**

```json
{
  "title": "OK Computer",
  "releaseDate": "1997-06-16",
  "isCompilation": false,
  "description": "Third studio album by Radiohead.",
  "artistIds": [1]
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `title` | string | Yes | Album title |
| `releaseDate` | date | No | ISO 8601 date |
| `isCompilation` | bool | No | Default: `false` |
| `description` | string | No | Album description |
| `artistIds` | int[] | No | Associated artist IDs |

**Response `201 Created`** — returns created album.

---

### Update Album

```
PUT /albums/{id}
```

**Response `200 OK`** — returns updated album.

**Response `404 Not Found`** — album does not exist.

---

### Delete Album

```
DELETE /albums/{id}
```

**Response `204 No Content`** — album deleted. Associated songs have their `AlbumId` set to `null` (`DeleteBehavior.SetNull`).

**Response `404 Not Found`** — album does not exist.

---

## Songs

### List Songs

```
GET /songs
```

**Query parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `page` | int | No | Page number (1-based, default: 1) |
| `pageSize` | int | No | Items per page (default: 20) |

**Response `200 OK`:**

```json
{
  "items": [
    {
      "id": 1,
      "title": "Karma Police",
      "duration": "00:04:21",
      "trackNumber": 4,
      "bpm": 76,
      "createdAt": "2024-01-15T10:30:00Z",
      "createdBy": "user-abc",
      "artist": { "id": 1, "name": "Radiohead" },
      "album": { "id": 1, "title": "OK Computer" },
      "genres": [
        { "id": 2, "name": "Alternative Rock" }
      ]
    }
  ],
  "totalCount": 200,
  "page": 1,
  "pageSize": 20
}
```

---

### Get Song

```
GET /songs/{id}
```

**Response `200 OK`** — returns full song object including lyrics if present.

**Response `404 Not Found`** — song does not exist.

---

### Create Song

```
POST /songs
```

**Request body:**

```json
{
  "title": "Karma Police",
  "artistId": 1,
  "albumId": 1,
  "duration": "00:04:21",
  "trackNumber": 4,
  "bpm": 76,
  "genreIds": [2],
  "lyrics": "Arrest this man, he talks in maths..."
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `title` | string | Yes | Song title |
| `artistId` | int | Yes | ID of the performing artist |
| `albumId` | int | No | ID of the album |
| `duration` | string | Yes | `HH:MM:SS` format |
| `trackNumber` | int | No | Track position on album |
| `bpm` | int | No | Beats per minute |
| `genreIds` | int[] | No | Associated genre IDs |
| `lyrics` | string | No | Song lyrics text |

**Response `201 Created`** — returns created song.

---

### Update Song

```
PUT /songs/{id}
```

**Response `200 OK`** — returns updated song.

**Response `404 Not Found`** — song does not exist.

---

### Delete Song

```
DELETE /songs/{id}
```

**Response `204 No Content`** — song and any associated lyrics deleted (`DeleteBehavior.Cascade`).

**Response `404 Not Found`** — song does not exist.

---

## Error Responses

All error responses follow a consistent shape:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Artist with ID 99 was not found."
}
```

| Status | Meaning |
|---|---|
| `400 Bad Request` | Validation error — missing required field or invalid value |
| `401 Unauthorized` | Missing or invalid JWT token |
| `403 Forbidden` | Token valid but insufficient permissions |
| `404 Not Found` | Resource does not exist |
| `409 Conflict` | Delete blocked by a referential integrity constraint |
| `500 Internal Server Error` | Unexpected server error |
