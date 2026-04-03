# Property Management System (PMS)

Scalable property management backend with lease contracts, JWT authentication, per-user webhook subscriptions, and RabbitMQ-driven webhook delivery.

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download) (or the SDK matching `TargetFramework` in the `.csproj` files)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (recommended) for PostgreSQL and RabbitMQ
- Optional: [EF Core tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) for migrations (`dotnet ef`)

## Repository layout

| Path | Description |
|------|-------------|
| `src/PMS.Domain` | Domain entities |
| `src/PMS.Application` | CQRS commands/queries, MediatR |
| `src/PMS.Infrastructure` | EF Core (PostgreSQL), repositories, JWT, RabbitMQ publisher |
| `src/PMS.Api` | HTTP API (controllers) |
| `src/PMS.Worker` | RabbitMQ consumer → HTTP webhooks |
| `src/PMS.Tests` | xUnit tests (domain + application) |
| `docs/` | Requirements, design, PR notes |
| `docker-compose.yml` | PostgreSQL, RabbitMQ, and **PMS.Api** (HTTP `8080`) |
| `Dockerfile` | Multi-stage build for `PMS.Api` |

## Quick start

### 1. Start infrastructure (and optional API in Docker)

From the repository root:

```bash
docker compose up -d
```

This builds and starts **PMS.Api** together with:

- **PostgreSQL** on port `5432` (user `postgres`, password `postgres`, database `pms`)
- **RabbitMQ** on `5672` (AMQP) and **management UI** on [http://localhost:15672](http://localhost:15672) (default user/password: `guest` / `guest`)
- **API** on [http://localhost:8080](http://localhost:8080) (HTTP inside the container; overrides in `docker-compose.yml` set `ConnectionStrings:Database` and `RabbitMq:HostName` for the Compose network)

To build only the API image: `docker build -t pms-api .`

To run **only Postgres + RabbitMQ** (e.g. local `dotnet run` for the API): `docker compose up -d postgres rabbitmq`.

### 2. Configure the API

Edit `src/PMS.Api/appsettings.json` (or use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) / environment variables in production):

- **`ConnectionStrings:Database`** — must match your Postgres host, database name, and password. For the default `docker-compose.yml` above, use password `postgres`.
- **`Jwt:Secret`** — set a long random string (at least 32 characters). Tokens will not validate if this is left as a placeholder in shared environments.

`src/PMS.Worker/appsettings.json` — use the **same** `ConnectionStrings:Database` and **`RabbitMq`** settings as the API so the worker reads subscriptions and connects to the same broker.

### 3. Apply database migrations

**Option A — Development (automatic)**  
In **Development**, `PMS.Api` applies pending EF Core migrations on startup. Run the API once (step 4) and the schema is created.

**Option B — Manual**

```bash
dotnet ef database update --project src/PMS.Infrastructure/PMS.Infrastructure.csproj --startup-project src/PMS.Api/PMS.Api.csproj
```

### 4. Run the API

```bash
dotnet run --project src/PMS.Api/PMS.Api.csproj
```

Default HTTP URL: **http://localhost:5005** (see `src/PMS.Api/Properties/launchSettings.json`).

OpenAPI: in Development, OpenAPI is mapped; check your ASP.NET Core version for the exact endpoint (e.g. `/openapi/v1.json`).

### 5. Run the worker (webhook delivery)

Webhook delivery runs in a **separate process**. In another terminal:

```bash
dotnet run --project src/PMS.Worker/PMS.Worker.csproj
```

Keep the worker running while you test lease events.

### 6. Try the flow

Use `src/PMS.Api/PMS.Api.http` in VS / VS Code, or any HTTP client:

1. **Register** — `POST /api/v1/auth/register` (creates tenant + user)
2. **Login** — `POST /api/v1/auth/login` → copy the `accessToken`
3. **Register webhook** — `POST /api/v1/webhook-subscriptions` with `Authorization: Bearer <token>` (e.g. `https://webhook.site/...` URL)
4. **Create lease** — `POST /api/v1/lease-contracts` with the same Bearer token

The worker should POST the integration event to the registered URL(s).

## Build and test

```bash
dotnet build PMS.slnx
dotnet test PMS.slnx
```

Tests live in `src/PMS.Tests` (xUnit, Moq, FluentAssertions). Run `dotnet test` from the repo root or pass `--filter FullyQualifiedName~PMS.Tests` to scope runs.

## Configuration reference

| Area | Where |
|------|--------|
| PostgreSQL | `ConnectionStrings:Database` in API and Worker |
| JWT | `Jwt` section in `PMS.Api` |
| RabbitMQ | `RabbitMq` in API (publisher) and Worker (consumer) — **exchange name must match** (`ExchangeName`, default `pms.integration`) |
| Webhook retries | `WebhookDelivery` in `PMS.Worker` (`MaxAttempts`, delays, jitter) |
| Logging (Serilog) | `Serilog` in both hosts — console + rolling files under `logs/` (`pms-api-*.log`, `pms-worker-*.log`); tune `MinimumLevel` and overrides per namespace |

## Documentation

- [docs/requirement.md](docs/requirement.md)
- [docs/system-design.md](docs/system-design.md)

## Troubleshooting

- **Worker not delivering** — Ensure RabbitMQ is up, `DispatchConsumersAsync` is enabled in code (already set), and both API and Worker use the same `RabbitMq:ExchangeName` and broker URL. The worker retries connecting to RabbitMQ on startup.
- **401 / invalid token** — Confirm `Jwt:Secret` matches between token issuance and validation and that the `Authorization: Bearer` header is sent.
- **Database connection errors** — Confirm Postgres is listening, `ConnectionStrings:Database` matches the Docker credentials, and the `pms` database exists (created by `docker-compose`).
