# Project Documentation

## 1. Solution Goals

This platform provides multi-tenant business operations management for SMB and enterprise tenants, including:

- Customer lifecycle management
- Project and task execution (Kanban)
- Billing lifecycle (invoice to payment)
- Reporting and operational analytics
- Administrative tenant/user/role control

## 2. Layered Architecture

### Backend (ABP + .NET 10)

- `SaasSystem.Domain.Shared`: shared constants, enums, permissions contracts
- `SaasSystem.Domain`: entities and business rules
- `SaasSystem.Application.Contracts`: DTOs and app service interfaces
- `SaasSystem.Application`: application services, AutoMapper, caching
- `SaasSystem.EntityFrameworkCore`: DbContext and EF mapping
- `SaasSystem.HttpApi`: API contracts/controllers
- `SaasSystem.HttpApi.Host`: composition root, middleware, auth, health, metrics
- `SaasSystem.DbMigrator`: migration and seed startup utility

### Frontend (Angular + ABP)

- Lazy loaded feature modules per business domain
- Generated ABP proxy services only
- Reactive forms with validation
- Permission-based rendering via ABP directives
- Container/presentational split for maintainability

## 3. Domain Model (ERD Summary)

### Entities and relationships

- `Customer (1) -> (many) Project`
- `Project (1) -> (many) WorkTask`
- `Customer (1) -> (many) Invoice`
- `Project (0..1) -> (many) Invoice`
- `Invoice (1) -> (many) Payment`

### Shared fields

All tenant-scoped aggregates include:

- `TenantId`
- `CreationTime`, `CreatorId`
- `LastModificationTime`, `LastModifierId`
- `IsDeleted`, `DeletionTime`, `DeleterId`

## 4. Multi-Tenancy & Data Isolation

- ABP `IMultiTenant` is implemented on tenant-owned entities.
- Global query filtering enforces `TenantId` scoping at DbContext level.
- Host-side administration can use ABP tenant context switching when needed.

## 5. Security Model

### Authentication

- OpenIddict (ABP default)
- JWT access tokens
- Refresh token flow for SPA renewal
- Token endpoint: `/connect/token`
- ABP runtime configuration endpoint: `/api/abp/application-configuration`

### Authorization

Permission hierarchy:

- `SaasSystem.Dashboard`
- `SaasSystem.Cars`
- `SaasSystem.Users`
- `SaasSystem.Orders`
- `SaasSystem.Settings`
- `SaasSystem.Customers`, `.Create`, `.Update`, `.Delete`
- `SaasSystem.Projects`
- `SaasSystem.Tasks`, `.Move`
- `SaasSystem.Invoices`
- `SaasSystem.Payments`
- `SaasSystem.Reports`
- `SaasSystem.Administration` (host-side only)

### Route and role matrix

- `admin` role can access all application pages. `Administration` remains available only on host side.
- `user` role can access only: Dashboard, Customers, Projects, Tasks, Invoices, Payments.
- Restricted routes are filtered out from sidebar and blocked by route guards.
- API endpoints are policy-protected and return `403` when the caller lacks permission.

| Route | Policy |
|---|---|
| `/dashboard` | `SaasSystem.Dashboard` |
| `/cars` | `SaasSystem.Cars` |
| `/car-details/:id` | `SaasSystem.Cars` |
| `/users` | `SaasSystem.Users` |
| `/orders` | `SaasSystem.Orders` |
| `/settings` | `SaasSystem.Settings` |
| `/customers` | `SaasSystem.Customers` |
| `/projects` | `SaasSystem.Projects` |
| `/tasks` | `SaasSystem.Tasks` |
| `/invoices` | `SaasSystem.Invoices` |
| `/payments` | `SaasSystem.Payments` |
| `/reports` | `SaasSystem.Reports` (admin-only) |
| `/administration` | `SaasSystem.Administration` (host-only) |

### Reports exports

- Invoice report preview endpoint: `GET /api/app/reports/invoices`
- Excel export endpoint: `GET /api/app/reports/invoices-excel`
- PDF export endpoint: `GET /api/app/reports/invoices-pdf`
- Filters supported: `fromDate`, `toDate`, `customerId`, `status`
- Export generation stack:
  - Excel: MiniExcel
  - PDF: QuestPDF (`LicenseType.Community`)

### Default local seed accounts

- Environment variable `SAASSYSTEM_DEFAULT_PASSWORD` is required for startup seeding.
- Seeded users:
  - `admin`
  - `user`
- Both accounts use the password from `SAASSYSTEM_DEFAULT_PASSWORD`.

### API hardening

- DTO-based boundaries (no entity exposure)
- Data annotations on request DTOs
- EF Core parameterized queries (SQL injection-safe path)
- HTTPS redirection
- Rate limiting middleware
- Secure headers:
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`

## 6. Caching Strategy

Dashboard aggregates are cached per tenant.

Cache key convention:

```text
dashboard:stats:{tenantId}
```

Invalidation points:

- customer create/update/delete
- task status movement
- invoice/payment mutations

## 7. Testing Strategy

### Backend

- Unit tests: xUnit + Moq + FluentAssertions
- Integration tests: `WebApplicationFactory` + in-memory DB
- Service-level verification for CRUD/filtering/authorization

### Frontend

- Unit/component tests: Jasmine + Karma
- Service tests: mocked generated proxies + HttpClient testing for generated customer proxy
- e2e smoke: Playwright
  - login
  - create customer
  - dashboard visible

Coverage gate:

- Minimum 70% for statements, branches, functions, and lines in CI

Notes:

- Playwright smoke uses route-level API mocks for dashboard/customers endpoints to keep e2e deterministic without external backend dependency.

## 8. CI/CD Standards

Pipeline stages:

1. Restore (`dotnet restore`, `npm install`)
2. Build (`dotnet build`, `npm run build`)
3. Test + coverage reports
4. SonarQube quality scan
5. Vulnerability checks (`npm audit`, `dotnet list package --vulnerable`)
6. Docker build/push
7. Helm deploy (`helm upgrade --install`)

## 9. Deployment Topology

### Docker Compose (local)

- API container
- Angular container
- SQL Server container

### Kubernetes namespace: `saas-system`

- Deployments: `api`, `angular`
- StatefulSet: SQL Server + PVC
- ConfigMap: app settings
- Secret: connection strings and app secrets
- HPA: API autoscaling
- Ingress: frontend route to angular service

## 10. Observability

### Metrics

- ASP.NET Core metrics endpoint: `/metrics`
- Prometheus scrape for request and runtime signals
- Pod CPU/memory from Kubernetes metrics

### Dashboards (Grafana)

- API latency
- request rate
- error rate
- pod resource usage

### Logging

- Serilog structured logs
- correlation id enrichment per request

## 11. SQL Server Configuration

Example connection string for local machine:

```text
Server=DESKTOP-OKFV1DJ;Database=SaasSystem;Trusted_Connection=True;TrustServerCertificate=True
```

Container alternative:

```text
Server=sqlserver,1433;Database=SaasSystem;User Id=sa;Password=<from-secret>;TrustServerCertificate=True
```

## 12. Operational Notes

- Keep secrets in environment variables or Kubernetes Secrets.
- Set `SAASSYSTEM_DEFAULT_PASSWORD` before running host or db migrator so identity seed can create default users.
- Domain seed creates a large dataset with 5 years of invoices/payments/projects/tasks for realistic reporting tests.
- Run migrations via CI/CD job before app rollout.
- Use blue/green or rolling update for API deployment.
- Keep Helm values environment-specific (`dev`, `staging`, `prod`).

## 13. Operational References

- `docs/OPERATIONS_RUNBOOK_AR.md` - Arabic operational runbook (build, run, test, deploy, CI parity).
- `docs/TROUBLESHOOTING_AR.md` - Arabic troubleshooting guide (actual incidents, root causes, fixes, prevention).
