# Asterion Ops — Workflow Control Center

Portfolio backend foundation for **C# / ASP.NET Core 10** positions and eventual Angular microfrontend integration.

## Implemented
- Ticket creation/listing REST API
- Version-checked status changes to reject stale updates (HTTP 409)
- OpenAPI document endpoint and CORS policy
- Automated build workflow

## Run
```bash
dotnet run --project backend/Asterion.Api
```
`GET /health` checks the server; `GET /api/tickets` lists records.

**Scope:** The current GitHub version is a runnable in-memory API foundation. The planned Angular shell, federated workflow microfrontend, SQL Server storage, and full automated tests are not yet uploaded.
