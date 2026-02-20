# SaaS Platform API Endpoint Testing Script
# Requires PowerShell 7+ and modules: Microsoft.PowerShell.Utility, WebRequest

param(
    [string]$BaseUrl = "http://localhost:5069",
    [string]$Username = "admin",
    [string]$Password = "",
    [switch]$SkipSslCheck = $false
)

function Import-DotEnv {
    param([string]$Path = ".env")

    if (-not (Test-Path $Path)) {
        return
    }

    Get-Content $Path | ForEach-Object {
        $line = $_.Trim()
        if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith("#")) {
            return
        }

        if ($line.StartsWith("export ")) {
            $line = $line.Substring(7).Trim()
        }

        $parts = $line.Split("=", 2)
        if ($parts.Count -ne 2) {
            return
        }

        $name = $parts[0].Trim()
        $value = $parts[1].Trim()

        if (($value.StartsWith('"') -and $value.EndsWith('"')) -or ($value.StartsWith("'") -and $value.EndsWith("'"))) {
            $value = $value.Substring(1, $value.Length - 2)
        }

        if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name, "Process"))) {
            [Environment]::SetEnvironmentVariable($name, $value, "Process")
        }
    }
}

Import-DotEnv

if ([string]::IsNullOrWhiteSpace($Password)) {
    $Password = $env:SAASSYSTEM_DEFAULT_PASSWORD
}

if ([string]::IsNullOrWhiteSpace($Password)) {
    throw "SAASSYSTEM_DEFAULT_PASSWORD is missing. Set it in .env or pass -Password."
}

# SSL certificate handling
if ($SkipSslCheck) {
    add-type @"
        using System.Net;
        using System.Security.Cryptography.X509Certificates;
        public class TrustAllCertsPolicy : ICertificatePolicy {
            public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem) {
                return true;
            }
        }
"@
    [System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SaaS Platform API Endpoint Testing" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host ""

$token = $null

# Function to make HTTP requests with error handling
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Endpoint,
        [hashtable]$Headers = @{},
        [string]$Body = $null,
        [string]$ContentType = $null,
        [string]$OutputFile = $null
    )
    
    try {
        $url = "$BaseUrl$Endpoint"
        $params = @{
            Method = $Method
            Uri = $url
            Headers = $Headers
        }
        
        if ($Body) {
            $params.Body = $Body
            $params.ContentType = if ([string]::IsNullOrWhiteSpace($ContentType)) { "application/json" } else { $ContentType }
        }
        
        if ($OutputFile) {
            $params.OutFile = $OutputFile
        }
        
        $response = Invoke-WebRequest @params -SkipHttpErrorCheck
        $isSuccessStatus = ($response.StatusCode -ge 200 -and $response.StatusCode -lt 300)
        return @{
            Success = $isSuccessStatus
            StatusCode = $response.StatusCode
            Content = if (-not $OutputFile) { $response.Content } else { "File saved to $OutputFile" }
            Headers = $response.Headers
        }
    }
    catch {
        $statusCode = $null
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $statusCode = $_.Exception.Response.StatusCode.value__
        }

        return @{
            Success = $false
            StatusCode = $statusCode
            Content = $_.Exception.Message
            Headers = @{}
        }
    }
}

# Test 1: API Connectivity
Write-Host "[1] Testing API connectivity..." -ForegroundColor Green
$healthCheck = Invoke-ApiRequest -Method "GET" -Endpoint "/health"
if ($healthCheck.Success) {
    Write-Host "✓ API is responding (Status: $($healthCheck.StatusCode))" -ForegroundColor Green
} else {
    Write-Host "✗ API not responding (Status: $($healthCheck.StatusCode))" -ForegroundColor Red
}
Write-Host ""

# Test 2: Authentication (password flow)
Write-Host "[2] Getting authentication token..." -ForegroundColor Green
$encodedUsername = [System.Uri]::EscapeDataString($Username)
$encodedPassword = [System.Uri]::EscapeDataString($Password)
$authBody = "client_id=SaasSystem_Angular&grant_type=password&username=$encodedUsername&password=$encodedPassword&scope=offline_access%20SaasSystem"
$authResponse = Invoke-ApiRequest -Method "POST" -Endpoint "/connect/token" -Headers @{
    "Content-Type" = "application/x-www-form-urlencoded"
} -Body $authBody -ContentType "application/x-www-form-urlencoded"

if ($authResponse.Success) {
    try {
        $tokenData = $authResponse.Content | ConvertFrom-Json
        $token = $tokenData.access_token
        if (-not [string]::IsNullOrWhiteSpace($token)) {
            Write-Host "✓ Authentication successful" -ForegroundColor Green
            Write-Host "Token type: $($tokenData.token_type)" -ForegroundColor Gray
            Write-Host "Expires in: $($tokenData.expires_in) seconds" -ForegroundColor Gray
        } else {
            Write-Host "✗ Authentication response did not include access_token" -ForegroundColor Red
            Write-Host "Response: $($authResponse.Content)" -ForegroundColor Gray
        }
    } catch {
        Write-Host "✗ Failed to parse token response" -ForegroundColor Red
        Write-Host "Response: $($authResponse.Content)" -ForegroundColor Gray
    }
} else {
    Write-Host "✗ Authentication failed (Status: $($authResponse.StatusCode))" -ForegroundColor Red
    Write-Host "Response: $($authResponse.Content)" -ForegroundColor Gray
}
Write-Host ""

# Set up authorization header if token is available
$authHeaders = @{}
if ($token) {
    $authHeaders["Authorization"] = "Bearer $token"
    $authHeaders["Content-Type"] = "application/json"
}

# Test 3: Revenue Report
Write-Host "[3] Testing Revenue Report Endpoint..." -ForegroundColor Green
$revenueResponse = Invoke-ApiRequest -Method "GET" -Endpoint "/api/app/reports/revenue?from=2024-01-01&to=2024-12-31" -Headers $authHeaders
if ($revenueResponse.Success) {
    Write-Host "✓ Revenue report endpoint working (Status: $($revenueResponse.StatusCode))" -ForegroundColor Green
    try {
        $data = $revenueResponse.Content | ConvertFrom-Json
        Write-Host "  Total Revenue: $($data.totalRevenue)" -ForegroundColor Gray
        Write-Host "  Period: $($data.from) to $($data.to)" -ForegroundColor Gray
        Write-Host "  Data points: $($data.buckets.Count)" -ForegroundColor Gray
    } catch {
        Write-Host "  Response: $($revenueResponse.Content)" -ForegroundColor Gray
    }
} else {
    Write-Host "✗ Revenue report endpoint failed (Status: $($revenueResponse.StatusCode))" -ForegroundColor Red
    Write-Host "  Response: $($revenueResponse.Content)" -ForegroundColor Gray
}
Write-Host ""

# Test 4: Invoices Report
Write-Host "[4] Testing Invoices Report Endpoint..." -ForegroundColor Green
$invoicesResponse = Invoke-ApiRequest -Method "GET" -Endpoint "/api/app/reports/invoices" -Headers $authHeaders
if ($invoicesResponse.Success) {
    Write-Host "✓ Invoices report endpoint working (Status: $($invoicesResponse.StatusCode))" -ForegroundColor Green
    try {
        $data = $invoicesResponse.Content | ConvertFrom-Json
        Write-Host "  Total invoices: $($data.summary.totalInvoices)" -ForegroundColor Gray
        Write-Host "  Paid invoices: $($data.summary.paidInvoices)" -ForegroundColor Gray
        Write-Host "  Pending invoices: $($data.summary.pendingInvoices)" -ForegroundColor Gray
        Write-Host "  Overdue invoices: $($data.summary.overdueInvoices)" -ForegroundColor Gray
    } catch {
        Write-Host "  Response: $($invoicesResponse.Content)" -ForegroundColor Gray
    }
} else {
    Write-Host "✗ Invoices report endpoint failed (Status: $($invoicesResponse.StatusCode))" -ForegroundColor Red
    Write-Host "  Response: $($invoicesResponse.Content)" -ForegroundColor Gray
}
Write-Host ""

# Test 5: Excel Export
Write-Host "[5] Testing Excel Export..." -ForegroundColor Green
$excelResponse = Invoke-ApiRequest -Method "GET" -Endpoint "/api/app/reports/invoices-excel" -Headers $authHeaders -OutputFile "invoices-export.xlsx"
if ($excelResponse.Success) {
    Write-Host "✓ Excel export working (Status: $($excelResponse.StatusCode))" -ForegroundColor Green
    Write-Host "  File: invoices-export.xlsx" -ForegroundColor Gray
    if (Test-Path "invoices-export.xlsx") {
        $fileInfo = Get-Item "invoices-export.xlsx"
        Write-Host "  File size: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
    }
} else {
    Write-Host "✗ Excel export failed (Status: $($excelResponse.StatusCode))" -ForegroundColor Red
    Write-Host "  Response: $($excelResponse.Content)" -ForegroundColor Gray
}
Write-Host ""

# Test 6: PDF Export
Write-Host "[6] Testing PDF Export..." -ForegroundColor Green
$pdfResponse = Invoke-ApiRequest -Method "GET" -Endpoint "/api/app/reports/invoices-pdf" -Headers $authHeaders -OutputFile "invoices-export.pdf"
if ($pdfResponse.Success) {
    Write-Host "✓ PDF export working (Status: $($pdfResponse.StatusCode))" -ForegroundColor Green
    Write-Host "  File: invoices-export.pdf" -ForegroundColor Gray
    if (Test-Path "invoices-export.pdf") {
        $fileInfo = Get-Item "invoices-export.pdf"
        Write-Host "  File size: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
    }
} else {
    Write-Host "✗ PDF export failed (Status: $($pdfResponse.StatusCode))" -ForegroundColor Red
    Write-Host "  Response: $($pdfResponse.Content)" -ForegroundColor Gray
}
Write-Host ""

# Test 7: Customers Endpoint
Write-Host "[7] Testing Customers Endpoint..." -ForegroundColor Green
$customersResponse = Invoke-ApiRequest -Method "GET" -Endpoint "/api/app/customers?skipCount=0&maxResultCount=5" -Headers $authHeaders
if ($customersResponse.Success) {
    Write-Host "✓ Customers endpoint working (Status: $($customersResponse.StatusCode))" -ForegroundColor Green
    try {
        $data = $customersResponse.Content | ConvertFrom-Json
        Write-Host "  Total customers: $($data.totalCount)" -ForegroundColor Gray
        Write-Host "  Items returned: $($data.items.Count)" -ForegroundColor Gray
    } catch {
        Write-Host "  Response: $($customersResponse.Content)" -ForegroundColor Gray
    }
} else {
    Write-Host "✗ Customers endpoint failed (Status: $($customersResponse.StatusCode))" -ForegroundColor Red
    Write-Host "  Response: $($customersResponse.Content)" -ForegroundColor Gray
}
Write-Host ""

# Test 8: Projects Endpoint
Write-Host "[8] Testing Projects Endpoint..." -ForegroundColor Green
$projectsResponse = Invoke-ApiRequest -Method "GET" -Endpoint "/api/app/projects?skipCount=0&maxResultCount=5" -Headers $authHeaders
if ($projectsResponse.Success) {
    Write-Host "✓ Projects endpoint working (Status: $($projectsResponse.StatusCode))" -ForegroundColor Green
    try {
        $data = $projectsResponse.Content | ConvertFrom-Json
        Write-Host "  Total projects: $($data.totalCount)" -ForegroundColor Gray
        Write-Host "  Items returned: $($data.items.Count)" -ForegroundColor Gray
    } catch {
        Write-Host "  Response: $($projectsResponse.Content)" -ForegroundColor Gray
    }
} else {
    Write-Host "✗ Projects endpoint failed (Status: $($projectsResponse.StatusCode))" -ForegroundColor Red
    Write-Host "  Response: $($projectsResponse.Content)" -ForegroundColor Gray
}
Write-Host ""

# Test 9: Dashboard Endpoint
Write-Host "[9] Testing Dashboard Endpoint..." -ForegroundColor Green
$dashboardResponse = Invoke-ApiRequest -Method "GET" -Endpoint "/api/app/dashboard/stats" -Headers $authHeaders
if ($dashboardResponse.Success) {
    Write-Host "✓ Dashboard endpoint working (Status: $($dashboardResponse.StatusCode))" -ForegroundColor Green
    try {
        $data = $dashboardResponse.Content | ConvertFrom-Json
        Write-Host "  Dashboard data retrieved successfully" -ForegroundColor Gray
    } catch {
        Write-Host "  Response: $($dashboardResponse.Content)" -ForegroundColor Gray
    }
} else {
    Write-Host "✗ Dashboard endpoint failed (Status: $($dashboardResponse.StatusCode))" -ForegroundColor Red
    Write-Host "  Response: $($dashboardResponse.Content)" -ForegroundColor Gray
}
Write-Host ""

# Cleanup
if (Test-Path "token.txt") {
    Remove-Item "token.txt" -Force
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing completed!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Summary
Write-Host ""
Write-Host "Files generated:" -ForegroundColor Yellow
if (Test-Path "invoices-export.xlsx") {
    Write-Host "  - invoices-export.xlsx" -ForegroundColor Gray
}
if (Test-Path "invoices-export.pdf") {
    Write-Host "  - invoices-export.pdf" -ForegroundColor Gray
}

Write-Host ""
Write-Host "To run with custom URL:" -ForegroundColor Yellow
Write-Host "  .\test-endpoints.ps1 -BaseUrl 'https://your-server:port'" -ForegroundColor Gray
Write-Host ""
Write-Host "To skip SSL certificate validation:" -ForegroundColor Yellow
Write-Host "  .\test-endpoints.ps1 -SkipSslCheck" -ForegroundColor Gray
