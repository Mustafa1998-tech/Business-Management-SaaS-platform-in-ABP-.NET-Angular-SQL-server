# دليل استكشاف الأخطاء وإصلاحها

## 1) طريقة قراءة أي مشكلة

استخدم النموذج التالي لكل مشكلة:

- **الأعراض**: ما الذي تراه فعليًا (رسالة خطأ، فشل اختبار، خدمة لا تعمل).
- **السبب الجذري**: لماذا حصلت المشكلة.
- **الحل الفوري**: خطوات سريعة لإرجاع النظام لحالة عمل.
- **الوقاية**: إجراء يمنع تكرار المشكلة.

---

## 2) أخطاء حصلت فعليًا في هذا المشروع

## الحالة 1: Playwright e2e كان يفتح تطبيقًا آخر

- **الأعراض**
  - اختبار e2e يفشل في العثور على عناصر المشروع.
  - Snapshot يظهر شاشة تطبيق مختلف.
- **السبب الجذري**
  - `reuseExistingServer` كان يسمح باستخدام سيرفر قديم على نفس المنفذ.
  - تعارض منفذ `4200` مع تطبيق آخر مفتوح.
- **الملفات المتأثرة**
  - `frontend/playwright.config.ts`
- **ما تغيّر**
  - `baseURL` أصبح `http://127.0.0.1:4301`
  - `webServer.port` أصبح `4301`
  - `reuseExistingServer` أصبح `false`
  - `command` أصبح `npx ng serve --host 127.0.0.1 --port 4301`
- **لماذا نجح الحل**
  - صار اختبار e2e يشغل سيرفرًا معزولًا ومضمونًا بدل reuse لسيرفر غير معروف.
- **الوقاية**
  - خصص منفذ ثابت منفصل للاختبارات.
  - لا تستخدم `reuseExistingServer` في smoke e2e.

## الحالة 2: فشل e2e بعد الحفظ بسبب عدم حتمية API

- **الأعراض**
  - الاختبار يفشل عند التحقق من ظهور العميل بعد `Save`.
- **السبب الجذري**
  - الاختبار كان يعتمد على Backend/بيانات فعلية غير ثابتة.
- **الملفات المتأثرة**
  - `frontend/e2e/app.e2e.spec.ts`
- **ما تغيّر**
  - إضافة route-level mocks لواجهات:
    - `GET /api/app/dashboard/stats`
    - `GET /api/app/customers`
    - `POST /api/app/customers`
- **لماذا نجح الحل**
  - الاختبار صار deterministic ويعمل بنفس النتيجة في أي بيئة.
- **الوقاية**
  - اعزل smoke e2e عن الاعتماد على بيانات خارجية متغيرة.

## الحالة 3: فشل `npm run lint` لعدم وجود lint target

- **الأعراض**
  - `Cannot find "lint" target for the specified project.`
- **السبب الجذري**
  - مشروع Angular لم يكن يحتوي target lint في `angular.json`.
- **الملفات المتأثرة**
  - `frontend/angular.json`
  - `frontend/package.json`
  - `frontend/.eslintrc.json`
- **ما تغيّر**
  - إضافة إعداد Angular ESLint Builder.
  - تأكيد سكربت `npm run lint`.
- **لماذا نجح الحل**
  - أصبح lint خطوة رسمية قابلة للتشغيل محليًا وداخل CI.
- **الوقاية**
  - أي مشروع جديد يجب أن يتضمن lint target من البداية.

## الحالة 4: ظهور عدد كبير من أخطاء ESLint بعد تفعيل angular-eslint

- **الأعراض**
  - عشرات الأخطاء المرتبطة بقواعد:
    - `prefer-standalone`
    - `prefer-inject`
    - `prefer-control-flow`
- **السبب الجذري**
  - القواعد الافتراضية كانت صارمة وغير مناسبة للبنية الحالية المعتمدة على NgModules.
- **الملفات المتأثرة**
  - `frontend/.eslintrc.json`
  - `frontend/src/app/app.module.ts`
- **ما تغيّر**
  - تعطيل القواعد غير المناسبة للبنية الحالية.
  - إصلاح خطأ فعلي (`unused import`).
- **لماذا نجح الحل**
  - أصبحت قواعد lint عملية وتحافظ على الجودة بدون فرض migration معمارية غير مخطط لها.
- **الوقاية**
  - اضبط lint rules حسب architecture المعتمدة بدل استخدام preset افتراضي كما هو.

## الحالة 5: فشل Build في DbMigrator

- **الأعراض**
  - أخطاء مثل:
    - `CS0234` namespace not found
    - `CS0246` type/module not found
    - `CS8417` await using غير مدعوم على النوع
- **السبب الجذري**
  - مرجع مشروع ناقص لـ Application.
  - `using` namespace غير صحيح لـ DbContext.
  - استخدام `await using` مع نوع لا يدعم `IAsyncDisposable`.
- **الملفات المتأثرة**
  - `backend/src/SaasSystem.DbMigrator/SaasSystem.DbMigrator.csproj`
  - `backend/src/SaasSystem.DbMigrator/Program.cs`
- **ما تغيّر**
  - إضافة ProjectReference إلى `SaasSystem.Application`.
  - استخدام namespace الصحيح `SaasSystem.EntityFrameworkCore.EntityFrameworkCore`.
  - استبدال `await using` بـ `using`.
- **لماذا نجح الحل**
  - تم حل الاعتمادات والنطاقات البرمجية وأصبح DbMigrator يبني ويعمل.
- **الوقاية**
  - عند تحويل أي placeholder إلى تنفيذ فعلي، راجع dependencies وlifetime contracts بالكامل.

## الحالة 6: فشل اختبار delete في Customer Service

- **الأعراض**
  - اختبار unit يتوقع `undefined` ويفشل.
- **السبب الجذري**
  - رد `HttpClient` الفعلي كان `null` في هذا السيناريو، وليس `undefined`.
- **الملفات المتأثرة**
  - `frontend/src/app/proxy/customers/customer.service.spec.ts`
- **ما تغيّر**
  - تعديل التوقع إلى `toBeNull()`.
  - `flush(null)` بدل object فارغ.
- **لماذا نجح الحل**
  - التوقع أصبح مطابقًا للسلوك الفعلي في test harness.
- **الوقاية**
  - اربط assertions بسلوك HttpTestingController الفعلي وليس افتراضات عامة.

## الحالة 7: خطأ 500 في `GET /api/app/customers`

- **الأعراض**
  - واجهة العملاء لا تعرض البيانات.
  - الطلب `GET /api/app/customers?...` يرجع `500 Internal Server Error`.
- **السبب الجذري**
  - بعض App Services لم تكن موروثة من `ApplicationService` مما أثّر على سلوك ABP الافتراضي (authorization/localization/context wiring).
- **الملفات المتأثرة**
  - `backend/src/SaasSystem.Application/Customers/CustomerAppService.cs`
  - `backend/src/SaasSystem.Application/Projects/ProjectAppService.cs`
  - `backend/src/SaasSystem.Application/Invoices/InvoiceAppService.cs`
  - `backend/src/SaasSystem.Application/Payments/PaymentAppService.cs`
- **ما تغيّر**
  - توحيد الوراثة من `ApplicationService`.
- **لماذا نجح الحل**
  - رجع سلوك ABP القياسي للخدمات التطبيقية، وانتهى خطأ `500`.
- **الوقاية**
  - أي App Service جديدة يجب أن ترث من `ApplicationService` إلا لو في سبب معماري واضح.

## الحالة 8: تحذير Localization في Angular (`defaultResourceName was not defined`)

- **الأعراض**
  - تحذير متكرر في المتصفح:
    - `Localization source name is not specified and the defaultResourceName was not defined!`
- **السبب الجذري**
  - لم يتم تعيين resource افتراضي في طبقة العقود.
- **الملفات المتأثرة**
  - `backend/src/SaasSystem.Application.Contracts/SaasSystemApplicationContractsModule.cs`
- **ما تغيّر**
  - تعيين:
    - `options.DefaultResourceType = typeof(SaasSystemResource);`
- **لماذا نجح الحل**
  - صار ABP يعرف مصدر localization الافتراضي.
- **الوقاية**
  - عند إضافة Resource جديد، ثبّت `DefaultResourceType` مبكرًا.

## الحالة 9: فشل GitHub Actions بسبب استخدام `secrets.*` في job-level `if`

- **الأعراض**
  - فشل workflow قبل التشغيل مع خطأ parsing:
    - `Unrecognized named-value: 'secrets'`
- **السبب الجذري**
  - استخدام `secrets.*` مباشرة داخل شرط `if` على مستوى job.
- **الملفات المتأثرة**
  - `.github/workflows/ci-cd.yml`
- **ما تغيّر**
  - إزالة `secrets.*` من job-level condition.
  - إضافة خطوات precheck (`sonar_config`, `docker_config`) وتفعيل/تعطيل الخطوات عبر outputs.
- **لماذا نجح الحل**
  - workflow صار valid نحويًا، مع gate منطقي حسب توفر الأسرار.
- **الوقاية**
  - تجنب الاعتماد على `secrets.*` في job-level `if` عندما يظهر تعارض parsing، واستخدم step outputs كبديل.

## الحالة 10: فشل CI في `setup-dotnet` بسبب cache lockfile

- **الأعراض**
  - مرحلة build تفشل مبكرًا في إعداد .NET.
- **السبب الجذري**
  - `actions/setup-dotnet` مع `cache: true` يتطلب lockfile للحزم (`packages.lock.json`) غير موجود في المشروع.
- **الملفات المتأثرة**
  - `.github/workflows/ci-cd.yml`
- **ما تغيّر**
  - إزالة `cache: true` من خطوات `setup-dotnet`.
- **لماذا نجح الحل**
  - اختفى الشرط الإجباري للـ lockfile في CI.
- **الوقاية**
  - إمّا اعتماد NuGet lock files رسميًا، أو ترك cache معطل في .NET setup.

## الحالة 11: فشل CI في `npm ci` (EUSAGE / package-lock not in sync)

- **الأعراض**
  - `npm ci` يفشل برسالة:
    - `package.json and package-lock.json are not in sync`
    - `Missing: @angular/animations@19.2.18 from lock file`
- **السبب الجذري**
  - تعارض peer dependencies بين Angular 20 وحزم ABP/ngx-datatable المتوقعة لإصدار 19 في سياق npm@10 داخل GitHub Runner.
- **الملفات المتأثرة**
  - `.github/workflows/ci-cd.yml`
- **ما تغيّر**
  - استخدام:
    - `npm ci --legacy-peer-deps`
  - في مراحل build/test/security-scan.
- **لماذا نجح الحل**
  - `legacy-peer-deps` منع فشل حل الـ peers الصارم في CI.
- **الوقاية**
  - الحل الجذري طويل المدى: توحيد نسخ Angular وABP حسب مصفوفة التوافق الرسمية.

## الحالة 12: فشل Frontend lint بسبب `any` في `policy.guard.spec.ts`

- **الأعراض**
  - `npm run lint` يفشل بقواعد `@typescript-eslint/no-explicit-any`.
- **السبب الجذري**
  - استخدام `as any` في route/state داخل الاختبار.
- **الملفات المتأثرة**
  - `frontend/src/app/core/guards/policy.guard.spec.ts`
- **ما تغيّر**
  - استبدال `any` بأنواع واضحة:
    - `ActivatedRouteSnapshot`
    - `RouterStateSnapshot`
- **لماذا نجح الحل**
  - التزم الاختبار بقواعد lint الحالية بدون تعطيلها.
- **الوقاية**
  - في tests، استخدم casts مقيّدة (`unknown as Type`) بدل `any`.

## الحالة 13: فشل Integration Test لتصدير التقارير (Authorization behavior)

- **الأعراض**
  - اختبار `Reports_Export_Should_Require_Authorization` يفشل أحيانًا.
- **السبب الجذري**
  - في بيئة الاختبار، ABP قد يرمي `AbpAuthorizationException` بدل إرجاع status HTTP مباشر.
- **الملفات المتأثرة**
  - `backend/test/SaasSystem.HttpApi.Host.IntegrationTests/HealthAndDashboardTests.cs`
- **ما تغيّر**
  - جعل الاختبار يقبل سلوكين صحيحين:
    - `401/403` إذا رجعت response
    - أو `AbpAuthorizationException` إذا رُميت exception
- **لماذا نجح الحل**
  - صار الاختبار يعكس سلوك ABP الحقيقي في TestHost.
- **الوقاية**
  - اختبارات authorization يجب أن تتعامل مع pipeline behavior وليس status code فقط.

## الحالة 14: خطأ SQL `Database 'SaasSystemDb' does not exist`

- **الأعراض**
  - تنفيذ سكربت إنشاء user/login يفشل في `USE [SaasSystemDb]`.
- **السبب الجذري**
  - اسم قاعدة البيانات في السكربت لا يطابق الاسم الفعلي على الجهاز (`SaasSystem_Dev`).
- **الملفات المتأثرة**
  - إعدادات البيئة المحلية `.env`
  - سكربت SQL المحلي في SSMS
- **ما تغيّر**
  - استخدام اسم DB الصحيح (`SaasSystem_Dev`).
  - إنشاء:
    - `LOGIN [saas_user]`
    - `USER [saas_user] FOR LOGIN [saas_user]`
    - إضافة أدوار `db_datareader`, `db_datawriter`
- **لماذا نجح الحل**
  - الاتصال صار صحيحًا، والباكند اشتغل بنجاح على نفس القاعدة.
- **الوقاية**
  - قبل أي سكربت `USE [db]` تأكد من الاسم الفعلي من Object Explorer.

## الحالة 15: فشل build محلي بسبب ملف DLL مقفول (File Lock)

- **الأعراض**
  - أخطاء MSBuild مثل `MSB3021/MSB3027` مع رسالة `file is being used by another process`.
- **السبب الجذري**
  - عملية `SaasSystem.HttpApi.Host` كانت تعمل مسبقًا وتقفل ملفات `bin/Debug`.
- **الحل الفوري**
  - إيقاف العملية القديمة ثم إعادة التشغيل:
    - `Stop-Process -Id <PID> -Force`
    - ثم `dotnet run`
- **الوقاية**
  - قبل إعادة build/run، تأكد ما في نسخة host قديمة شغالة بنفس مسار build.

---

## 3) أخطاء محتملة مستقبلًا وكيف تصلحها

## 3.1 تعارض نسخ Angular/ABP (ERESOLVE peer dependencies)

- **الأعراض**
  - `npm install` أو `npm ci` يظهر `ERESOLVE`/peer conflict.
- **السبب المحتمل**
  - عدم توافق نسخ Angular الأساسية مع نسخ ABP Angular packages.
- **الحل**
  - راجع المصفوفة الرسمية للتوافق بين ABP وAngular.
  - ثبت نسخ dependencies في `package.json` و`package-lock.json`.
  - حل مؤقت للتطوير المحلي فقط: `npm install --legacy-peer-deps`.
- **الوقاية**
  - لا ترقّي Angular أو ABP بشكل منفصل بدون خطة compatibility.

## 3.2 `Connection string 'Default' was not found`

- **الأعراض**
  - فشل startup أو DbMigrator برسالة connection string مفقود.
- **السبب المحتمل**
  - غياب `ConnectionStrings:Default` في env/appsettings.
- **الحل**
  - عيّن متغير البيئة:
    - `$env:ConnectionStrings__Default = "Server=<...>;Database=<...>;..."`
  - أو اضبطه في `appsettings*.json`.
- **الوقاية**
  - تحقّق من connection string ضمن startup checklist.

## 3.3 SQL login failure أو connection timeout

- **الأعراض**
  - `Login failed for user 'sa'` أو timeout عند الاتصال.
- **السبب المحتمل**
  - كلمة مرور خاطئة، SQL Server غير جاهز، أو port/network policy.
- **الحل**
  - تحقق من `SA_PASSWORD`.
  - تحقق من container/pod status.
  - جرب اتصال مباشر من نفس البيئة.
- **الوقاية**
  - استخدم Secret management واضح ومراقبة readiness قبل التطبيق.

## 3.4 HTTPS/CORS failure بين Angular وAPI

- **الأعراض**
  - طلبات API تفشل من المتصفح برسائل CORS/SSL.
- **السبب المحتمل**
  - mismatch بين `apiBaseUrl` و`App:CorsOrigins`.
- **الحل**
  - طابق:
    - `frontend/src/environments/environment.ts` (`apiBaseUrl`)
    - `backend/src/SaasSystem.HttpApi.Host/appsettings*.json` (`CorsOrigins`, `SelfUrl`)
- **الوقاية**
  - وثّق URLs لكل بيئة (`dev/staging/prod`) في ملف قيم واضح.

## 3.5 Playwright browser غير مثبّت

- **الأعراض**
  - `Executable doesn't exist` أو browser launch failure.
- **الحل**
  - `npx playwright install --with-deps chromium`
- **الوقاية**
  - شغّل install ضمن CI stage قبل e2e.

## 3.6 مشاكل ChromeHeadless في CI

- **الأعراض**
  - Karma tests تفشل عند إطلاق Chrome.
- **الحل**
  - تأكد من وجود متطلبات headless browser في runner.
  - كخيار fallback استخدم Playwright component/e2e أو runner مناسب.
- **الوقاية**
  - ثبّت صورة CI مع browser dependencies جاهزة.

## 3.7 Docker build context/tag errors

- **الأعراض**
  - `COPY failed` أو image tag غير صحيح.
- **السبب المحتمل**
  - تنفيذ أمر build من مسار خاطئ أو tag غير صالح.
- **الحل**
  - استخدم أوامر المشروع المعتمدة:
    - `docker build -f devops/docker/Dockerfile.api -t <user>/saas-api:latest .`
    - `docker build -f devops/docker/Dockerfile.angular -t <user>/saas-angular:latest .`
- **الوقاية**
  - التزم بأوامر build من جذر المشروع.

## 3.8 Kubernetes `ImagePullBackOff`

- **الأعراض**
  - Pod لا يبدأ وحالته `ImagePullBackOff`.
- **السبب المحتمل**
  - image/tag خطأ أو registry auth ناقص.
- **الحل**
  - تحقق من image في deployment/values.
  - تحقق من imagePullSecrets إذا registry خاص.
- **الوقاية**
  - ثبّت naming convention للصور والتاجات.

## 3.9 Kubernetes `CrashLoopBackOff`

- **الأعراض**
  - Pod يعيد التشغيل باستمرار.
- **السبب المحتمل**
  - startup exception (غالبًا config/connection string).
- **الحل**
  - `kubectl logs <pod> -n saas-system`
  - `kubectl describe pod <pod> -n saas-system`
  - أصلح env/config ثم `rollout restart`.
- **الوقاية**
  - استخدم readiness/liveness مضبوطين وpre-deploy smoke check.

## 3.10 Kubernetes `Pending PVC`

- **الأعراض**
  - StatefulSet لا يبدأ والـ PVC في Pending.
- **السبب المحتمل**
  - StorageClass غير متاح أو insufficient storage.
- **الحل**
  - تحقق من:
    - `kubectl get pvc -n saas-system`
    - `kubectl describe pvc <name> -n saas-system`
  - عدل storage class أو الحجم.
- **الوقاية**
  - توحيد StorageClass policy في cluster baseline.

## 3.11 Helm upgrade failure

- **الأعراض**
  - `helm upgrade` يفشل أو release يبقى بحالة غير سليمة.
- **الحل**
  - `helm status saas -n saas-system`
  - `helm history saas -n saas-system`
  - `helm rollback saas <REVISION> -n saas-system`
- **الوقاية**
  - استخدم release checklist قبل أي upgrade.

## 3.12 SonarQube quality gate failure

- **الأعراض**
  - pipeline يتوقف في مرحلة quality gate.
- **السبب المحتمل**
  - code smells/bugs/coverage تحت المطلوب أو misconfigured report path.
- **الحل**
  - راجع `sonar-project.properties`.
  - تأكد من وجود `frontend/coverage/lcov.info`.
  - أصلح المشاكل ثم أعد pipeline.
- **الوقاية**
  - شغّل lint/tests/coverage محليًا قبل push.

---

## 4) أوامر تشخيص سريع

## Backend

```powershell
cd backend
dotnet build SaasSystem.slnx -c Debug
dotnet test SaasSystem.slnx -c Debug --no-build
cd src/SaasSystem.HttpApi.Host
dotnet run
```

## Frontend

```powershell
cd frontend
npm run lint
npm run test -- --watch=false --browsers=ChromeHeadless
npm run e2e
```

## Docker

```powershell
docker compose -f devops/docker-compose.yml ps
docker compose -f devops/docker-compose.yml logs api
docker compose -f devops/docker-compose.yml logs angular
docker compose -f devops/docker-compose.yml logs sqlserver
```

## Kubernetes

```powershell
kubectl get all -n saas-system
kubectl get events -n saas-system --sort-by=.metadata.creationTimestamp
kubectl describe pod <pod-name> -n saas-system
kubectl logs deployment/api -n saas-system
kubectl logs deployment/angular -n saas-system
```

## Helm

```powershell
helm list -n saas-system
helm status saas -n saas-system
helm history saas -n saas-system
```
