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
    public class VenuesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public VenuesControllerIntegrationTests(WebApplicationFactory<Program> factory)
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
        public async Task CreateVenue_WithValidVenue_ReturnsCreated()
        {
            // Arrange
            var newVenue = new Venue
            {
                Name = "Test Arena",
                Seats = ["A1", "A2", "A3", "B1", "B2", "B3", "C1", "C2", "C3"]
            };

            var json = JsonSerializer.Serialize(newVenue, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/venues", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.Contains("/venues/", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task CreateVenue_WithInvalidVenue_ReturnsBadRequest()
        {
            // Arrange
            var invalidVenue = new Venue
            {
                Name = "", // Empty name should be invalid
                Seats = ["A1", "A2", "A3"]
            };

            var json = JsonSerializer.Serialize(invalidVenue, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/venues", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateVenue_WithInvalidJson_ReturnsBadRequest()
        {
            // Arrange
            var invalidJson = "{ invalid json }";
            var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/venues", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateVenue_WithEmptySeatsArray_ReturnsCreated()
        {
            // Arrange
            var newVenue = new Venue
            {
                Name = "Empty Seats Arena",
                Seats = []
            };

            var json = JsonSerializer.Serialize(newVenue, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/venues", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.Contains("/venues/", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task CreateVenue_WithLargeSeatsArray_ReturnsCreated()
        {
            // Arrange
            var seats = Enumerable.Range(1, 1000)
                .Select(i => $"A{i}")
                .ToArray();

            var newVenue = new Venue
            {
                Name = "Large Arena",
                Seats = seats
            };

            var json = JsonSerializer.Serialize(newVenue, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/venues", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.Contains("/venues/", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task GetVenue_WithValidId_ReturnsVenue()
        {
            // Arrange
            var newVenue = new Venue
            {
                Name = "Get Test Venue",
                Seats = ["A1", "A2", "A3", "B1", "B2", "B3"]
            };

            // First create a venue
            var createJson = JsonSerializer.Serialize(newVenue, _jsonOptions);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/venues", createContent);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // Extract the venue ID from the location header
            var location = createResponse.Headers.Location?.ToString();
            var venueId = location?.Split('/').Last();

            // Act
            var response = await _client.GetAsync($"/venues/{venueId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var retrievedVenue = JsonSerializer.Deserialize<Venue>(responseContent, _jsonOptions);
            
            Assert.NotNull(retrievedVenue);
            Assert.Equal(newVenue.Name, retrievedVenue.Name);
            Assert.Equal(newVenue.Seats.Length, retrievedVenue.Seats.Length);
            Assert.True(newVenue.Seats.SequenceEqual(retrievedVenue.Seats));
        }

        [Fact]
        public async Task GetVenue_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/venues/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains($"Event with ID {invalidId} not found", responseContent);
        }

        [Fact]
        public async Task GetVenue_WithInvalidGuidFormat_ReturnsBadRequest()
        {
            // Arrange
            var invalidGuid = "not-a-valid-guid";

            // Act
            var response = await _client.GetAsync($"/venues/{invalidGuid}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetVenue_WithSpecificId_ReturnsExpectedVenue()
        {
            // Arrange
            var specificId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/venues/{specificId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var retrievedVenue = JsonSerializer.Deserialize<Venue>(responseContent, _jsonOptions);
            
            Assert.NotNull(retrievedVenue);
            Assert.Equal(specificId, retrievedVenue.Id);
            Assert.Equal("Default Venue", retrievedVenue.Name);
            Assert.Equal(4, retrievedVenue.Seats.Length);
            Assert.True(retrievedVenue.Seats.SequenceEqual(["A1", "A2", "B1", "B2"]));
        }

        [Fact]
        public async Task EndToEnd_VenueLifecycle_WorksCorrectly()
        {
            // Arrange
            var originalVenue = new Venue
            {
                Name = "End-to-End Test Venue",
                Seats = ["A1", "A2", "A3", "B1", "B2", "B3", "C1", "C2", "C3"]
            };

            // Act 1: Create Venue
            var createJson = JsonSerializer.Serialize(originalVenue, _jsonOptions);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/venues", createContent);
            
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            
            var location = createResponse.Headers.Location?.ToString();
            var venueId = location?.Split('/').Last();
            Assert.NotNull(venueId);

            // Act 2: Get Venue
            var getResponse = await _client.GetAsync($"/venues/{venueId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var retrievedVenue = JsonSerializer.Deserialize<Venue>(getContent, _jsonOptions);
            Assert.NotNull(retrievedVenue);
            Assert.Equal(originalVenue.Name, retrievedVenue.Name);
            Assert.True(originalVenue.Seats.SequenceEqual(retrievedVenue.Seats));
        }

        [Fact]
        public async Task CreateMultipleVenues_AllReturnCreated()
        {
            // Arrange
            var venues = new[]
            {
                new Venue { Name = "Venue 1", Seats = ["A1", "A2"] },
                new Venue { Name = "Venue 2", Seats = ["B1", "B2", "B3"] },
                new Venue { Name = "Venue 3", Seats = ["C1", "C2", "C3", "C4"] }
            };

            // Act & Assert
            foreach (var venue in venues)
            {
                var json = JsonSerializer.Serialize(venue, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("/venues", content);

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.NotNull(response.Headers.Location);
                Assert.Contains("/venues/", response.Headers.Location.ToString());
            }
        }

        [Fact]
        public async Task CreateVenue_WithSpecialCharactersInName_ReturnsCreated()
        {
            // Arrange
            var newVenue = new Venue
            {
                Name = "Test Arena & Stadium (2024)",
                Seats = ["A1", "A2", "A3"]
            };

            var json = JsonSerializer.Serialize(newVenue, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/venues", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.Contains("/venues/", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task CreateVenue_WithDuplicateSeats_ReturnsCreated()
        {
            // Arrange
            var newVenue = new Venue
            {
                Name = "Duplicate Seats Arena",
                Seats = ["A1", "A1", "A2", "A2", "B1"]
            };

            var json = JsonSerializer.Serialize(newVenue, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/venues", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.Contains("/venues/", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task CreateVenue_WithNullSeats_ReturnsBadRequest()
        {
            // Arrange
            var json = "{\"name\":\"Test Arena\",\"seats\":null}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/venues", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateVenue_WithMissingName_ReturnsBadRequest()
        {
            // Arrange
            var json = "{\"seats\":[\"A1\",\"A2\",\"A3\"]}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/venues", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateVenue_WithMissingSeats_ReturnsBadRequest()
        {
            // Arrange
            var json = "{\"name\":\"Test Arena\"}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/venues", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAllVenues_WithMultipleVenues_ReturnsAllVenues()
        {
            // Arrange
            var venuesToCreate = new[]
            {
                new Venue { Name = "Venue Alpha", Seats = ["A1", "A2"] },
                new Venue { Name = "Venue Beta", Seats = ["B1", "B2", "B3"] },
                new Venue { Name = "Venue Gamma", Seats = ["C1", "C2", "C3", "C4"] }
            };
            var createdVenueIds = new List<Guid>();
            foreach (var venue in venuesToCreate)
            {
                var json = JsonSerializer.Serialize(venue, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("/venues", content);
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var location = response.Headers.Location?.ToString();
                Assert.NotNull(location);
                var venueId = Guid.Parse(location!.Split('/').Last());
                createdVenueIds.Add(venueId);
            }

            // Act
            var getResponse = await _client.GetAsync("/venues?page=1&pageSize=10");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var venues = JsonSerializer.Deserialize<List<Venue>>(getContent, _jsonOptions);
            Assert.NotNull(venues);

            // Assert all created venues are present
            foreach (var venue in venuesToCreate)
            {
                Assert.Contains(venues, v => v.Name == venue.Name && v.Seats.SequenceEqual(venue.Seats));
            }
        }
    }
} 