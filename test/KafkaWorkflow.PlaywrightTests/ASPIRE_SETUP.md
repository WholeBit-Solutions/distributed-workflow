# Running Playwright Tests with Aspire

This guide explains how to execute the Playwright integration tests within the Aspire distributed application environment.

## Prerequisites

- .NET 10.0 or later
- Docker Desktop (for Aspire containers)
- Aspire workload installed: `dotnet workload install aspire`
- Chrome/Chromium browser (Playwright handles installation)

## Architecture

The test environment leverages the full Aspire orchestration:

```
┌─────────────────────────────────────────────────────────┐
│              Aspire Dashboard                            │
├─────────────────────────────────────────────────────────┤
│  SQL Server (port 55245)                                │
│  Apache Kafka (port 9092) + Kafka UI (port 8080)       │
│  WebAPI Service (port 7252)                             │
│  Consumer Service (background)                          │
└─────────────────────────────────────────────────────────┘
         ↑
         │ Runs Tests Against
         ↓
┌─────────────────────────────────────────────────────────┐
│     Playwright Browser Testing                          │
├─────────────────────────────────────────────────────────┤
│  PeopleControllerTests                                  │
│  ContactControllerTests                                 │
│  AddressControllerTests                                 │
│  PeopleControllerErrorHandlingTests                     │
│  MultiControllerWorkflowTests                           │
└─────────────────────────────────────────────────────────┘
```

## Configuration

The `PlaywrightFixture` automatically discovers the WebAPI service endpoint:

### Service Discovery Priority

1. **Aspire Environment Variables** (highest priority)
   - `SERVICES__WEBAPI__HTTP__0` - Automatically set by Aspire when the WebApi service is running

2. **Custom Environment Variable**
   - `PLAYWRIGHT_BASE_URL` - Can be manually set for non-Aspire environments

3. **Default Local URL** (fallback)
   - `https://localhost:7252` - Used when running tests standalone

### SSL Certificate Handling

Tests automatically trust self-signed certificates in development environments:

```csharp
ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
```

## Running Tests

### Option 1: From Aspire Dashboard (Recommended)

```bash
dotnet run --project KafkaWorkflow.AppHost
```

Then:
1. Navigate to http://localhost:15251 (Aspire Dashboard)
2. Click on the **webapi** resource
3. View test results in the resource logs

### Option 2: From Command Line

```bash
# Run all tests
dotnet test test/KafkaWorkflow.PlaywrightTests/KafkaWorkflow.PlaywrightTests.csproj

# Run specific test class
dotnet test test/KafkaWorkflow.PlaywrightTests/KafkaWorkflow.PlaywrightTests.csproj \
  --filter "ClassName=PeopleControllerTests"

# Run with custom base URL
set PLAYWRIGHT_BASE_URL=https://localhost:7252
dotnet test test/KafkaWorkflow.PlaywrightTests/KafkaWorkflow.PlaywrightTests.csproj
```

### Option 3: From Visual Studio

1. Open **Test Explorer** (Test → Windows → Test Explorer)
2. Build the solution first
3. Click **Run All Tests in View**
4. Monitor progress in the test results window

## Test Classes

### PeopleController Tests
- Tests CRUD operations on `/people` endpoint
- Verifies Person entity creation, retrieval, update, and deletion
- Tests cascade deletion with related contacts and addresses

### ContactController Tests  
- Tests CRUD operations on `/contact/{personId}` endpoint
- Verifies ContactInfo creation with foreign key validation
- Tests relationship with Person entity

### AddressController Tests
- Tests CRUD operations on `/address/{contactInfoId}` endpoint
- Verifies Address creation with foreign key validation  
- Tests relationship with ContactInfo entity

### ErrorHandling Tests
- Tests 404 scenarios for non-existent resources
- Tests 400 Bad Request for invalid foreign keys
- Tests cascading delete behavior

### MultiController Workflow Tests
- Tests end-to-end workflows across all three controllers
- Verifies Person → ContactInfo → Address creation chain
- Tests Kafka event publishing and consumer processing

## Service Configuration in Aspire

The `AppHost.cs` configures three main services:

### 1. SQL Server Database
```
Port: 55245
Database: People
Connection: Data Source=localhost,55245;...
```

### 2. Apache Kafka Broker
```
Port: 9092
Topics:
  - people-topic (1 partition, replication factor 1)
  - contact-topic (1 partition, replication factor 1)
  - address-topic (1 partition, replication factor 1)
Consumer Group: people-consumer-group
```

### 3. WebAPI Service
```
Port: 5000 (HTTP) / 5001 (HTTPS)
Base URL: https://localhost:7252 (from dashboard)
Endpoints:
  - GET /people
  - GET /people/{id}
  - POST /people
  - PUT /people
  - DELETE /people/{id}
  - GET /contact
  - GET /contact/{id}
  - POST /contact/{personId}
  - PUT /contact
  - DELETE /contact/{id}
  - GET /address
  - GET /address/{id}
  - POST /address/{contactInfoId}
  - PUT /address
  - DELETE /address/{id}
```

### 4. Consumer Service
```
Processes events from three Kafka topics
Runs three workers:
  - PeopleWorker (consumes people-topic)
  - ContactWorker (consumes contact-topic)
  - AddressWorker (consumes address-topic)
All execute PersonWorkflow with three validation steps
```

## Environment Variables

When running tests via Aspire:

| Variable | Value | Set By |
|----------|-------|--------|
| `ASPIRE` | `true` | Aspire Host |
| `SERVICES__WEBAPI__HTTP__0` | `https://localhost:7252` | Aspire Resource Discovery |
| `SERVICES__DATABASE__CONNECTION__0` | SQL connection string | Aspire Resource Discovery |
| `SERVICES__KAFKA__CONNECTION__0` | Kafka endpoint | Aspire Resource Discovery |

## Test Fixture Behavior

The `PlaywrightFixture` in SetUp:

1. Discovers the WebAPI base URL (in order of priority)
2. Waits for service health endpoint to respond (up to 30 seconds)
3. Creates a Chromium browser instance
4. Creates a browser context with self-signed cert trust
5. Creates a new page
6. Navigates to Scalar API documentation

On TearDown:

1. Closes the page
2. Closes the browser context
3. Closes the browser instance

## Health Check Endpoints

Tests verify service availability by checking:

```
GET /health
GET /scalar/v1
```

Both endpoints must respond with HTTP 200 before tests proceed.

## Timeout Configuration

| Setting | Value | Purpose |
|---------|-------|---------|
| `MaxRetries` | 30 | Maximum service availability check attempts |
| `RetryDelayMs` | 1000 | Delay between health checks (1 second) |
| `WaitUntil` | `NetworkIdle` | Wait for all network activity to complete |
| Test Timeout | 30 seconds | Individual test execution timeout |

## Troubleshooting

### Issue: "Service at {url} did not become available"

**Cause**: WebAPI service didn't start in time  
**Solution**:
- Increase `MaxRetries` in `PlaywrightFixture.cs`
- Check WebAPI logs in Aspire Dashboard
- Verify SQL Server and Kafka are healthy

### Issue: SSL Certificate Validation Failed

**Cause**: Self-signed certificate not trusted  
**Solution**: Already handled by `IgnoreHTTPSErrors = true`

### Issue: Port Already in Use

**Cause**: Another service using port 7252  
**Solution**: Aspire automatically assigns alternative port; check dashboard

### Issue: Chromium Browser Not Found

**Cause**: Playwright browser not installed  
**Solution**: Run `playwright install` or let Playwright auto-install on first run

### Issue: Tests Timeout on Page Navigation

**Cause**: API is slow or returning errors  
**Solution**:
- Check WebAPI logs
- Verify database is accessible
- Check Kafka connectivity

### Issue: Foreign Key Constraint Violations

**Cause**: Tests running out of order or database state issue  
**Solution**:
- Tests should be independent
- Check test database is clean
- Run with `--no-build` to avoid rebuild issues

## Performance Considerations

- **Sequential Execution**: Tests run one at a time by default
- **Setup Cost**: ~5 seconds for browser initialization per test class
- **Network Wait**: Tests wait for `NetworkIdle` state (typically 1-2 seconds per request)
- **Database Operations**: Async operations may add 100-500ms per operation

## CI/CD Integration

### GitHub Actions Example

```yaml
- name: Run Playwright Tests
  run: |
    dotnet test test/KafkaWorkflow.PlaywrightTests/ \
      --logger "trx;LogFileName=test-results.trx" \
      --results-directory ./test-results
```

### Local Development

```bash
# With Aspire
dotnet run --project KafkaWorkflow.AppHost &
sleep 10
dotnet test test/KafkaWorkflow.PlaywrightTests/

# Standalone (without Aspire)
set PLAYWRIGHT_BASE_URL=https://localhost:7252
dotnet test test/KafkaWorkflow.PlaywrightTests/
```

## API Testing Flow

### Example: Create Person Workflow

```
1. POST /people
   Request: { firstName: "Alice", lastName: "Johnson", dateOfBirth: "1990-03-15" }
   Response: 201 Created, Location: /people/{id}

2. POST /contact/{personId}
   Request: { email: "alice@example.com", phone: "555-0100" }
   Response: 201 Created, Location: /contact/{id}

3. POST /address/{contactInfoId}
   Request: { street: "100 Main St", city: "Boston", state: "MA", postalCode: "02101" }
   Response: 201 Created, Location: /address/{id}

4. Kafka Events (Async)
   - people-topic receives Person event
   - PeopleWorker processes and triggers PersonWorkflow
   - contact-topic receives ContactInfo event
   - ContactWorker processes and triggers PersonWorkflow
   - address-topic receives Address event
   - AddressWorker processes and triggers PersonWorkflow
   - Three validation steps execute in sequence
   - State is updated in database

5. GET /people/{id}
   Response: 200 OK with full entity including contacts and addresses
```

## Related Resources

- [PlaywrightFixture.cs](./PlaywrightFixture.cs) - Test fixture implementation
- [AppHost.cs](../../KafkaWorkflow.AppHost/AppHost.cs) - Aspire configuration
- [MERMAID_ARCHITECTURE_DIAGRAMS.md](../../MERMAID_ARCHITECTURE_DIAGRAMS.md) - System diagrams
- [README.md](../../README.md) - Project overview
- [Microsoft Playwright Documentation](https://playwright.dev/dotnet/)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)

## Common Commands

```bash
# Clean rebuild
dotnet clean
dotnet build

# Run all tests with verbose output
dotnet test --verbosity=detailed

# Run single test method
dotnet test --filter "Name~TestMethodName"

# Debug test (requires IDE)
# Set breakpoint in test, then run from Test Explorer with "Debug"

# Check Aspire health
curl https://localhost:7252/health --insecure

# View Aspire dashboard
# Navigate to http://localhost:15251
```
