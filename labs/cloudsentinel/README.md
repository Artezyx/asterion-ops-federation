# CloudSentinel — cloud reliability laboratory

Independent infrastructure and reliability lab for the Asterion API. The application is unchanged.

- **Kubernetes**: rolling deployment, health probes, resources, disruption budget and secret reference.
- **Terraform**: minimal Azure resource group demonstrating provider pinning, input variables and tags.
- **Prometheus**: sample PromQL alert rules requiring consistent OpenTelemetry/ASP.NET metric names.
- **Python SLO monitor**: functioning percentile/error-budget evaluator with tests and a CLI.

## Run the checks

From `labs/cloudsentinel/monitor`:

```bash
python -m unittest discover -p 'test_*.py' -v
python slo.py --input sample.csv
```

## Deployment prerequisites and limitations

This is a **reference deployment configuration**, not a live Azure installation.
The container image placeholder must be replaced with a real published image.
The database connection secret must be provisioned securely before deployment.
Verify the application's actual container port, health route and filesystem permissions.
Before applying Terraform, supply an Azure subscription and choose a supported Azure
region; the default is only an example. AKS is deliberately **not** provisioned to avoid
accidental cloud expenses. Kubernetes manifests have not been cluster-tested.
Prometheus alert expressions require production metric label verification.

To validate these pieces in a real environment, use `terraform fmt -check`,
`terraform validate`, `kubectl apply --dry-run=client`, then staging deployment
and controlled rollback. No claims of production readiness or Azure credentials.
