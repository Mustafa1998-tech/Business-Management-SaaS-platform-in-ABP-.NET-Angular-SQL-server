# API Endpoint Testing Guide

This document provides comprehensive instructions for testing the SaaS Platform API endpoints from the terminal.

## Prerequisites

### Required Tools
- **PowerShell 7+** (for `test-endpoints.ps1`)
- **cURL** (for all scripts)
- **jq** (optional, for JSON formatting in bash script)
- **Python 3** (for JSON formatting in bash script)

### SSL Certificate Handling
The API uses HTTPS with a self-signed certificate. All scripts handle this automatically:
- **Batch script**: Uses `-k` flag with curl
- **PowerShell script**: Uses custom certificate policy
- **Bash script**: Uses `-k` flag with curl

## Available Test Scripts

### 1. Batch Script (Windows)
```bash
test-endpoints.bat
```
- Basic endpoint testing
- Uses curl commands
- Generates Excel and PDF files
- Windows batch file format

### 2. PowerShell Script (Cross-platform)
```powershell
.\test-endpoints.ps1
```
- Advanced error handling
- Detailed response analysis
- File size reporting
- Customizable parameters

### 3. Bash Script (Linux/macOS/WSL)
```bash
./test-endpoints.sh
```
- Unix/Linux compatible
- JSON formatting with Python
- File size reporting
- Customizable base URL

## API Endpoints Tested

### Authentication
- **POST** `/connect/token` - Get access token

### Reports Module
- **GET** `/api/app/reports/revenue` - Revenue report with date range
- **GET** `/api/app/reports/invoices` - Invoices report with filtering
- **GET** `/api/app/reports/invoices-excel` - Export invoices to Excel
- **GET** `/api/app/reports/invoices-pdf` - Export invoices to PDF

### Business Modules
- **GET** `/api/app/customers` - Customer management
- **GET** `/api/app/projects` - Project management
- **GET** `/api/app/dashboard` - Dashboard analytics

## Usage Examples

### Basic Usage
```bash
# Windows Batch
test-endpoints.bat

# PowerShell
.\test-endpoints.ps1

# Bash
./test-endpoints.sh
```

### Custom Base URL
```powershell
# PowerShell with custom URL
.\test-endpoints.ps1 -BaseUrl "https://api.example.com"

# Bash with custom URL
./test-endpoints.sh "https://api.example.com"
```

### SSL Certificate Options
```powershell
# Skip SSL validation (PowerShell)
.\test-endpoints.ps1 -SkipSslCheck

# The bash and batch scripts automatically skip SSL validation
```

## Expected Output

### Successful Test Run
```
========================================
SaaS Platform API Endpoint Testing
========================================
Base URL: https://localhost:44369

[1] Testing API connectivity...
✓ API is responding (Status: 200)

[2] Getting authentication token...
✓ Authentication successful
Token type: Bearer
Expires in: 3600 seconds

[3] Testing Revenue Report Endpoint...
✓ Revenue report endpoint working (Status: 200)
  Total Revenue: 15000.50
  Period: 2024-01-01 to 2024-12-31
  Data points: 12

[4] Testing Invoices Report Endpoint...
✓ Invoices report endpoint working (Status: 200)
  Total invoices: 25
  Paid invoices: 20
  Pending invoices: 3
  Overdue invoices: 2

[5] Testing Excel Export...
✓ Excel export working (Status: 200)
  File: invoices-export.xlsx
  File size: 15.23 KB

[6] Testing PDF Export...
✓ PDF export working (Status: 200)
  File: invoices-export.pdf
  File size: 45.67 KB

[7] Testing Customers Endpoint...
✓ Customers endpoint working (Status: 200)
  Total customers: 10
  Items returned: 10

[8] Testing Projects Endpoint...
✓ Projects endpoint working (Status: 200)
  Total projects: 15
  Items returned: 15

[9] Testing Dashboard Endpoint...
✓ Dashboard endpoint working (Status: 200)
  Dashboard data retrieved successfully

========================================
Testing completed!
========================================

Files generated:
  - invoices-export.xlsx
  - invoices-export.pdf
```

## Manual Testing with cURL

### Get Authentication Token
```bash
curl -k -X POST "https://localhost:44369/connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=SaasSystem_Angular&grant_type=client_credentials&scope=SaasSystem"
```

### Test Revenue Report
```bash
curl -k -X GET "https://localhost:44369/api/app/reports/revenue?from=2024-01-01&to=2024-12-31" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json"
```

### Test Invoices Report
```bash
curl -k -X GET "https://localhost:44369/api/app/reports/invoices" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json"
```

### Download Excel File
```bash
curl -k -X GET "https://localhost:44369/api/app/reports/invoices-excel" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -o "invoices.xlsx"
```

### Download PDF File
```bash
curl -k -X GET "https://localhost:44369/api/app/reports/invoices-pdf" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -o "invoices.pdf"
```

## Troubleshooting

### Common Issues

#### 1. API Not Responding
- **Problem**: Connection refused or timeout
- **Solution**: Ensure the API is running on `https://localhost:44369`
- **Command**: Check with `netstat -an | findstr :44369` (Windows) or `netstat -an | grep :44369` (Linux)

#### 2. Authentication Failed
- **Problem**: Invalid client credentials
- **Solution**: Verify `client_id` and `scope` in `appsettings.json`
- **Check**: `SaasSystem_Angular` should be configured in OpenIddict settings

#### 3. SSL Certificate Error
- **Problem**: Certificate validation failed
- **Solution**: All scripts handle this automatically with `-k` flag or custom policy
- **Manual**: Use `-k` flag with curl or handle certificates in your HTTP client

#### 4. Database Connection Error
- **Problem**: API starts but returns database errors
- **Solution**: Run database migrations first
- **Command**: `cd backend/src/SaasSystem.DbMigrator && dotnet run`

#### 5. Permission Denied
- **Problem**: 403 Forbidden responses
- **Solution**: Check user permissions and tenant access
- **Verify**: User has required permissions for the endpoints

### Debug Mode

For detailed debugging, modify the scripts to show full response headers and body:

```powershell
# PowerShell debug
$response = Invoke-WebRequest -Verbose @params
$response.Headers | Format-List
$response.Content
```

```bash
# Bash debug
curl -v -k -X GET "$BASE_URL/api/app/reports/revenue?from=2024-01-01&to=2024-12-31" \
     -H "Authorization: Bearer $TOKEN"
```

## Performance Testing

### Load Testing with curl
```bash
# Simple load test (10 concurrent requests)
for i in {1..10}; do
    curl -k -X GET "https://localhost:44369/api/app/reports/revenue?from=2024-01-01&to=2024-12-31" \
         -H "Authorization: Bearer $TOKEN" \
         -w "Request $i: %{time_total}s\n" \
         -o /dev/null \
         -s &
done
wait
```

### Benchmark with Apache Bench
```bash
ab -n 100 -c 10 -H "Authorization: Bearer $TOKEN" \
   "https://localhost:44369/api/app/reports/revenue?from=2024-01-01&to=2024-12-31"
```

## Integration with CI/CD

### GitHub Actions Example
```yaml
- name: Test API Endpoints
  run: |
    ./test-endpoints.sh "https://api.${{ github.sha }}.example.com"
  env:
    SA_PASSWORD: ${{ secrets.DB_PASSWORD }}
```

### Docker Integration
```bash
# Test endpoints in Docker container
docker run --rm -v $(pwd):/app -w /app \
  curlimages/curl:latest ./test-endpoints.sh "https://api:44369"
```

## Next Steps

1. **Run the API**: Start the backend service
2. **Execute Tests**: Choose your preferred test script
3. **Analyze Results**: Review response times and data
4. **Integrate**: Add to your CI/CD pipeline
5. **Monitor**: Set up automated endpoint monitoring

For more advanced testing scenarios, consider using tools like:
- **Postman** for GUI-based testing
- **Newman** for automated Postman collections
- **JMeter** for load testing
- **K6** for modern load testing
