# Business Management SaaS Platform

Enterprise multi-tenant SaaS platform for managing customers, projects, tasks, invoices, payments, and operational reporting.

## Tech Stack

- Backend: .NET 10, ABP Framework, EF Core, SQL Server, OpenIddict
- Frontend: Angular (ABP UI), generated proxy services
- Data: SQL Server with tenant-aware schema and audit/soft-delete support
- DevOps: Docker, Kubernetes, Helm, GitHub Actions, SonarQube
- Observability: Serilog, Prometheus, Grafana

## High-Level Architecture

```text
[Angular ABP UI]
      |
      v
[API Gateway / Ingress]
      |
      v
[ABP HttpApi.Host]
  |        |        |
  |        |        +--> OpenIddict (JWT + Refresh Token)
  |        +-----------> Application Layer (DTOs, Permissions, Services)
  +--------------------> Domain + EF Core + SQL Server (Multi-tenant)

Cross-cutting:
- Distributed cache for dashboard stats
- Serilog structured logs + correlation id
- Health checks + metrics (/health, /metrics)
- Rate limiting + secure headers
```

## Core Modules

- Dashboard
- Customers
- Projects
- Tasks (Kanban)
- Invoices
- Payments
- Reports
- Administration (Users, Roles, Tenants)

## Repository Layout

```text
backend/
  src/
  test/
frontend/
devops/
  docker/
  k8s/
  helm-chart/
docs/
```

## Quick Start

### 1. Prerequisites

- .NET SDK 10
- Node.js 22+
- SQL Server (local or container)
- Docker Desktop + Kubernetes (optional for containerized run)

### 2. Environment Variables

Set secrets through `.env` (not in code):

```powershell
Copy-Item .env.example .env
```

Then edit `.env` and set at least:

- `ConnectionStrings__Default`
- `SAASSYSTEM_DEFAULT_PASSWORD`

`SAASSYSTEM_DEFAULT_PASSWORD` is required for first-time user creation, and recommended for local development because startup seeding aligns existing `admin`/`user` passwords to this value.

### 3. Run Backend

```powershell
cd backend
# restore/build when all source files are in place
dotnet restore
dotnet build
# run host project
cd src/SaasSystem.HttpApi.Host
dotnet run
```

`dotnet run` now auto-loads `.env` from the current/parent directories.

### 4. Run Frontend

```powershell
cd frontend
npm install
npm start
```

## Database

- Provider: SQL Server
- Multi-tenancy: TenantId-aware global query filters
- Soft delete and audit fields: ABP audited aggregate roots
- Recommended indexes: TenantId, foreign keys, and reporting keys

Migration command:

```powershell
cd backend/src/SaasSystem.EntityFrameworkCore
dotnet ef migrations add InitialCreate --startup-project ../SaasSystem.HttpApi.Host
dotnet ef database update --startup-project ../SaasSystem.HttpApi.Host
```

## Quality Gates

- Backend tests: xUnit + Moq + FluentAssertions
- Frontend tests: Jasmine/Karma + Playwright e2e
- Coverage threshold: 70% minimum for statements, branches, functions, and lines
- Linting: `dotnet format` and Angular lint
- PR checklist + conventional commits enforced in CI

Test commands:

```powershell
# backend
cd backend
dotnet format SaasSystem.slnx --verify-no-changes
dotnet test SaasSystem.slnx -c Debug

# frontend
cd ../frontend
npm run lint
npm run test -- --watch=false --browsers=ChromeHeadless
npm run e2e
```

Note: e2e smoke tests use Playwright route-level API mocks to keep frontend smoke deterministic in local and CI runs.

## Security Baseline

- ABP permission policies for each module
- DTO validation with data annotations
- Route guards and permission-based UI controls
- HTTPS redirection, rate limiting, secure response headers
- Secrets via env vars and Kubernetes Secrets

## Role-Based Page Separation

The app now enforces ABP policy checks in both UI and API:

- Admin (`admin` role): full business pages and host-side administration.
- User (`user` role): `Dashboard`, `Customers`, `Projects`, `Tasks`, `Invoices`, `Payments`.
- Restricted pages are hidden from sidebar for unauthorized users.
- Direct navigation to restricted URLs is blocked by guards.
- Direct API access without policy returns `403 Forbidden` (or `401 Unauthorized` when not authenticated).

Policies mapped to routes:

- `SaasSystem.Dashboard` -> `/dashboard`
- `SaasSystem.Cars` -> `/cars`, `/car-details/:id`
- `SaasSystem.Users` -> `/users`
- `SaasSystem.Orders` -> `/orders`
- `SaasSystem.Settings` -> `/settings`
- `SaasSystem.Customers` -> `/customers`
- `SaasSystem.Projects` -> `/projects`
- `SaasSystem.Tasks` -> `/tasks`
- `SaasSystem.Invoices` -> `/invoices`
- `SaasSystem.Payments` -> `/payments`
- `SaasSystem.Reports` -> `/reports` (admin-only)
- `SaasSystem.Administration` (host-only) -> `/administration`

Reports exports:

- `GET /api/app/reports/invoices` (preview with filters)
- `GET /api/app/reports/invoices-excel` (professional Excel export)
- `GET /api/app/reports/invoices-pdf` (professional PDF export)

Seed profile:

- Domain seeding generates large historical data across 5 years to emulate a long-running SaaS production dataset.

## Local Login

- Login page: `http://localhost:4200/login`
- OAuth client id: `SaasSystem_Angular`
- Token endpoint: `POST /connect/token`
- ABP app config endpoint: `GET /api/abp/application-configuration`

Default local users after seeding (same password from `SAASSYSTEM_DEFAULT_PASSWORD`):

- `admin` / `<SAASSYSTEM_DEFAULT_PASSWORD>`
- `user` / `<SAASSYSTEM_DEFAULT_PASSWORD>`

## Deployment

- Local containers: `devops/docker-compose.yml` (run with `docker compose --env-file .env -f devops/docker-compose.yml up --build -d`)
- Kubernetes manifests: `devops/k8s/base`
- Helm chart: `devops/helm-chart`
- CI/CD pipeline: `.github/workflows/ci-cd.yml`

## Documentation

Detailed technical documentation is available in:

- `docs/PROJECT_DOCUMENTATION.md`
