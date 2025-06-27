### This project was created by Cursor. It is the only part of this solution created by AI. It has not been cleaned up.

# EventsController Integration Tests

This project contains comprehensive integration tests for the `EventsController` and `VenuesController` in the Ticket Management System.

## Overview

The integration tests use `WebApplicationFactory<Program>` to create an in-memory test server that hosts the actual ASP.NET Core application. This allows us to test the complete HTTP request/response pipeline, including:

- HTTP routing
- Model binding
- Controller actions
- Response formatting
- Status codes
- Headers

## Test Coverage

### EventsController Tests

The tests cover all HTTP methods implemented in the `EventsController`:

#### POST /events
- ✅ Creating events with valid data
- ✅ Handling invalid event data
- ✅ Handling malformed JSON

#### GET /events/{eventId}
- ✅ Retrieving events with valid IDs
- ✅ Handling non-existent event IDs
- ✅ Handling invalid GUID formats

#### PUT /events/{eventId}
- ✅ Updating events with valid data
- ✅ Handling updates to non-existent events
- ✅ Handling malformed JSON in updates
- ✅ Handling invalid GUID formats

#### POST /events/{eventId}/tickettypes
- ✅ Creating ticket types with valid data
- ✅ Handling invalid event IDs
- ✅ Handling malformed JSON

#### GET /events/{eventId}/tickettypes
- ✅ Retrieving all ticket types for an event
- ✅ Handling non-existent event IDs

#### GET /events/{eventId}/tickettypes/{ticketTypeId}
- ✅ Retrieving specific ticket types
- ✅ Handling invalid event/ticket type IDs

#### PUT /events/{eventId}/tickettypes/{ticketTypeId}
- ✅ Updating ticket types with valid data
- ✅ Handling invalid event/ticket type IDs

#### GET /events/{eventId}/tickets
- ✅ Retrieving tickets for an event with pagination
- ✅ Handling invalid event IDs

#### End-to-End Testing
- ✅ Complete event lifecycle (Create → Read → Update → Verify)

### VenuesController Tests

The tests cover all HTTP methods implemented in the `VenuesController`:

#### POST /venues
- ✅ Creating venues with valid data
- ✅ Handling invalid venue data (empty name, missing required fields)
- ✅ Handling malformed JSON
- ✅ Creating venues with empty seats array
- ✅ Creating venues with large seats arrays
- ✅ Creating venues with special characters in names
- ✅ Creating venues with duplicate seats
- ✅ Handling null or missing required fields

#### GET /venues/{venueId}
- ✅ Retrieving venues with valid IDs
- ✅ Handling non-existent venue IDs
- ✅ Handling invalid GUID formats
- ✅ Testing the specific venue creation behavior in the controller

#### End-to-End Testing
- ✅ Complete venue lifecycle (Create → Read)
- ✅ Creating multiple venues

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

# Run only EventsController tests
dotnet test --filter "EventsControllerIntegrationTests"

# Run only VenuesController tests
dotnet test --filter "VenuesControllerIntegrationTests"
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
- Some tests may fail due to missing validation logic in the controllers (as identified in previous test runs)

## Known Issues

Based on previous test runs, some tests may fail due to:

1. **Missing validation logic** in controllers (e.g., event end time before start time)
2. **Persistence issues** in the in-memory data layer between requests
3. **Missing existence checks** before updates
4. **Inconsistent error handling** for invalid data

These issues highlight areas where the controllers and data layer could be improved for better robustness.

## Troubleshooting

If tests fail:
1. Ensure the main project builds successfully
2. Check that all dependencies are restored
3. Verify that the `Program.cs` class is accessible (not internal)
4. Ensure the test project references the main project correctly
5. Review the known issues section above for expected failures 