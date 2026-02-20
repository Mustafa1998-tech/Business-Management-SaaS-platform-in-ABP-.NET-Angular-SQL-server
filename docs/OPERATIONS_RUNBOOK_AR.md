# دليل التشغيل التنفيذي للمنصة (Runbook)

## 1) نطاق الدليل

### الهدف
هذا الدليل يوفّر مسار تشغيل عملي من الصفر حتى البناء والاختبار والنشر لمنصة **Business Management SaaS**.

### الجمهور المستهدف
- مطور Backend
- مطور Frontend
- مهندس DevOps
- QA Engineer

### يشمل
- أوامر التشغيل المحلي
- أوامر قاعدة البيانات والترحيلات
- أوامر الجودة والاختبارات
- أوامر Docker/Kubernetes/Helm
- مطابقة خطوات CI محليًا

### لا يشمل
- شرح معماري تفصيلي عميق (موجود في `docs/PROJECT_DOCUMENTATION.md`)
- سياسات الأمن المؤسسية خارج نطاق المشروع

## 2) المتطلبات المسبقة

- .NET SDK 10
- Node.js 22+
- SQL Server (محلي أو Container)
- Docker Desktop
- Kubernetes (Docker Desktop)
- Helm 3
- Kubectl

## 3) مسارات التنفيذ المعتمدة

- جذر المشروع: `c:\Users\Mustafa\Desktop\Business Management SaaS platform`
- Backend: `backend/`
- Frontend: `frontend/`
- DevOps: `devops/`

> كل الأوامر أدناه تفترض التنفيذ من جذر المشروع ما لم يُذكر غير ذلك.

## 4) إعداد البيئة (نمط مختلط للبيانات الحساسة)

استخدم Placeholder آمن في الأوامر. توجد قيم محلية حاليًا في ملفات الإعداد (`appsettings*.json`) لأغراض التطوير المحلي.

### متغيرات بيئة أساسية (PowerShell)

```powershell
$env:ConnectionStrings__Default = "Server=<SQL_SERVER>;Database=<DB_NAME>;Trusted_Connection=True;TrustServerCertificate=True"
$env:OpenIddict__Applications__Angular__ClientSecret = "<OPENIDDICT_CLIENT_SECRET>"
$env:SA_PASSWORD = "<STRONG_SQL_PASSWORD>"
```

### أماكن الإعدادات في المشروع

- `backend/src/SaasSystem.HttpApi.Host/appsettings.json`
- `backend/src/SaasSystem.HttpApi.Host/appsettings.Development.json`
- `devops/k8s/base/secret.yaml`
- `devops/helm-chart/values.yaml`

## 5) أوامر التطوير المحلي (ترتيب إلزامي)

### 5.1 تشغيل Backend

```powershell
cd backend
dotnet restore SaasSystem.slnx
dotnet build SaasSystem.slnx -c Debug
cd src/SaasSystem.HttpApi.Host
dotnet run
```

### 5.2 تشغيل Frontend

```powershell
cd frontend
npm install
npm start
```

### 5.3 تشغيل DbMigrator (تطبيق Migration + Seed)

```powershell
cd backend/src/SaasSystem.DbMigrator
dotnet run
```

## 6) أوامر قاعدة البيانات

### إنشاء Migration جديدة

```powershell
cd backend/src/SaasSystem.EntityFrameworkCore
dotnet ef migrations add <MigrationName> --startup-project ../SaasSystem.HttpApi.Host
```

### تطبيق Migration على قاعدة البيانات

```powershell
dotnet ef database update --startup-project ../SaasSystem.HttpApi.Host
```

### متى أستخدم EF CLI ومتى أستخدم DbMigrator؟

- استخدم **EF CLI** عند تطوير Migration جديدة.
- استخدم **DbMigrator** في التشغيل الفعلي أو قبل الإطلاق لتطبيق كل Migration وتشغيل Seed بشكل موحّد.

## 7) أوامر الجودة والاختبارات

### Backend

```powershell
cd backend
dotnet format SaasSystem.slnx --verify-no-changes
dotnet test SaasSystem.slnx -c Debug
```

### Frontend

```powershell
cd frontend
npm run lint
npm run test -- --watch=false --browsers=ChromeHeadless
npm run e2e
```

## 8) أوامر Docker محليًا

### تشغيل الخدمات

```powershell
$env:SA_PASSWORD = "<STRONG_SQL_PASSWORD>"
docker compose -f devops/docker-compose.yml up --build -d
```

### التحقق

```powershell
docker compose -f devops/docker-compose.yml ps
docker compose -f devops/docker-compose.yml logs api
docker compose -f devops/docker-compose.yml logs angular
docker compose -f devops/docker-compose.yml logs sqlserver
```

### الإيقاف والتنظيف

```powershell
docker compose -f devops/docker-compose.yml down
docker compose -f devops/docker-compose.yml down -v
```

## 9) أوامر Kubernetes (Base Manifests)

### التطبيق

```powershell
kubectl apply -f devops/k8s/base/namespace.yaml
kubectl apply -f devops/k8s/base
```

### فحوصات الصحة

```powershell
kubectl get pods -n saas-system
kubectl get svc -n saas-system
kubectl describe pod <pod-name> -n saas-system
kubectl logs deployment/api -n saas-system
kubectl logs deployment/angular -n saas-system
```

## 10) أوامر Helm

### نشر/تحديث

```powershell
helm upgrade --install saas ./devops/helm-chart --namespace saas-system --create-namespace
```

### التحقق والتاريخ

```powershell
helm status saas -n saas-system
helm history saas -n saas-system
```

### Rollback

```powershell
helm rollback saas <REVISION> -n saas-system
```

## 11) CI Parity محليًا (مطابق للـ Workflow)

## المرحلة 1: Restore

```powershell
dotnet restore backend/SaasSystem.slnx
cd frontend
npm install
cd ..
```

## المرحلة 2: Build

```powershell
dotnet build backend/SaasSystem.slnx -c Release --no-restore
cd frontend
npm ci
npm run build
cd ..
```

## المرحلة 3: Test

```powershell
dotnet format backend/SaasSystem.slnx --verify-no-changes
dotnet test backend/SaasSystem.slnx -c Release --collect:"XPlat Code Coverage" --logger trx
cd frontend
npm ci
npm run lint
npm run test
npx playwright install --with-deps chromium
npm run e2e
cd ..
```

## المرحلة 4: Code Quality (محلي اختياري)

> محليًا يعتمد على توفر SonarQube server/token في بيئتك.

## المرحلة 5: Security Scan

```powershell
dotnet list backend/SaasSystem.slnx package --vulnerable
cd frontend
npm audit --audit-level=high
cd ..
```

## المرحلة 6: Docker

```powershell
docker build -f devops/docker/Dockerfile.api -t <docker-user>/saas-api:latest .
docker build -f devops/docker/Dockerfile.angular -t <docker-user>/saas-angular:latest .
```

## المرحلة 7: Deploy

```powershell
helm upgrade --install saas ./devops/helm-chart --namespace saas-system --create-namespace
```

## 12) جدول الأوامر الموحّد

| الأمر | الهدف | مكان التنفيذ | مثال | مؤشر النجاح | فشل شائع سريع |
|---|---|---|---|---|---|
| `dotnet restore backend/SaasSystem.slnx` | استرجاع حزم backend | جذر المشروع | `dotnet restore backend/SaasSystem.slnx` | بدون Errors | مشكلة NuGet feed أو network |
| `dotnet build backend/SaasSystem.slnx -c Debug` | بناء backend | جذر المشروع | `dotnet build backend/SaasSystem.slnx -c Debug` | Build succeeded | أخطاء compile أو missing reference |
| `dotnet run` (Host) | تشغيل API | `backend/src/SaasSystem.HttpApi.Host` | `dotnet run` | ظهور listening URL | connection string أو port مشغول |
| `dotnet run` (DbMigrator) | تطبيق migration + seed | `backend/src/SaasSystem.DbMigrator` | `dotnet run` | رسالة `Database migration and seed completed.` | قاعدة غير متاحة أو صلاحيات SQL |
| `dotnet ef migrations add` | إنشاء migration | `backend/src/SaasSystem.EntityFrameworkCore` | `dotnet ef migrations add AddInvoiceIndexes --startup-project ../SaasSystem.HttpApi.Host` | إنشاء ملف Migration جديد | startup project خاطئ |
| `dotnet ef database update` | تحديث DB schema | `backend/src/SaasSystem.EntityFrameworkCore` | `dotnet ef database update --startup-project ../SaasSystem.HttpApi.Host` | Database updated | connection string مفقود |
| `dotnet format --verify-no-changes` | تحقق تنسيق C# | `backend` | `dotnet format SaasSystem.slnx --verify-no-changes` | Exit code 0 | تعديلات تنسيق مطلوبة |
| `dotnet test` | اختبارات backend | `backend` | `dotnet test SaasSystem.slnx -c Debug` | كل الاختبارات Passed | failing tests أو in-memory config |
| `npm install` | تثبيت حزم frontend (dev) | `frontend` | `npm install` | Installation complete | peer dependency warnings/errors |
| `npm ci` | تثبيت مطابق lockfile | `frontend` | `npm ci` | Installation complete | lockfile mismatch |
| `npm start` | تشغيل Angular dev server | `frontend` | `npm start` | Angular served on port | port مشغول |
| `npm run lint` | فحص جودة frontend | `frontend` | `npm run lint` | All files pass linting | lint rules أو config ناقص |
| `npm run test -- --watch=false --browsers=ChromeHeadless` | Unit tests frontend | `frontend` | `npm run test -- --watch=false --browsers=ChromeHeadless` | TOTAL: SUCCESS | ChromeHeadless unavailable |
| `npm run e2e` | e2e smoke | `frontend` | `npm run e2e` | `1 passed` | Playwright browser missing |
| `docker compose ... up --build -d` | تشغيل stack محلي | جذر المشروع | `docker compose -f devops/docker-compose.yml up --build -d` | كل الخدمات Up | SA_PASSWORD أو build context |
| `kubectl apply -f devops/k8s/base` | نشر manifests الأساسية | جذر المشروع | `kubectl apply -f devops/k8s/base` | resources configured | namespace/secret/storage issues |
| `helm upgrade --install ...` | نشر عبر Helm | جذر المشروع | `helm upgrade --install saas ./devops/helm-chart --namespace saas-system --create-namespace` | release deployed | values غير صحيحة أو image pull |

## 13) Smoke Verification سريع بعد أي تغيير كبير

```powershell
# Backend
cd backend
dotnet format SaasSystem.slnx --verify-no-changes
dotnet test SaasSystem.slnx -c Debug

# Frontend
cd ../frontend
npm run lint
npm run test -- --watch=false --browsers=ChromeHeadless
npm run e2e
```
