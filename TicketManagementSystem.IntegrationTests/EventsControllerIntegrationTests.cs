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
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // Helper method to create a venue and return its ID
        private async Task<Guid> CreateVenueAsync()
        {
            var venue = new Venue
            {
                Name = "Test Venue",
                Seats = new[] { "A1", "A2", "A3" }
            };
            var venueJson = JsonSerializer.Serialize(venue, _jsonOptions);
            var venueContent = new StringContent(venueJson, Encoding.UTF8, "application/json");
            var venueResponse = await _client.PostAsync("/venues", venueContent);
            venueResponse.EnsureSuccessStatusCode();
            var venueId = Guid.Parse(venueResponse.Headers.Location!.ToString().Split('/').Last());
            return venueId;
        }

        [Fact]
        public async Task CreateEvent_WithValidEvent_ReturnsCreated()
        {
            // Arrange
            var venueId = await CreateVenueAsync();
            var newEvent = new Event
            {
                Name = "Test Concert",
                Description = "A great test concert",
                VenueId = venueId,
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
            var venueId = await CreateVenueAsync();
            var invalidEvent = new Event
            {
                Name = "Invalid Event",
                Description = "Invalid event with end time before start time",
                VenueId = venueId,
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
            var venueId = await CreateVenueAsync();
            var newEvent = new Event
            {
                Name = "Get Test Event",
                Description = "Event to test GET",
                VenueId = venueId,
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
            var venueId = await CreateVenueAsync();
            var originalEvent = new Event
            {
                Name = "Original Event",
                Description = "Original description",
                VenueId = venueId,
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
                VenueId = venueId,
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
        public async Task CreateTicketType_WithValidData_ReturnsCreated()
        {
            // Arrange
            var venueId = await CreateVenueAsync();
            var newEvent = new Event
            {
                Name = "Event for Ticket Types",
                Description = "Event to test ticket type creation",
                VenueId = venueId,
                EventStart = DateTime.UtcNow.AddDays(30),
                EventEnd = DateTime.UtcNow.AddDays(30).AddHours(3),
                ForSaleStart = DateTime.UtcNow.AddDays(1),
                ForSaleEnd = DateTime.UtcNow.AddDays(29)
            };

            // Create an event first
            var createEventJson = JsonSerializer.Serialize(newEvent, _jsonOptions);
            var createEventContent = new StringContent(createEventJson, Encoding.UTF8, "application/json");
            var createEventResponse = await _client.PostAsync("/events", createEventContent);
            Assert.Equal(HttpStatusCode.Created, createEventResponse.StatusCode);

            var eventLocation = createEventResponse.Headers.Location?.ToString();
            var eventId = eventLocation?.Split('/').Last();

            var newTicketType = new TicketType
            {
                Name = "VIP Ticket",
                Price = 150.00m,
                Seats = ["A1", "A2", "A3"]
            };

            var json = JsonSerializer.Serialize(newTicketType, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync($"/events/{eventId}/tickettypes", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.Contains($"/events/{eventId}/tickettypes/", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task CreateTicketType_WithInvalidEventId_ReturnsBadRequest()
        {
            // Arrange
            var invalidEventId = Guid.NewGuid();
            var newTicketType = new TicketType
            {
                Name = "VIP Ticket",
                Price = 150.00m,
                Seats = ["A1", "A2", "A3"]
            };

            var json = JsonSerializer.Serialize(newTicketType, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync($"/events/{invalidEventId}/tickettypes", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetTicketTypes_WithValidEventId_ReturnsTicketTypes()
        {
            // Arrange
            var venueId = await CreateVenueAsync();
            var newEvent = new Event
            {
                Name = "Event for Ticket Types",
                Description = "Event to test ticket type retrieval",
                VenueId = venueId,
                EventStart = DateTime.UtcNow.AddDays(30),
                EventEnd = DateTime.UtcNow.AddDays(30).AddHours(3),
                ForSaleStart = DateTime.UtcNow.AddDays(1),
                ForSaleEnd = DateTime.UtcNow.AddDays(29)
            };

            // Create an event first
            var createEventJson = JsonSerializer.Serialize(newEvent, _jsonOptions);
            var createEventContent = new StringContent(createEventJson, Encoding.UTF8, "application/json");
            var createEventResponse = await _client.PostAsync("/events", createEventContent);
            Assert.Equal(HttpStatusCode.Created, createEventResponse.StatusCode);

            var eventLocation = createEventResponse.Headers.Location?.ToString();
            var eventId = eventLocation?.Split('/').Last();

            // Create a ticket type
            var newTicketType = new TicketType
            {
                Name = "VIP Ticket",
                Price = 150.00m,
                Seats = ["A1", "A2", "A3"]
            };

            var createTicketTypeJson = JsonSerializer.Serialize(newTicketType, _jsonOptions);
            var createTicketTypeContent = new StringContent(createTicketTypeJson, Encoding.UTF8, "application/json");
            await _client.PostAsync($"/events/{eventId}/tickettypes", createTicketTypeContent);

            // Act
            var response = await _client.GetAsync($"/events/{eventId}/tickettypes");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var ticketTypes = JsonSerializer.Deserialize<List<TicketType>>(responseContent, _jsonOptions);
            
            Assert.NotNull(ticketTypes);
            Assert.NotEmpty(ticketTypes);
        }

        [Fact]
        public async Task GetTicketTypes_WithInvalidEventId_ReturnsNotFound()
        {
            // Arrange
            var invalidEventId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/events/{invalidEventId}/tickettypes");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetTicketType_WithValidIds_ReturnsTicketType()
        {
            // Arrange
            var venueId = await CreateVenueAsync();
            var newEvent = new Event
            {
                Name = "Event for Ticket Type",
                Description = "Event to test single ticket type retrieval",
                VenueId = venueId,
                EventStart = DateTime.UtcNow.AddDays(30),
                EventEnd = DateTime.UtcNow.AddDays(30).AddHours(3),
                ForSaleStart = DateTime.UtcNow.AddDays(1),
                ForSaleEnd = DateTime.UtcNow.AddDays(29)
            };

            // Create an event first
            var createEventJson = JsonSerializer.Serialize(newEvent, _jsonOptions);
            var createEventContent = new StringContent(createEventJson, Encoding.UTF8, "application/json");
            var createEventResponse = await _client.PostAsync("/events", createEventContent);
            Assert.Equal(HttpStatusCode.Created, createEventResponse.StatusCode);

            var eventLocation = createEventResponse.Headers.Location?.ToString();
            var eventId = eventLocation?.Split('/').Last();

            // Create a ticket type
            var newTicketType = new TicketType
            {
                Name = "VIP Ticket",
                Price = 150.00m,
                Seats = ["A1", "A2", "A3"]
            };

            var createTicketTypeJson = JsonSerializer.Serialize(newTicketType, _jsonOptions);
            var createTicketTypeContent = new StringContent(createTicketTypeJson, Encoding.UTF8, "application/json");
            var createTicketTypeResponse = await _client.PostAsync($"/events/{eventId}/tickettypes", createTicketTypeContent);
            Assert.Equal(HttpStatusCode.Created, createTicketTypeResponse.StatusCode);

            var ticketTypeLocation = createTicketTypeResponse.Headers.Location?.ToString();
            var ticketTypeId = ticketTypeLocation?.Split('/').Last();

            // Act
            var response = await _client.GetAsync($"/events/{eventId}/tickettypes/{ticketTypeId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var ticketType = JsonSerializer.Deserialize<TicketType>(responseContent, _jsonOptions);
            
            Assert.NotNull(ticketType);
            Assert.Equal(newTicketType.Name, ticketType.Name);
            Assert.Equal(newTicketType.Price, ticketType.Price);
        }

        [Fact]
        public async Task GetTicketType_WithInvalidIds_ReturnsNotFound()
        {
            // Arrange
            var invalidEventId = Guid.NewGuid();
            var invalidTicketTypeId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/events/{invalidEventId}/tickettypes/{invalidTicketTypeId}");

            // Assert
            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdateTicketType_WithValidData_ReturnsUpdatedTicketType()
        {
            // Arrange
            var venueId = await CreateVenueAsync();
            var newEvent = new Event
            {
                Name = "Event for Ticket Type Update",
                Description = "Event to test ticket type update",
                VenueId = venueId,
                EventStart = DateTime.UtcNow.AddDays(30),
                EventEnd = DateTime.UtcNow.AddDays(30).AddHours(3),
                ForSaleStart = DateTime.UtcNow.AddDays(1),
                ForSaleEnd = DateTime.UtcNow.AddDays(29)
            };

            // Create an event first
            var createEventJson = JsonSerializer.Serialize(newEvent, _jsonOptions);
            var createEventContent = new StringContent(createEventJson, Encoding.UTF8, "application/json");
            var createEventResponse = await _client.PostAsync("/events", createEventContent);
            Assert.Equal(HttpStatusCode.Created, createEventResponse.StatusCode);

            var eventLocation = createEventResponse.Headers.Location?.ToString();
            var eventId = eventLocation?.Split('/').Last();

            // Create a ticket type
            var originalTicketType = new TicketType
            {
                Name = "VIP Ticket",
                Price = 150.00m,
                Seats = ["A1", "A2", "A3"]
            };

            var createTicketTypeJson = JsonSerializer.Serialize(originalTicketType, _jsonOptions);
            var createTicketTypeContent = new StringContent(createTicketTypeJson, Encoding.UTF8, "application/json");
            var createTicketTypeResponse = await _client.PostAsync($"/events/{eventId}/tickettypes", createTicketTypeContent);
            Assert.Equal(HttpStatusCode.Created, createTicketTypeResponse.StatusCode);

            var ticketTypeLocation = createTicketTypeResponse.Headers.Location?.ToString();
            var ticketTypeId = ticketTypeLocation?.Split('/').Last();

            // Prepare updated ticket type
            var updatedTicketType = new TicketType
            {
                Name = "Premium VIP Ticket",
                Price = 200.00m,
                Seats = ["A1", "A2", "A3", "A4"]
            };

            var updateJson = JsonSerializer.Serialize(updatedTicketType, _jsonOptions);
            var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/events/{eventId}/tickettypes/{ticketTypeId}", updateContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var resultTicketType = JsonSerializer.Deserialize<TicketType>(responseContent, _jsonOptions);
            
            Assert.NotNull(resultTicketType);
            Assert.Equal(updatedTicketType.Name, resultTicketType.Name);
            Assert.Equal(updatedTicketType.Price, resultTicketType.Price);
            Assert.Equal(Guid.Parse(ticketTypeId!), resultTicketType.Id);
        }

        [Fact]
        public async Task UpdateTicketType_WithInvalidIds_ReturnsBadRequest()
        {
            // Arrange
            var invalidEventId = Guid.NewGuid();
            var invalidTicketTypeId = Guid.NewGuid();
            var updatedTicketType = new TicketType
            {
                Name = "Premium VIP Ticket",
                Price = 200.00m,
                Seats = ["A1", "A2", "A3", "A4"]
            };

            var updateJson = JsonSerializer.Serialize(updatedTicketType, _jsonOptions);
            var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/events/{invalidEventId}/tickettypes/{invalidTicketTypeId}", updateContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetEventTickets_WithValidEventId_ReturnsTickets()
        {
            // Arrange
            var venueId = await CreateVenueAsync();
            var newEvent = new Event
            {
                Name = "Event for Tickets",
                Description = "Event to test ticket retrieval",
                VenueId = venueId,
                EventStart = DateTime.UtcNow.AddDays(30),
                EventEnd = DateTime.UtcNow.AddDays(30).AddHours(3),
                ForSaleStart = DateTime.UtcNow.AddDays(1),
                ForSaleEnd = DateTime.UtcNow.AddDays(29)
            };

            // Create an event first
            var createEventJson = JsonSerializer.Serialize(newEvent, _jsonOptions);
            var createEventContent = new StringContent(createEventJson, Encoding.UTF8, "application/json");
            var createEventResponse = await _client.PostAsync("/events", createEventContent);
            Assert.Equal(HttpStatusCode.Created, createEventResponse.StatusCode);

            var eventLocation = createEventResponse.Headers.Location?.ToString();
            var eventId = eventLocation?.Split('/').Last();

            // Act
            var response = await _client.GetAsync($"/events/{eventId}/tickets?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var tickets = JsonSerializer.Deserialize<List<object>>(responseContent, _jsonOptions);
            
            Assert.NotNull(tickets);
        }

        [Fact]
        public async Task GetEventTickets_WithInvalidEventId_ReturnsBadRequest()
        {
            // Arrange
            var invalidEventId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/events/{invalidEventId}/tickets?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetEventTickets_Paging_WorksCorrectly()
        {
            // Arrange
            var venueId = await CreateVenueAsync();
            var newEvent = new Event
            {
                Name = "Paging Test Event",
                Description = "Event for paging test",
                VenueId = venueId,
                EventStart = DateTime.UtcNow.AddDays(10),
                EventEnd = DateTime.UtcNow.AddDays(10).AddHours(2),
                ForSaleStart = DateTime.UtcNow.AddDays(1),
                ForSaleEnd = DateTime.UtcNow.AddDays(9)
            };
            var eventJson = JsonSerializer.Serialize(newEvent, _jsonOptions);
            var eventContent = new StringContent(eventJson, Encoding.UTF8, "application/json");
            var eventResponse = await _client.PostAsync("/events", eventContent);
            eventResponse.EnsureSuccessStatusCode();
            var eventId = eventResponse.Headers.Location!.ToString().Split('/').Last();

            // Create ticket type with 10 seats
            var ticketType = new TicketType
            {
                Name = "PagingType",
                Price = 20.00m,
                Seats = Enumerable.Range(1, 10).Select(i => $"A{i}").ToArray()
            };
            var ticketTypeJson = JsonSerializer.Serialize(ticketType, _jsonOptions);
            var ticketTypeContent = new StringContent(ticketTypeJson, Encoding.UTF8, "application/json");
            var ticketTypeResponse = await _client.PostAsync($"/events/{eventId}/tickettypes", ticketTypeContent);
            ticketTypeResponse.EnsureSuccessStatusCode();

            // Act - Page 1
            var page1Response = await _client.GetAsync($"/events/{eventId}/tickets?page=1&pageSize=5");
            Assert.Equal(HttpStatusCode.OK, page1Response.StatusCode);
            var page1Content = await page1Response.Content.ReadAsStringAsync();
            var page1Tickets = JsonSerializer.Deserialize<List<Ticket>>(page1Content, _jsonOptions);
            Assert.NotNull(page1Tickets);
            Assert.Equal(5, page1Tickets.Count);

            // Act - Page 2
            var page2Response = await _client.GetAsync($"/events/{eventId}/tickets?page=2&pageSize=5");
            Assert.Equal(HttpStatusCode.OK, page2Response.StatusCode);
            var page2Content = await page2Response.Content.ReadAsStringAsync();
            var page2Tickets = JsonSerializer.Deserialize<List<Ticket>>(page2Content, _jsonOptions);
            Assert.NotNull(page2Tickets);
            Assert.Equal(5, page2Tickets.Count);

            // Assert - No overlap between pages
            var page1Ids = page1Tickets.Select(t => t.Id).ToHashSet();
            var page2Ids = page2Tickets.Select(t => t.Id).ToHashSet();
            Assert.Empty(page1Ids.Intersect(page2Ids));

            // Assert - All seats are present across both pages
            var allSeats = page1Tickets.Concat(page2Tickets).Select(t => t.Seat).OrderBy(s => s).ToArray();
            var expectedSeats = Enumerable.Range(1, 10).Select(i => $"A{i}").OrderBy(s => s).ToArray();
            Assert.Equal(expectedSeats, allSeats);
        }

        [Fact]
        public async Task GetEventTickets_FilterByStatus_AvailableOnly()
        {
            // Arrange
            var venueId = await CreateVenueAsync();
            var newEvent = new Event
            {
                Name = "Status Filter Event",
                Description = "Event for status filter test",
                VenueId = venueId,
                EventStart = DateTime.UtcNow.AddDays(10),
                EventEnd = DateTime.UtcNow.AddDays(10).AddHours(2),
                ForSaleStart = DateTime.UtcNow.AddDays(1),
                ForSaleEnd = DateTime.UtcNow.AddDays(9)
            };
            var eventJson = JsonSerializer.Serialize(newEvent, _jsonOptions);
            var eventContent = new StringContent(eventJson, Encoding.UTF8, "application/json");
            var eventResponse = await _client.PostAsync("/events", eventContent);
            eventResponse.EnsureSuccessStatusCode();
            var eventId = eventResponse.Headers.Location!.ToString().Split('/').Last();

            // Create ticket type with 6 seats
            var ticketType = new TicketType
            {
                Name = "StatusType",
                Price = 30.00m,
                Seats = Enumerable.Range(1, 6).Select(i => $"A{i}").ToArray()
            };
            var ticketTypeJson = JsonSerializer.Serialize(ticketType, _jsonOptions);
            var ticketTypeContent = new StringContent(ticketTypeJson, Encoding.UTF8, "application/json");
            var ticketTypeResponse = await _client.PostAsync($"/events/{eventId}/tickettypes", ticketTypeContent);
            ticketTypeResponse.EnsureSuccessStatusCode();

            // Get all tickets
            var ticketsResponse = await _client.GetAsync($"/events/{eventId}/tickets?page=1&pageSize=10");
            ticketsResponse.EnsureSuccessStatusCode();
            var ticketsContent = await ticketsResponse.Content.ReadAsStringAsync();
            var tickets = JsonSerializer.Deserialize<List<Ticket>>(ticketsContent, _jsonOptions)!;
            Assert.Equal(6, tickets.Count);

            // Purchase 2 tickets
            for (int i = 0; i < 2; i++)
            {
                var purchase = new TicketPurchase
                {
                    PurchaserId = Guid.NewGuid(),
                    PurchaseToken = $"token-{i}",
                    PurchasePrice = 30.00m
                };
                var purchaseJson = JsonSerializer.Serialize(purchase, _jsonOptions);
                var purchaseContent = new StringContent(purchaseJson, Encoding.UTF8, "application/json");
                var purchaseResponse = await _client.PostAsync($"/tickets/{tickets[i].Id}/purchase", purchaseContent);
                Assert.Equal(HttpStatusCode.Created, purchaseResponse.StatusCode);
            }

            // Reserve 2 tickets
            for (int i = 2; i < 4; i++)
            {
                var reservation = new TicketReservation { UserId = Guid.NewGuid() };
                var reservationJson = JsonSerializer.Serialize(reservation, _jsonOptions);
                var reservationContent = new StringContent(reservationJson, Encoding.UTF8, "application/json");
                var reservationResponse = await _client.PostAsync($"/tickets/{tickets[i].Id}/reservations", reservationContent);
                Assert.Equal(HttpStatusCode.Created, reservationResponse.StatusCode);
            }

            // Act: Get only available tickets
            var availableResponse = await _client.GetAsync($"/events/{eventId}/tickets?ticketStatus=Available&page=1&pageSize=10");
            Assert.Equal(HttpStatusCode.OK, availableResponse.StatusCode);
            var availableContent = await availableResponse.Content.ReadAsStringAsync();
            var availableTickets = JsonSerializer.Deserialize<List<Ticket>>(availableContent, _jsonOptions)!;

            // Assert: Only the last 2 tickets are available
            Assert.Equal(2, availableTickets.Count);
            var availableIds = availableTickets.Select(t => t.Id).ToHashSet();
            Assert.Contains(tickets[4].Id, availableIds);
            Assert.Contains(tickets[5].Id, availableIds);
            Assert.All(availableTickets, t => Assert.Equal(TicketStatus.Available, t.Status));
        }

        [Fact]
        public async Task EndToEnd_EventLifecycle_WorksCorrectly()
        {
            // Arrange
            var venueId = await CreateVenueAsync();
            var originalEvent = new Event
            {
                Name = "End-to-End Test Event",
                Description = "Testing complete event lifecycle",
                VenueId = venueId,
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
                VenueId = venueId,
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