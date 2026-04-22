# Quick Start Guide - Playwright Integration Tests

## 5-Minute Setup

### Step 1: Start the WebAPI (Terminal 1)
```bash
cd src/KafkaWorkflow
dotnet run --project KafkaWorkflow.WebApi
```

You should see: "Now listening on: http://localhost:5000"

### Step 2: Run Tests (Terminal 2)
```bash
cd src/KafkaWorkflow
dotnet test test/KafkaWorkflow.PlaywrightTests
```

### Step 3: View Results
Tests should complete in ~30-60 seconds. You'll see:
```
Test run completed. Ran X test(s). X Passed, 0 Failed
```

## Common Commands

### Run All Tests
```bash
dotnet test test/KafkaWorkflow.PlaywrightTests
```

### Run Specific Test
```bash
dotnet test test/KafkaWorkflow.PlaywrightTests --filter "Name=Get_Returns_AllPersons_With_ValidResponse"
```

### Run with Detailed Output
```bash
dotnet test test/KafkaWorkflow.PlaywrightTests --verbosity normal
```

### Run Only Performance Tests
```bash
dotnet test test/KafkaWorkflow.PlaywrightTests --filter "ClassName=KafkaWorkflow.PlaywrightTests.PeopleControllerPerformanceTests"
```

### Run Only Error Handling Tests
```bash
dotnet test test/KafkaWorkflow.PlaywrightTests --filter "ClassName=KafkaWorkflow.PlaywrightTests.PeopleControllerErrorHandlingTests"
```

## What Gets Tested

### ✅ GET /people
- [x] Returns 200 OK
- [x] Returns valid JSON
- [x] Returns list of persons
- [x] Response contains required fields
- [x] Response time < 5 seconds

### ✅ POST /people
- [x] Creates new person
- [x] Returns 201 Created
- [x] Person is saved to database
- [x] Handles invalid JSON (400 Bad Request)
- [x] Response time < 5 seconds

### ✅ PUT /people
- [x] Updates existing person
- [x] Returns 200 OK
- [x] Changes are persisted
- [x] Handles invalid JSON
- [x] Response time < 5 seconds

### ✅ DELETE /people/{id}
- [x] Removes person
- [x] Returns 200 OK
- [x] Person no longer in list
- [x] Returns 404 for non-existent ID
- [x] Response time < 5 seconds

### ✅ Additional Tests
- [x] Full CRUD workflow (Create → Read → Update → Delete)
- [x] Concurrent requests handling
- [x] Content-Type validation
- [x] Response structure validation
- [x] Error message extraction

## Project Structure

```
test/KafkaWorkflow.PlaywrightTests/
├── KafkaWorkflow.PlaywrightTests.csproj      # Project definition
├── PlaywrightFixture.cs                      # Test setup/teardown
├── TestUtilities.cs                          # Helper methods
├── PeopleControllerTests.cs                  # Main tests (28 tests)
├── PeopleControllerErrorHandlingTests.cs     # Error tests (8 tests)
├── README.md                                 # Full documentation
└── SUMMARY.md                                # Technical overview
```

## Troubleshooting

### ❌ "Connection refused" Error
**Problem**: WebAPI not running
**Solution**: 
```bash
# Terminal 1
dotnet run --project KafkaWorkflow.WebApi
```

### ❌ "Playwright not found" Error
**Problem**: Playwright not installed
**Solution**:
```bash
dotnet build test/KafkaWorkflow.PlaywrightTests
```

### ❌ Tests timeout
**Problem**: System slow or WebAPI unresponsive
**Solution**:
- Check WebAPI is running
- Check system resources
- Try running fewer tests at once

### ❌ Port 5000 already in use
**Problem**: Another process using port 5000
**Solution**:
```bash
# Find and kill process on port 5000
# Windows:
netstat -ano | findstr :5000
taskkill /PID <pid> /F

# Linux/Mac:
lsof -i :5000
kill -9 <pid>
```

## Test Results

After running tests, you'll see:
```
  Passed: 28
  Failed: 0
  Skipped: 0
  Total: 28
```

Each test follows this format:
```
✓ Get_Returns_AllPersons_With_ValidResponse
✓ Post_CreatesNewPerson_ReturnsCreatedResponse
✓ Put_UpdatesExistingPerson_ReturnsOkResponse
✓ Delete_RemovesExistingPerson_ReturnsOkResponse
...
```

## Next Steps

1. ✅ Tests are configured and ready to run
2. ✅ No additional setup required
3. ✅ Run `dotnet test` to start
4. 📚 Read `README.md` for detailed documentation
5. 🔧 Modify tests as needed for your API

## Performance Expectations

| Operation | Expected Time |
|-----------|---|
| GET all persons | < 100ms |
| POST new person | < 500ms |
| PUT update person | < 500ms |
| DELETE person | < 500ms |
| Full test suite | < 60s |

## Support

For more information:
- See `README.md` for detailed documentation
- See `SUMMARY.md` for technical details
- Check test files for specific test implementation
- Review `playwright.config.yaml` for configuration options

## Running in CI/CD

The project includes a GitHub Actions workflow example:
```bash
# Copy the workflow file to .github/workflows/
cp test/KafkaWorkflow.PlaywrightTests/.github-workflows-playwright-tests.yml \
   .github/workflows/playwright-tests.yml
```

Tests will now run automatically on:
- Push to main/develop branches
- Pull requests
- Manual trigger via Actions tab

---

**You're all set! Run `dotnet test test/KafkaWorkflow.PlaywrightTests` to start testing.**
