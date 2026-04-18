# Social Login .NET Demo

A production-ready ASP.NET Core Web API demonstrating Google OAuth2 social login with JWT authentication. The solution follows Clean Architecture principles where practical and separates concerns across Domain, Infrastructure, Common and Presentation layers.

---

## Features

- **Google Social Login** — Verify Google ID tokens using the official Google Auth library
- **Auto User Registration** — New users are automatically registered on first login
- **JWT Access Tokens** — Short-lived signed JWTs with role claims
- **Refresh Token Flow** — Rotate refresh tokens on each use (single-use tokens)
- **Refresh Token Revocation** — Explicit logout / token revoke endpoint
- **ASP.NET Core Identity** — Full Identity integration with role seeding
- **Clean Architecture** — Domain / Application / Infrastructure / API layers
- **CQRS with MediatR** — Commands and query handlers with pipeline behaviors
- **FluentValidation** — Request validation via MediatR pipeline behavior
- **Result Pattern** — No thrown exceptions for business logic; strongly typed results
- **Options Pattern** — Strongly typed configuration for JWT and Google Auth
- **API Versioning** — URL-segment versioning (`/api/v1/...`)
- **Global Exception Handling** — Middleware catches all unhandled exceptions
- **Consistent Error Responses** — Every error returns the same `ApiResponse` envelope
- **PostgreSQL + EF Core** — Code-first schema with migrations
- **Swagger UI** — Interactive API documentation with Bearer token support

---

## Architecture

```
src/
├── Domain/                        # Enterprise business rules (Entities, Errors, Common types)
│   ├── Common/                    # Result, Error, MaybeException
│   ├── Entities/                  # ApplicationUser, RefreshToken
│   └── Errors/                    # Domain-specific error types
│
├── Infrastructure/                # Frameworks, drivers, adapters
│   ├── Options/                   # JwtOptions, GoogleAuthOptions
│   ├── Persistence/               # ApplicationDbContext, EF Migrations
│   ├── Services/                  # TokenService, GoogleAuthService
│   └── DependencyInjection.cs     # Service registrations + Seed helpers
│
├── Common/                        # Cross-cutting primitives (Result<T>, Error)
│
└── Presentation/                  # API (controllers, middleware, program)
    ├── Controllers/               # AuthController (v1)
    ├── Extensions/                # Result → IActionResult conversions
    ├── Middleware/                # Global exception handling
    └── Program.cs                 # Web host & DI composition
```

**Dependency flow:**
Presentation -> Infrastructure -> Domain

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- A Google Cloud project with an **OAuth 2.0 Client ID**

---

## Setup

### 1. Clone the repository

```bash
git clone https://github.com/jawadprime/social-login-dotnet-demo.git
cd social-login-dotnet-demo
```

### 2. Configure application settings

Edit `src/Presentation/appsettings.json` (or create one there) or use **User Secrets**:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=social_login_db;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "SecretKey": "YOUR_STRONG_SECRET_KEY_MIN_32_CHARACTERS_LONG",
    "Issuer": "SocialLoginApi",
    "Audience": "SocialLoginClient",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "GoogleAuth": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com"
  }
}
```

> **Tip:** Use `dotnet user-secrets` to keep secrets out of source control:
> ```bash
> cd src/SocialLogin.Api
> dotnet user-secrets set "Jwt:SecretKey" "your_secret_key"
> dotnet user-secrets set "GoogleAuth:ClientId" "your_client_id"
> ```

### 3. Apply database migrations

The API automatically runs migrations on startup in Development mode. To apply migrations manually:

```bash
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Presentation
```

### 4. Run the API

```bash
dotnet run --project src/Presentation
```

Swagger UI: **https://localhost:{port}/swagger**

---

## Getting a Google ID Token (for Testing)

1. Create an OAuth 2.0 **Web Client ID** in [Google Cloud Console](https://console.cloud.google.com/apis/credentials)
2. Add your frontend origin to **Authorized JavaScript origins**
3. Use [Google's OAuth Playground](https://developers.google.com/oauthplayground/) or the [Token Decoder](https://jwt.io) to obtain an `id_token`
4. Pass the raw `id_token` string in the `idToken` field of the `google-login` endpoint

---

## API Reference

All responses follow the envelope format:

```json
{
  "success": true,
  "data": { ... },
  "error": null
}
```

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "Auth.InvalidGoogleToken",
    "message": "The Google token is invalid or expired.",
    "statusCode": 401
  }
}
```

### Base URL

```
/api/v1
```

---

### `POST /api/v1/auth/google-login`

Authenticate using a Google ID token. Registers the user automatically on first login.

**Request Body**
```json
{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6..."
}
```

**Response `200 OK`**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64encodedtoken...",
    "accessTokenExpiry": "2025-01-01T00:15:00Z",
    "userId": "a1b2c3d4-...",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe"
  }
}
```

**Errors:** `400 Bad Request` (missing token), `401 Unauthorized` (invalid/expired Google token)

---

### `POST /api/v1/auth/refresh-token`

Exchange a valid refresh token for a new access token and refresh token pair.

> Refresh tokens are **single-use** — each call rotates the token.

**Request Body**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64encodedtoken..."
}
```

**Response `200 OK`** — Same shape as `google-login` response.

**Errors:** `401 Unauthorized` (expired, revoked, or already-used refresh token)

---

### `POST /api/v1/auth/revoke-token`

Revoke a refresh token. Requires a valid JWT Bearer token.

**Headers**
```
Authorization: Bearer <accessToken>
```

**Request Body**
```json
{
  "refreshToken": "base64encodedtoken..."
}
```

**Response `200 OK`**
```json
{
  "success": true,
  "error": null
}
```

**Errors:** `401 Unauthorized`, `404 Not Found` (token not found or already revoked)

---

## Error Codes Reference

| Code | HTTP | Description |
|------|------|-------------|
| `Auth.InvalidGoogleToken` | 401 | Google ID token failed verification |
| `Auth.UserNotFound` | 404 | User record does not exist |
| `Auth.UserCreationFailed` | 500 | Identity failed to create the user |
| `Auth.RefreshTokenNotFound` | 404 | Refresh token does not exist or belongs to another user |
| `Auth.RefreshTokenExpired` | 401 | Refresh token has passed its expiry date |
| `Auth.RefreshTokenRevoked` | 401 | Refresh token was explicitly revoked |
| `Auth.RefreshTokenAlreadyUsed` | 401 | Single-use refresh token was already consumed |
| `Server.UnhandledException` | 500 | Unexpected server error |

---

## Project Dependencies

| Package | Purpose |
|---------|---------|
| `MediatR` | (optional) CQRS command/query dispatcher |
| `FluentValidation` | Request validation |
| `Google.Apis.Auth` | Google ID token verification |
| `Microsoft.AspNetCore.Identity` | User management & password hashing |
| `Microsoft.EntityFrameworkCore` | ORM |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | PostgreSQL EF Core provider |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT middleware |
| `Asp.Versioning.Mvc.ApiExplorer` | API versioning |
| `Swashbuckle.AspNetCore` | Swagger / OpenAPI UI |

---

## Security Considerations

- **JWT Secret Key** — must be at least 32 characters; store in environment variables or secrets manager in production
- **Refresh Token Rotation** — each use invalidates the old token (prevents replay attacks)
- **HTTPS** — enforced via `UseHttpsRedirection`
- **IP Tracking** — both token creation and revocation record the client IP
- **Role Seeding** — `User` and `Admin` roles are seeded automatically on startup

---

## Notes

- The solution in this repository separates Domain, Infrastructure, Presentation and Common layers. There is no dedicated "Application" project in the current workspace — application/service-level logic is implemented across the Infrastructure and Presentation layers.
- If you plan to evolve this into a full Clean Architecture scaffold, consider extracting use-cases, DTOs and MediatR handlers into a dedicated Application project.

## License

MIT
