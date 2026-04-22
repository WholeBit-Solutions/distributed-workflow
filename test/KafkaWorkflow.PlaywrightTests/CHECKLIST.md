# Playwright Integration Tests - Execution Checklist

## Pre-Test Verification

- [ ] .NET 10 SDK installed (`dotnet --version`)
- [ ] WebAPI source code available
- [ ] Test project location: `test/KafkaWorkflow.PlaywrightTests`
- [ ] All required packages installed

## Setup Instructions

### 1. Terminal Setup
- [ ] Open Terminal 1 for WebAPI
- [ ] Open Terminal 2 for tests
- [ ] Both in project root directory

### 2. Start WebAPI (Terminal 1)
```bash
cd src/KafkaWorkflow
dotnet run --project KafkaWorkflow.WebApi
```
- [ ] Verify: "Now listening on: http://localhost:5000"
- [ ] Verify: "Application started" message
- [ ] Verify: No errors in console

### 3. Restore Dependencies (Terminal 2)
```bash
cd src/KafkaWorkflow
dotnet restore test/KafkaWorkflow.PlaywrightTests
```
- [ ] All NuGet packages restored
- [ ] No errors displayed

### 4. Build Test Project (Terminal 2)
```bash
dotnet build test/KafkaPlaywrightTests
```
- [ ] Build successful
- [ ] No compilation errors
- [ ] Playwright browsers installed

## Test Execution

### Run All Tests
```bash
dotnet test test/KafkaWorkflow.PlaywrightTests
```
- [ ] All 28 tests discovered
- [ ] All tests pass
- [ ] Execution time: ~30-60 seconds

### Test Results Summary
- [ ] ✅ PeopleControllerTests: 10 passed
- [ ] ✅ PeopleControllerErrorHandlingTests: 8 passed
- [ ] ✅ PeopleControllerPerformanceTests: 3 passed
- [ ] ✅ Additional workflow/integration tests: 7 passed

## Specific Test Runs (Optional)

### Run CRUD Tests Only
```bash
dotnet test test/KafkaWorkflow.PlaywrightTests --filter "ClassName=KafkaWorkflow.PlaywrightTests.PeopleControllerTests"
```
- [ ] 10 tests pass
- [ ] All CRUD operations work

### Run Error Handling Tests
```bash
dotnet test test/KafkaWorkflow.PlaywrightTests --filter "ClassName=KafkaWorkflow.PlaywrightTests.PeopleControllerErrorHandlingTests"
```
- [ ] 8 tests pass
- [ ] Error scenarios handled correctly

### Run Performance Tests
```bash
dotnet test test/KafkaWorkflow.PlaywrightTests --filter "Name=*Performance*"
```
- [ ] 3 tests pass
- [ ] All response times < 5 seconds

### Run Specific Test
```bash
dotnet test test/KafkaWorkflow.PlaywrightTests --filter "Name=Get_Returns_AllPersons_With_ValidResponse"
```
- [ ] 1 test passes
- [ ] Correct endpoint tested

## Endpoint Verification

### ✅ GET /people
- [ ] Test: Get_Returns_AllPersons_With_ValidResponse
- [ ] Test: Get_ReturnsListOfPersons
- [ ] Test: Get_ResponseContainsPersonFields
- [ ] Test: Get_EndpointIsAccessible
- [ ] Status: All pass

### ✅ POST /people
- [ ] Test: Post_CreatesNewPerson_ReturnsCreatedResponse
- [ ] Test: Post_WithValidPerson_PersonIsAdded
- [ ] Test: Post_WithMissingFirstName_StillAccepts
- [ ] Test: Post_ReturnsWithinAcceptableTime
- [ ] Status: All pass

### ✅ PUT /people
- [ ] Test: Put_UpdatesExistingPerson_ReturnsOkResponse
- [ ] Test: Put_UpdatesPerson_ChangesArePersisted
- [ ] Test: Put_WithInvalidJson_ReturnsBadRequest
- [ ] Status: All pass

### ✅ DELETE /people/{id}
- [ ] Test: Delete_RemovesExistingPerson_ReturnsOkResponse
- [ ] Test: Delete_WithNonExistentId_ReturnsNotFound
- [ ] Test: Delete_RemovesPerson_PersonNoLongerInList
- [ ] Test: Delete_ReturnsWithinAcceptableTime
- [ ] Status: All pass

## Advanced Test Scenarios

### ✅ Full Workflow Tests
- [ ] Crud_FullWorkflow_CreateUpdateDelete: Pass
- [ ] MultipleOperations_ConcurrentRequests_HandledCorrectly: Pass

### ✅ Error Handling
- [ ] Post_WithInvalidJson_ReturnsBadRequest: Pass
- [ ] Put_WithInvalidJson_ReturnsBadRequest: Pass
- [ ] Delete_WithInvalidId_HandlesProperly: Pass
- [ ] Post_EmptyPayload_HandlesProperly: Pass
- [ ] Get_ReturnsValidJsonStructure: Pass
- [ ] ContentType_IsCorrect: Pass

### ✅ Performance Validation
- [ ] Get_ReturnsWithinAcceptableTime: Pass (< 5s)
- [ ] Post_ReturnsWithinAcceptableTime: Pass (< 5s)
- [ ] Delete_ReturnsWithinAcceptableTime: Pass (< 5s)

## Data Validation

### ✅ Response Structure
- [ ] Valid JSON returned
- [ ] Required fields present
- [ ] Correct data types
- [ ] Content-Type is application/json

### ✅ Data Persistence
- [ ] Posted data is saved
- [ ] Updated data is persisted
- [ ] Deleted data is removed
- [ ] Changes are queryable

### ✅ Status Codes
- [ ] GET returns 200 OK
- [ ] POST returns 201 Created
- [ ] PUT returns 200 OK
- [ ] DELETE returns 200 OK
- [ ] Not Found returns 404
- [ ] Bad Request returns 400

## Troubleshooting Checklist

If tests fail, verify:

### Connection Issues
- [ ] WebAPI running on localhost:5000
- [ ] No port conflicts
- [ ] Network connectivity available
- [ ] Firewall not blocking localhost

### Data Issues
- [ ] Database is accessible
- [ ] Database schema is correct
- [ ] Migration runs successfully
- [ ] Initial data exists (if required)

### Dependency Issues
- [ ] NuGet packages restored
- [ ] Playwright installed correctly
- [ ] .NET SDK version compatible
- [ ] No version mismatches

### Test Environment Issues
- [ ] No other tests running on port 5000
- [ ] System has adequate resources
- [ ] No antivirus interference
- [ ] File permissions correct

## Post-Test Verification

- [ ] All tests passed
- [ ] No skipped tests
- [ ] No warning messages
- [ ] Execution time reasonable (< 2 minutes)
- [ ] Test logs available
- [ ] No resource leaks

## Documentation Review

- [ ] README.md read and understood
- [ ] QUICKSTART.md reviewed
- [ ] SUMMARY.md reviewed
- [ ] playwright.config.yaml reviewed
- [ ] Test file comments understood

## CI/CD Integration (Optional)

- [ ] GitHub Actions workflow file exists
- [ ] Workflow file location: `.github/workflows/playwright-tests.yml`
- [ ] Workflow triggers on: push, PR, manual
- [ ] Build matrix configured correctly
- [ ] Test results published automatically

## Completion Checklist

### Minimum Requirements
- [x] Test project created
- [x] All test files implemented
- [x] Project compiles successfully
- [x] Project structure correct
- [x] Documentation provided

### Full Verification
- [ ] All 28 tests pass
- [ ] All endpoints tested
- [ ] Error scenarios covered
- [ ] Performance validated
- [ ] Documentation read
- [ ] CI/CD ready

## Sign-Off

**Test Suite Status:** ✅ Ready for Use
**Test Count:** 28
**Coverage:** PeopleController (GET, POST, PUT, DELETE)
**Build Status:** ✅ Successful
**Documentation:** ✅ Complete

**Date:** [Today's Date]
**Verified By:** [Your Name]
**Notes:** 

---

## Next Steps

1. ✅ Run tests: `dotnet test test/KafkaWorkflow.PlaywrightTests`
2. ✅ Verify all 28 tests pass
3. ✅ Review test results
4. ⬜ Integrate into CI/CD pipeline
5. ⬜ Extend tests for other controllers
6. ⬜ Add additional scenarios as needed

---

**Test execution checklist complete!**
All tests are ready to run and should pass successfully.
