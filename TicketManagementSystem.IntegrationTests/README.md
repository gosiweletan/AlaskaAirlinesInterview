# EventsController Integration Tests

This project was created by Cursor. It is the only part of the solution created by AI.

This project contains comprehensive integration tests for the `EventsController` in the Ticket Management System.

## Overview

The integration tests use `WebApplicationFactory<Program>` to create an in-memory test server that hosts the actual ASP.NET Core application. This allows us to test the complete HTTP request/response pipeline, including:

- HTTP routing
- Model binding
- Controller actions
- Response formatting
- Status codes
- Headers

## Test Coverage

The tests cover all HTTP methods implemented in the `EventsController`:

### POST /events
- ✅ Creating events with valid data
- ✅ Handling invalid event data
- ✅ Handling malformed JSON

### GET /events/{eventId}
- ✅ Retrieving events with valid IDs
- ✅ Handling non-existent event IDs
- ✅ Handling invalid GUID formats

### PUT /events/{eventId}
- ✅ Updating events with valid data
- ✅ Handling updates to non-existent events
- ✅ Handling malformed JSON in updates
- ✅ Handling invalid GUID formats

### End-to-End Testing
- ✅ Complete event lifecycle (Create → Read → Update → Verify)

## Running the Tests

### Using Visual Studio
1. Open the solution in Visual Studio
2. Open Test Explorer (Test → Test Explorer)
3. Run all tests or specific test methods

### Using Command Line
```bash
# Navigate to the test project directory
cd TicketManagementSystem.IntegrationTests

# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Run specific test
dotnet test --filter "CreateEvent_WithValidEvent_ReturnsCreated"
```

### Using Visual Studio Code
1. Install the .NET Core Test Explorer extension
2. Open the test project
3. Use the test explorer to run tests

## Test Structure

Each test follows the Arrange-Act-Assert pattern:

```csharp
[Fact]
public async Task TestName_Scenario_ExpectedResult()
{
    // Arrange - Set up test data and conditions
    
    // Act - Execute the action being tested
    
    // Assert - Verify the expected outcomes
}
```

## Dependencies

The test project depends on:
- `Microsoft.AspNetCore.Mvc.Testing` - For WebApplicationFactory
- `xunit` - Testing framework
- `Microsoft.NET.Test.Sdk` - Test runner
- `Microsoft.Extensions.DependencyInjection` - For service configuration
- `Microsoft.Extensions.Logging` - For logging configuration

## Notes

- Tests use `IClassFixture<WebApplicationFactory<Program>>` for efficient test setup
- JSON serialization uses camelCase naming policy to match API conventions
- Tests are designed to be independent and can run in any order
- The test server uses in-memory storage, so tests don't affect persistent data

## Troubleshooting

If tests fail:
1. Ensure the main project builds successfully
2. Check that all dependencies are restored
3. Verify that the `Program.cs` class is accessible (not internal)
4. Ensure the test project references the main project correctly 