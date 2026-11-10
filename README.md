# Asterion Ops — Microfrontend Workflow Control Center
**Target role:** Remote Angular / .NET full-stack engineer (Angular 20, TypeScript, native federation, ASP.NET Core, EF Core, SQL Server).

A split-deployment workflow control center with a **dynamic Angular shell**, a separately built **workflow remote**, and a versioned **.NET REST API** backed by SQL Server. Operators open, assign, complete and audit review tickets without shipping the entire UI as a single bundle.

## Architecture
```text
Angular Shell (4200) ---- native federation ---> Workflow Remote (4201)
                                             |         |
                                             | HttpClient
                                             v
                              ASP.NET Core 10 REST API (5080)
                                             |
                                    EF Core / SQL Server
```

- Frontend: standalone Angular components, signals, lazy remote loading, typed HTTP client, reactive forms, and shell-level routing.
- Backend: state transitions, validation, pagination, optimistic concurrency via rowversion, append-only audit events, ProblemDetails responses, EF Core.
- Developer tooling: Docker Compose for SQL Server and API, xUnit unit tests, CI for .NET and Angular.

## Start the API
1. Install Docker and .NET 10 SDK.
2. Copy `.env.example` to `.env`; change `MSSQL_SA_PASSWORD` to a strong local value.
3. Run `docker compose --env-file .env up --build -d`.
4. Visit `http://localhost:5080/health`. On first start the demo database is created via EF `EnsureCreated()`; use migrations before production.

## Start both microfrontends
Install Node 22+ and run:
```bash
cd frontend
npm install
npm run start:remote # separate terminal; port 4201
npm run start:shell  # separate terminal; port 4200
```
Open `http://localhost:4200/workflows`. The shell fetches `federation.manifest.json` and dynamically loads `./Component` from the remote.

## REST contract
| Method | Endpoint | Notes |
|---|---|---|
| GET | `/health` | Readiness |
| GET | `/api/v1/tickets?page=1&pageSize=20` | Paginated list |
| POST | `/api/v1/tickets` | Ticket creation + first audit entry |
| PATCH | `/api/v1/tickets/{id}/status` | Optimistic version check + audit entry |
| GET | `/api/v1/tickets/{id}/history` | Chronological audit entries |

Example:
```bash
curl -X POST http://localhost:5080/api/v1/tickets -H 'Content-Type: application/json' \
 -d '{"title":"Investigate settlement anomaly","owner":"operations"}'
```
Use `GET /api/v1/tickets` to obtain the version token before PATCH.

## Verification
```bash
dotnet test backend/Asterion.Tests/Asterion.Tests.csproj
cd frontend && npm ci && npm run build:remote && npm run build:shell
```
CI builds independently. Tests target state transitions; a full end-to-end browser + SQL Server exercise remains a deploy-time check.

## Demonstrates
Angular 17+ / microfrontends / TypeScript / .NET / REST / EF Core / SQL Server / Docker / CI. Jira is a delivery tool, not an app feature; project tasks are documented in `docs/BACKLOG.md`.

**Portfolio code, not a production deployment.** Authentication, authorization, TLS, migrations, and observability require environment-specific hardening.
