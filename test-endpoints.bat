@echo off
setlocal

if exist ".env" (
  for /f "usebackq tokens=1,* delims==" %%A in (".env") do (
    if not "%%A"=="" if not "%%A:~0,1%"=="#" set "%%A=%%B"
  )
)

echo ========================================
echo SaaS Platform API Endpoint Testing
echo ========================================
echo.

set BASE_URL=http://localhost:5069
set TOKEN=
set USERNAME=admin
if "%SAASSYSTEM_DEFAULT_PASSWORD%"=="" (
  echo ERROR: SAASSYSTEM_DEFAULT_PASSWORD is missing. Set it in .env before running this script.
  exit /b 1
)
set PASSWORD=%SAASSYSTEM_DEFAULT_PASSWORD%

:: Test 1: Health check
echo [1] Testing API connectivity...
curl -s -o nul -w "HTTP Status: %%{http_code}\n" %BASE_URL%/health || echo API not responding
echo.

:: Test 2: Get authentication token (password flow)
echo [2] Getting authentication token...
curl -X POST "%BASE_URL%/connect/token" ^
  -H "Content-Type: application/x-www-form-urlencoded" ^
  -d "client_id=SaasSystem_Angular&grant_type=password&username=%USERNAME%&password=%PASSWORD%&scope=offline_access%%20SaasSystem" ^
  -s > token-response.json

powershell -NoProfile -Command "$j = Get-Content token-response.json -Raw | ConvertFrom-Json; if($j.access_token){ Set-Content token.txt $j.access_token }" >nul 2>nul

if exist token.txt (
    set /p TOKEN=<token.txt
    echo Token obtained successfully
) else (
    echo Failed to get token - using anonymous access
)
echo.

:: Test 3: Revenue Report Endpoint
echo [3] Testing Revenue Report Endpoint...
curl -X GET "%BASE_URL%/api/app/reports/revenue?from=2024-01-01&to=2024-12-31" ^
  -H "Authorization: Bearer %TOKEN%" ^
  -H "Content-Type: application/json" ^
  -s | jq . 2>nul || echo Revenue report endpoint test failed
echo.

:: Test 4: Invoices Report Endpoint
echo [4] Testing Invoices Report Endpoint...
curl -X GET "%BASE_URL%/api/app/reports/invoices" ^
  -H "Authorization: Bearer %TOKEN%" ^
  -H "Content-Type: application/json" ^
  -s | jq . 2>nul || echo Invoices report endpoint test failed
echo.

:: Test 5: Invoices Excel Export
echo [5] Testing Invoices Excel Export...
curl -X GET "%BASE_URL%/api/app/reports/invoices-excel" ^
  -H "Authorization: Bearer %TOKEN%" ^
  -H "Content-Type: application/json" ^
  -s -o "invoices-export.xlsx" ^
  -w "HTTP Status: %%{http_code}, File size: %%{size_download} bytes"
echo.

:: Test 6: Invoices PDF Export
echo [6] Testing Invoices PDF Export...
curl -X GET "%BASE_URL%/api/app/reports/invoices-pdf" ^
  -H "Authorization: Bearer %TOKEN%" ^
  -H "Content-Type: application/json" ^
  -s -o "invoices-export.pdf" ^
  -w "HTTP Status: %%{http_code}, File size: %%{size_download} bytes"
echo.

:: Test 7: Customers Endpoint
echo [7] Testing Customers Endpoint...
curl -X GET "%BASE_URL%/api/app/customers?skipCount=0&maxResultCount=5" ^
  -H "Authorization: Bearer %TOKEN%" ^
  -H "Content-Type: application/json" ^
  -s | jq . 2>nul || echo Customers endpoint test failed
echo.

:: Test 8: Projects Endpoint
echo [9] Testing Projects Endpoint...
curl -X GET "%BASE_URL%/api/app/projects?skipCount=0&maxResultCount=5" ^
  -H "Authorization: Bearer %TOKEN%" ^
  -H "Content-Type: application/json" ^
  -s | jq . 2>nul || echo Projects endpoint test failed
echo.

:: Test 9: Dashboard Endpoint
echo [10] Testing Dashboard Endpoint...
curl -X GET "%BASE_URL%/api/app/dashboard/stats" ^
  -H "Authorization: Bearer %TOKEN%" ^
  -H "Content-Type: application/json" ^
  -s | jq . 2>nul || echo Dashboard endpoint test failed
echo.

:: Cleanup
if exist token.txt del token.txt
if exist token-response.json del token-response.json

echo ========================================
echo Testing completed!
echo ========================================
pause
