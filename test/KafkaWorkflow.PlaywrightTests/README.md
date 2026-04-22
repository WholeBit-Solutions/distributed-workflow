# Playwright Integration Tests for KafkaWorkflow

This project contains integration tests for the KafkaWorkflow API using Playwright and NUnit.

## Overview

The test suite covers the `PeopleController` endpoints:
- **GET /people** - Retrieve all persons
- **POST /people** - Create a new person
- **PUT /people** - Update an existing person
- **DELETE /people/{id}** - Delete a person

## Test Categories

### PeopleControllerTests
Comprehensive tests covering:
- Basic CRUD operations
- Response validation
- Data persistence verification
- Concurrent request handling
- Full workflow testing (Create → Read → Update → Delete)

### PeopleControllerErrorHandlingTests
Error handling and validation tests:
- Invalid JSON handling
- Empty payload handling
- Content-Type validation
- Response structure validation

### PeopleControllerPerformanceTests
Performance and timing tests:
- GET endpoint response time
- POST endpoint response time
- DELETE endpoint response time
- Acceptable response time assertions (< 5 seconds)

## Prerequisites

1. **Running Application**: The WebAPI must be running on `http://localhost:5000`
2. **.NET 10+**: Required to run the tests
3. **Playwright**: Automatically installed via NuGet package

## Installation

```bash
# Restore NuGet packages
dotnet restore test/KafkaWorkflow.PlaywrightTests/KafkaWorkflow.PlaywrightTests.csproj

# Install Playwright browsers (first time only)
dotnet build test/KafkaWorkflow.PlaywrightTests/KafkaWorkflow.PlaywrightTests.csproj
```

## Running Tests

### Run All Tests
```bash
dotnet test test/KafkaWorkflow.PlaywrightTests
```

### Run Specific Test Class
```bash
dotnet test test/KafkaWorkflow.PlaywrightTests --filter "ClassName=KafkaWorkflow.PlaywrightTests.PeopleControllerTests"
```

### Run with Verbose Output
```bash
dotnet test test/KafkaWorkflow.PlaywrightTests --verbosity normal
```

### Run Specific Test
```bash
dotnet test test/KafkaWorkflow.PlaywrightTests --filter "Name=Get_Returns_AllPersons_With_ValidResponse"
```

## Test Configuration

### Base URL
The default base URL is `http://localhost:5000`. To change it, modify the `PlaywrightFixture.cs` file:

```csharp
private const string BaseUrl = "http://localhost:5000";
```

### Timeouts
- Default HTTP timeout: 30 seconds (HttpClient default)
- Network wait timeout: Configured per operation
- Performance test threshold: 5 seconds

## Test Data

Tests use timestamps to generate unique test data, avoiding conflicts:
```csharp
var timestamp = DateTime.UtcNow.Ticks;
var personId = (int)(timestamp % int.MaxValue);
```

## CI/CD Integration

Add to your CI pipeline:

```yaml
- name: Run Playwright Integration Tests
  run: |
    dotnet test test/KafkaWorkflow.PlaywrightTests \
      --logger "trx;LogFileName=playwright-tests.trx" \
      --verbosity normal
```

## Common Issues

### Tests Fail with "Connection Refused"
- **Cause**: WebAPI not running on localhost:5000
- **Solution**: Start the WebAPI before running tests

### Playwright Browser Not Found
- **Cause**: Playwright browsers not installed
- **Solution**: Run `dotnet build` first to install browsers

### Timeout Errors
- **Cause**: System is slow or under heavy load
- **Solution**: Increase timeout values in test code or check system resources

## Extending Tests

To add more tests:

1. Create new test method in appropriate test class
2. Use the existing pattern: Arrange → Act → Assert
3. Follow naming convention: `[HttpMethod]_[Operation]_[ExpectedResult]`
4. Use `PlaywrightFixture` for browser and HTTP client setup

Example:
```csharp
[Test]
public async Task Post_WithSpecificData_ReturnsExpectedResult()
{
    // Arrange
    var data = new PersonDto { /* ... */ };
    
    // Act
    var response = await client.PostAsync(endpoint, content);
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
}
```

## Dependencies

- Microsoft.Playwright 1.48.2
- Microsoft.Playwright.NUnit 1.48.2
- NUnit 4.3.2
- Microsoft.NET.Test.Sdk 17.14.0

## Notes

- Tests are independent and can run in any order
- Each test cleans up its own resources
- Performance tests may vary based on system load
- Authentication/authorization tests can be added in future versions
