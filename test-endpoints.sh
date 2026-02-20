#!/bin/bash

# SaaS Platform API Endpoint Testing Script
# Usage: ./test-endpoints.sh [base-url]

if [ -f ".env" ]; then
    set -a
    # shellcheck disable=SC1091
    . ./.env
    set +a
fi

BASE_URL=${1:-"http://localhost:5069"}
TOKEN=""
USERNAME=${2:-"admin"}
PASSWORD=${3:-"${SAASSYSTEM_DEFAULT_PASSWORD:-}"}

if [ -z "$PASSWORD" ]; then
    echo "Error: SAASSYSTEM_DEFAULT_PASSWORD is not set. Add it to .env or pass the password as argument 3."
    exit 1
fi

echo "========================================"
echo "SaaS Platform API Endpoint Testing"
echo "========================================"
echo "Base URL: $BASE_URL"
echo ""

# Function to test endpoint
test_endpoint() {
    local method=$1
    local endpoint=$2
    local description=$3
    local data=$4
    
    echo "[$description] Testing $method $endpoint..."
    
    if [ -n "$TOKEN" ]; then
        response=$(curl -k -s -w "\nHTTP_STATUS:%{http_code}" \
            -X "$method" \
            -H "Authorization: Bearer $TOKEN" \
            -H "Content-Type: application/json" \
            ${data:+-d "$data"} \
            "$BASE_URL$endpoint")
    else
        response=$(curl -k -s -w "\nHTTP_STATUS:%{http_code}" \
            -X "$method" \
            -H "Content-Type: application/json" \
            ${data:+-d "$data"} \
            "$BASE_URL$endpoint")
    fi
    
    http_code=$(echo "$response" | tail -n1 | cut -d: -f2)
    body=$(echo "$response" | sed '$d')
    
    if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
        echo "✓ Success (HTTP $http_code)"
        echo "$body" | python3 -m json.tool 2>/dev/null || echo "$body"
    else
        echo "✗ Failed (HTTP $http_code)"
        echo "$body"
    fi
    echo ""
}

# Test 1: Health endpoint
echo "[1] Testing health endpoint..."
health_response=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/health")
if [ "$health_response" = "200" ]; then
    echo "✓ Health endpoint is up (HTTP 200)"
else
    echo "✗ Health endpoint failed (HTTP $health_response)"
fi
echo ""

# Test 2: Get authentication token (password flow)
echo "[2] Getting authentication token..."
token_response=$(curl -s -X POST "$BASE_URL/connect/token" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    --data-urlencode "client_id=SaasSystem_Angular" \
    --data-urlencode "grant_type=password" \
    --data-urlencode "username=$USERNAME" \
    --data-urlencode "password=$PASSWORD" \
    --data-urlencode "scope=offline_access SaasSystem")

if echo "$token_response" | python3 -c "import sys, json; json.load(sys.stdin)" 2>/dev/null; then
    TOKEN=$(echo "$token_response" | python3 -c "import sys, json; print(json.load(sys.stdin).get('access_token', ''))")
    echo "✓ Token obtained successfully"
else
    echo "✗ Failed to get token"
    echo "$token_response"
fi
echo ""

# Test 3: Revenue Report
test_endpoint "GET" "/api/app/reports/revenue?from=2024-01-01&to=2024-12-31" "3" "Revenue Report"

# Test 4: Invoices Report
test_endpoint "GET" "/api/app/reports/invoices" "4" "Invoices Report"

# Test 5: Excel Export
echo "[5] Testing Excel Export..."
if [ -n "$TOKEN" ]; then
    curl -s -H "Authorization: Bearer $TOKEN" \
         -H "Content-Type: application/json" \
         "$BASE_URL/api/app/reports/invoices-excel" \
         -o "invoices-export.xlsx" \
         -w "HTTP Status: %{http_code}, File size: %{size_download} bytes"
else
    curl -s -H "Content-Type: application/json" \
         "$BASE_URL/api/app/reports/invoices-excel" \
         -o "invoices-export.xlsx" \
         -w "HTTP Status: %{http_code}, File size: %{size_download} bytes"
fi
echo ""
echo ""

# Test 6: PDF Export
echo "[6] Testing PDF Export..."
if [ -n "$TOKEN" ]; then
    curl -s -H "Authorization: Bearer $TOKEN" \
         -H "Content-Type: application/json" \
         "$BASE_URL/api/app/reports/invoices-pdf" \
         -o "invoices-export.pdf" \
         -w "HTTP Status: %{http_code}, File size: %{size_download} bytes"
else
    curl -s -H "Content-Type: application/json" \
         "$BASE_URL/api/app/reports/invoices-pdf" \
         -o "invoices-export.pdf" \
         -w "HTTP Status: %{http_code}, File size: %{size_download} bytes"
fi
echo ""
echo ""

# Test 7: Customers
test_endpoint "GET" "/api/app/customers?skipCount=0&maxResultCount=5" "7" "Customers"

# Test 8: Projects
test_endpoint "GET" "/api/app/projects?skipCount=0&maxResultCount=5" "8" "Projects"

# Test 9: Dashboard
test_endpoint "GET" "/api/app/dashboard/stats" "9" "Dashboard"

echo "========================================"
echo "Testing completed!"
echo "========================================"

# Show generated files
echo ""
echo "Files generated:"
if [ -f "invoices-export.xlsx" ]; then
    size=$(stat -f%z "invoices-export.xlsx" 2>/dev/null || stat -c%s "invoices-export.xlsx" 2>/dev/null)
    echo "  - invoices-export.xlsx ($(($size / 1024)) KB)"
fi

if [ -f "invoices-export.pdf" ]; then
    size=$(stat -f%z "invoices-export.pdf" 2>/dev/null || stat -c%s "invoices-export.pdf" 2>/dev/null)
    echo "  - invoices-export.pdf ($(($size / 1024)) KB)"
fi
