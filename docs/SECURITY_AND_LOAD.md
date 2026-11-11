# Security, frontend integration and load profile

Asterion authenticates API requests using JWT signature, issuer, audience and
expiry verification. `tickets.read` can view lists/audits, while
`tickets.write` is required to create and change ticket status. The audit
actor is extracted from the verified token `sub` claim, not from untrusted
form fields.

Configure `Auth__SigningKey` with at least 32 random bytes on the API
container (and optionally `Auth__Issuer`, `Auth__Audience`). For local
testing, obtain a short-lived JWT from your test issuer and paste it into the
Workflow Console access-token field. The Angular component keeps it only
in memory and never puts it in the compiled bundle or localStorage.
The API cannot issue tokens.

Production deployments should use OIDC authorization-code flow with PKCE,
asymmetric JWKS validation, HTTPS, a token refresh strategy, EF migrations,
centralized authorization and CSP. The local signing scheme is a portfolio demo.

Run dotnet tests and Angular remote/shell builds in GitHub Actions.
The k6 profile can be run against a disposable environment:

```sh
JWT="<short-lived-token>" BASE_URL=http://localhost:5080 k6 run load/ticket-reads.js
```

10 VUs for 30 seconds with p95 under 800 ms is a test target, not a
measured performance result. Report actual throughput and errors separately.
