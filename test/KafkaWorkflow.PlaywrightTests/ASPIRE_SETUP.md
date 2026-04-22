# Running Playwright Tests from Aspire Dashboard

This guide explains how to execute the Playwright integration tests from the Aspire Dashboard.

## Prerequisites

- .NET 10.0 or later
- Docker Desktop (for Aspire containers)
- Aspire workload installed: `dotnet workload install aspire`

## Configuration

The Playwright tests are now configured to work seamlessly with Aspire:

### Automatic Service Discovery

The `PlaywrightFixture` automatically discovers the WebAPI service endpoint through:

1. **Aspire Environment Variables** (highest priority)
   - `SERVICES__WEBAPI__HTTP__0` - Set by Aspire when the project is added as a resource

2. **Custom Environment Variable**
   - `PLAYWRIGHT_BASE_URL` - Can be manually set if needed

3. **Default Local URL** (fallback)
   - `https://localhost:7252` - Used when running standalone

### SSL Certificate Handling

The tests automatically trust self-signed certificates when running in development/Aspire environments.

## Running Tests from Aspire Dashboard

### 1. Start Aspire Dashboard

```bash
dotnet run --project KafkaWorkflow.AppHost
```

### 2. From the Dashboard

1. Navigate to **Resources** in the dashboard
2. Locate the **playwright** resource (listed as "KafkaWorkflow.PlaywrightTests")
3. Click the **Run** button to execute all tests
4. Monitor test progress in the resource logs

### 3. Running Specific Tests

To run specific test suites programmatically, you can use the test method filter:

```bash
dotnet test test/KafkaWorkflow.PlaywrightTests/KafkaWorkflow.PlaywrightTests.csproj \
  --filter "ClassName=AddressControllerTests"
```

## Environment Setup

When running from Aspire:

- The `ASPIRE=true` environment variable is automatically set
- The WebAPI service endpoint is injected via `SERVICES__WEBAPI__HTTP__0`
- All dependent services (SQL Server, Kafka) are automatically started and available
- The Playwright tests automatically wait for the WebAPI to be healthy before starting

## Troubleshooting

### Tests Fail with "Service not available"

**Cause**: The WebAPI hasn't started yet
**Solution**: Increase `MaxRetries` in `PlaywrightFixture.cs` or check WebAPI logs for errors

### SSL Certificate Errors

**Cause**: The certificate validation is failing
**Solution**: Ensure `IgnoreHTTPSErrors = true` is set in the browser context (already configured)

### Port Already in Use

**Cause**: Default port 7252 is already in use
**Solution**: The Aspire dashboard will automatically assign a different port

### Playwright Browser Installation

**Cause**: Chromium browser not installed
**Solution**: Run `pwsh bin/Debug/net10.0/playwright.ps1 install` or `bash bin/Debug/net10.0/playwright.sh install`

## Current Test Coverage

The following test classes are available:

- **AddressControllerTests** - CRUD operations for addresses
- **ContactControllerTests** - CRUD operations for contact information
- **PeopleControllerTests** - CRUD operations for people
- **PeopleControllerErrorHandlingTests** - Error handling scenarios
- **MultiControllerWorkflowTests** - End-to-end workflows

## Performance Notes

- Tests use a 30-second timeout for service availability (configurable)
- Each test automatically waits for network idle state before proceeding
- Tests run sequentially by default; parallel execution can be enabled in `runsettings.json`

## CI/CD Integration

When running in CI/CD pipelines with Aspire:

```bash
# Set custom base URL if needed
export PLAYWRIGHT_BASE_URL=https://api.example.com

# Run tests
dotnet test test/KafkaWorkflow.PlaywrightTests/KafkaWorkflow.PlaywrightTests.csproj
```

## See Also

- [PlaywrightFixture.cs](./PlaywrightFixture.cs) - Main test fixture implementation
- [AppHost.cs](../../KafkaWorkflow.AppHost/AppHost.cs) - Aspire configuration
- [Microsoft Playwright Documentation](https://playwright.dev/dotnet/)
