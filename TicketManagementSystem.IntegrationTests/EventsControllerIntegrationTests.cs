using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;
using TicketManagementSystem;
using TicketManagementSystem.Models;
using Xunit;

namespace TicketManagementSystem.IntegrationTests
{
    public class EventsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public EventsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configure test services if needed
                    services.AddLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                    });
                });
            });

            _client = _factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
				PropertyNameCaseInsensitive = true
			};
        }

        [Fact]
        public async Task CreateEvent_WithValidEvent_ReturnsCreated()
        {
            // Arrange
            var newEvent = new Event
            {
                Name = "Test Concert",
                Description = "A great test concert",
                VenueId = Guid.NewGuid(),
                EventStart = DateTime.UtcNow.AddDays(30),
                EventEnd = DateTime.UtcNow.AddDays(30).AddHours(3),
                ForSaleStart = DateTime.UtcNow.AddDays(1),
                ForSaleEnd = DateTime.UtcNow.AddDays(29)
            };

            var json = JsonSerializer.Serialize(newEvent, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/events", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.Contains("/events/", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task CreateEvent_WithInvalidEvent_ReturnsBadRequest()
        {
            // Arrange
            // Create an event with invalid data (e.g., end time before start time)
            var invalidEvent = new Event
            {
                Name = "Invalid Event",
                Description = "Invalid event with end time before start time",
                VenueId = Guid.NewGuid(),
                EventStart = DateTime.UtcNow.AddDays(30),
                EventEnd = DateTime.UtcNow.AddDays(29), // End before start
                ForSaleStart = DateTime.UtcNow.AddDays(1),
                ForSaleEnd = DateTime.UtcNow.AddDays(29)
            };

            var json = JsonSerializer.Serialize(invalidEvent, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/events", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_WithInvalidJson_ReturnsBadRequest()
        {
            // Arrange
            var invalidJson = "{ invalid json }";
            var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/events", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetEvent_WithValidId_ReturnsEvent()
        {
            // Arrange
            var newEvent = new Event
            {
                Name = "Get Test Event",
                Description = "Event to test GET",
                VenueId = Guid.NewGuid(),
                EventStart = DateTime.UtcNow.AddDays(30),
                EventEnd = DateTime.UtcNow.AddDays(30).AddHours(3),
                ForSaleStart = DateTime.UtcNow.AddDays(1),
                ForSaleEnd = DateTime.UtcNow.AddDays(29)
            };

            // First create an event
            var createJson = JsonSerializer.Serialize(newEvent, _jsonOptions);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/events", createContent);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // Extract the event ID from the location header
            var location = createResponse.Headers.Location?.ToString();
            var eventId = location?.Split('/').Last();

            // Act
            var response = await _client.GetAsync($"/events/{eventId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var retrievedEvent = JsonSerializer.Deserialize<Event>(responseContent, _jsonOptions);
            
            Assert.NotNull(retrievedEvent);
            Assert.Equal(newEvent.Name, retrievedEvent.Name);
            Assert.Equal(newEvent.Description, retrievedEvent.Description);
            Assert.Equal(newEvent.VenueId, retrievedEvent.VenueId);
        }

        [Fact]
        public async Task GetEvent_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/events/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains($"Event with ID {invalidId} not found", responseContent);
        }

        [Fact]
        public async Task GetEvent_WithInvalidGuidFormat_ReturnsBadRequest()
        {
            // Arrange
            var invalidGuid = "not-a-valid-guid";

            // Act
            var response = await _client.GetAsync($"/events/{invalidGuid}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_WithValidData_ReturnsUpdatedEvent()
        {
            // Arrange
            var originalEvent = new Event
            {
                Name = "Original Event",
                Description = "Original description",
                VenueId = Guid.NewGuid(),
                EventStart = DateTime.UtcNow.AddDays(30),
                EventEnd = DateTime.UtcNow.AddDays(30).AddHours(3),
                ForSaleStart = DateTime.UtcNow.AddDays(1),
                ForSaleEnd = DateTime.UtcNow.AddDays(29)
            };

            // Create an event first
            var createJson = JsonSerializer.Serialize(originalEvent, _jsonOptions);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/events", createContent);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // Extract the event ID
            var location = createResponse.Headers.Location?.ToString();
            var eventId = location?.Split('/').Last();

            // Prepare updated event
            var updatedEvent = new Event
            {
                Name = "Updated Event",
                Description = "Updated description",
                VenueId = Guid.NewGuid(),
                EventStart = DateTime.UtcNow.AddDays(35),
                EventEnd = DateTime.UtcNow.AddDays(35).AddHours(4),
                ForSaleStart = DateTime.UtcNow.AddDays(5),
                ForSaleEnd = DateTime.UtcNow.AddDays(34)
            };

            var updateJson = JsonSerializer.Serialize(updatedEvent, _jsonOptions);
            var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/events/{eventId}", updateContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var resultEvent = JsonSerializer.Deserialize<Event>(responseContent, _jsonOptions);
            
            Assert.NotNull(resultEvent);
            Assert.Equal(updatedEvent.Name, resultEvent.Name);
            Assert.Equal(updatedEvent.Description, resultEvent.Description);
            Assert.Equal(updatedEvent.VenueId, resultEvent.VenueId);
            Assert.Equal(Guid.Parse(eventId!), resultEvent.Id);
        }

        [Fact]
        public async Task UpdateEvent_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = Guid.NewGuid();
            var updatedEvent = new Event
            {
                Name = "Updated Event",
                Description = "Updated description",
                VenueId = Guid.NewGuid(),
                EventStart = DateTime.UtcNow.AddDays(35),
                EventEnd = DateTime.UtcNow.AddDays(35).AddHours(4),
                ForSaleStart = DateTime.UtcNow.AddDays(5),
                ForSaleEnd = DateTime.UtcNow.AddDays(34)
            };

            var updateJson = JsonSerializer.Serialize(updatedEvent, _jsonOptions);
            var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/events/{invalidId}", updateContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_WithInvalidJson_ReturnsBadRequest()
        {
            // Arrange
            var validId = Guid.NewGuid();
            var invalidJson = "{ invalid json }";
            var updateContent = new StringContent(invalidJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/events/{validId}", updateContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_WithInvalidGuidFormat_ReturnsBadRequest()
        {
            // Arrange
            var invalidGuid = "not-a-valid-guid";
            var updatedEvent = new Event
            {
                Name = "Updated Event",
                Description = "Updated description",
                VenueId = Guid.NewGuid(),
                EventStart = DateTime.UtcNow.AddDays(35),
                EventEnd = DateTime.UtcNow.AddDays(35).AddHours(4),
                ForSaleStart = DateTime.UtcNow.AddDays(5),
                ForSaleEnd = DateTime.UtcNow.AddDays(34)
            };

            var updateJson = JsonSerializer.Serialize(updatedEvent, _jsonOptions);
            var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/events/{invalidGuid}", updateContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task EndToEnd_EventLifecycle_WorksCorrectly()
        {
            // Arrange
            var originalEvent = new Event
            {
                Name = "End-to-End Test Event",
                Description = "Testing complete event lifecycle",
                VenueId = Guid.NewGuid(),
                EventStart = DateTime.UtcNow.AddDays(30),
                EventEnd = DateTime.UtcNow.AddDays(30).AddHours(3),
                ForSaleStart = DateTime.UtcNow.AddDays(1),
                ForSaleEnd = DateTime.UtcNow.AddDays(29)
            };

            // Act 1: Create Event
            var createJson = JsonSerializer.Serialize(originalEvent, _jsonOptions);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/events", createContent);
            
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            
            var location = createResponse.Headers.Location?.ToString();
            var eventId = location?.Split('/').Last();
            Assert.NotNull(eventId);

            // Act 2: Get Event
            var getResponse = await _client.GetAsync($"/events/{eventId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var retrievedEvent = JsonSerializer.Deserialize<Event>(getContent, _jsonOptions);
            Assert.NotNull(retrievedEvent);
            Assert.Equal(originalEvent.Name, retrievedEvent.Name);

            // Act 3: Update Event
            var updatedEvent = new Event
            {
                Name = "Updated End-to-End Event",
                Description = "Updated lifecycle description",
                VenueId = Guid.NewGuid(),
                EventStart = DateTime.UtcNow.AddDays(35),
                EventEnd = DateTime.UtcNow.AddDays(35).AddHours(4),
                ForSaleStart = DateTime.UtcNow.AddDays(5),
                ForSaleEnd = DateTime.UtcNow.AddDays(34)
            };

            var updateJson = JsonSerializer.Serialize(updatedEvent, _jsonOptions);
            var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");
            var updateResponse = await _client.PutAsync($"/events/{eventId}", updateContent);
            
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            // Act 4: Verify Update
            var verifyResponse = await _client.GetAsync($"/events/{eventId}");
            Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
            
            var verifyContent = await verifyResponse.Content.ReadAsStringAsync();
            var finalEvent = JsonSerializer.Deserialize<Event>(verifyContent, _jsonOptions);
            
            Assert.NotNull(finalEvent);
            Assert.Equal(updatedEvent.Name, finalEvent.Name);
            Assert.Equal(updatedEvent.Description, finalEvent.Description);
            Assert.Equal(Guid.Parse(eventId), finalEvent.Id);
        }
    }
} 