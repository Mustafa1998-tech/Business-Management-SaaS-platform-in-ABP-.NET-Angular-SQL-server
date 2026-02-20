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
