# Playwright Tests - Aspire Integration Summary

## Changes Made

### 1. **PlaywrightFixture.cs** - Enhanced for Aspire Support

#### Key Improvements:
- **Configurable Base URL**: Now reads from environment variables set by Aspire (`SERVICES__WEBAPI__HTTP__0`)
- **Health Check Retry Logic**: Waits up to 30 seconds for the API service to be healthy before running tests
- **SSL Certificate Handling**: Automatically trusts self-signed certificates in development
- **Environment Variable Priority**:
  1. `SERVICES__WEBAPI__HTTP__0` (Aspire service endpoint)
  2. `PLAYWRIGHT_BASE_URL` (custom override)
  3. `https://localhost:7252` (default fallback)

#### Changes:
```csharp
// Before: Hardcoded base URL
private const string BaseUrl = "https://localhost:7252";

// After: Configurable with environment variables
private string _baseUrl = null!;
private const string DefaultBaseUrl = "https://localhost:7252";
private const int MaxRetries = 30;
private const int RetryDelayMs = 1000;
```

### 2. **AppHost.cs** - Playwright Project Integration

Uncommented and enabled the Playwright project resource:
```csharp
var playwright = builder.AddProject<Projects.KafkaWorkflow_PlaywrightTests>("playwright")
    .WithExplicitStart()
    .WithEnvironment("ASPIRE", "true")
    .WithReference(webapi)
    .WaitFor(webapi);
```

- `WithExplicitStart()`: Tests only run when triggered from dashboard
- `WithReference(webapi)`: Ensures WebAPI environment variables are passed
- `WaitFor(webapi)`: Ensures WebAPI is ready before test runs

### 3. **AppHost.csproj** - Project Reference

Added the Playwright test project as a project reference:
```xml
<ProjectReference Include="..\test\KafkaWorkflow.PlaywrightTests\KafkaWorkflow.PlaywrightTests.csproj" />
```

### 4. **ASPIRE_SETUP.md** - Documentation

Created comprehensive guide covering:
- Prerequisites
- Configuration details
- How to run tests from the Aspire dashboard
- Environment setup
- Troubleshooting tips
- CI/CD integration

## How It Works

### Running from Aspire Dashboard

1. **Start Aspire**: `dotnet run --project KafkaWorkflow.AppHost`
2. **Navigate Dashboard**: Open http://localhost:18626 (or provided URL)
3. **Find Playwright**: Look for "playwright" resource in Resources tab
4. **Run Tests**: Click the Run button
5. **Monitor**: Watch progress in resource logs

### Service Discovery Flow

```
Test Starts
    ↓
Check SERVICES__WEBAPI__HTTP__0 (Aspire)
    ↓ (if not found)
Check PLAYWRIGHT_BASE_URL
    ↓ (if not found)
Use Default (https://localhost:7252)
    ↓
Wait for Service Health (up to 30 retries, 1s each)
    ↓
Launch Browser & Run Tests
```

## Benefits

✅ **Automated Service Orchestration**: No manual port/URL configuration  
✅ **Resilient**: Retries when services are starting up  
✅ **SSL Handling**: Works with self-signed certificates  
✅ **Dashboard Integration**: Visual monitoring of test execution  
✅ **Dependency Management**: WebAPI and dependencies start before tests  
✅ **Environment Isolated**: Each test run has consistent environment  

## Running Tests Standalone (Manual)

If you need to run tests without Aspire:

```bash
# Start API manually
dotnet run --project KafkaWorkflow.WebApi

# In another terminal, run tests
dotnet test test/KafkaWorkflow.PlaywrightTests/KafkaWorkflow.PlaywrightTests.csproj \
  --configuration Release
```

Or set custom URL:
```bash
$env:PLAYWRIGHT_BASE_URL = "https://localhost:7252"
dotnet test test/KafkaWorkflow.PlaywrightTests/
```

## Test Classes Available

| Test Class | Purpose |
|-----------|---------|
| AddressControllerTests | CRUD operations for addresses |
| ContactControllerTests | CRUD operations for contacts |
| PeopleControllerTests | CRUD operations for people |
| PeopleControllerErrorHandlingTests | Error scenarios |
| MultiControllerWorkflowTests | End-to-end workflows |

## Current Capabilities

- ✅ Automatic service endpoint discovery
- ✅ Health check with retry logic
- ✅ SSL certificate validation bypass
- ✅ Browser context isolation
- ✅ Network idle wait states
- ✅ Concurrent request handling

## Troubleshooting

### "Service not available" error
- Increase `MaxRetries` in PlaywrightFixture.cs
- Check WebAPI logs in Aspire dashboard
- Ensure SQL Server and Kafka are running

### SSL Certificate warnings
- Already handled by `IgnoreHTTPSErrors = true`
- Works with self-signed development certificates

### Port conflicts
- Aspire automatically assigns alternate ports
- Check dashboard for actual port assignments

## Next Steps

1. **Run from Dashboard**: `dotnet run --project KafkaWorkflow.AppHost`
2. **Navigate to Dashboard**: http://localhost:18626
3. **Execute Tests**: Click Run on playwright resource
4. **Monitor Output**: View detailed logs and results

---

**Documentation**: See [ASPIRE_SETUP.md](./ASPIRE_SETUP.md) for detailed setup instructions.
