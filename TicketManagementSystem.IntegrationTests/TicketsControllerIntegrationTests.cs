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
    public class TicketsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public TicketsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
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

        private async Task<(Guid eventId, Guid ticketTypeId, Guid ticketId)> CreateEventWithTicketAsync()
        {
            // Create event
            var newEvent = new Event
            {
                Name = "Ticket Test Event",
                Description = "Event for ticket tests",
                VenueId = Guid.NewGuid(),
                EventStart = DateTime.UtcNow.AddDays(10),
                EventEnd = DateTime.UtcNow.AddDays(10).AddHours(2),
                ForSaleStart = DateTime.UtcNow.AddDays(1),
                ForSaleEnd = DateTime.UtcNow.AddDays(9)
            };
            var eventJson = JsonSerializer.Serialize(newEvent, _jsonOptions);
            var eventContent = new StringContent(eventJson, Encoding.UTF8, "application/json");
            var eventResponse = await _client.PostAsync("/events", eventContent);
            eventResponse.EnsureSuccessStatusCode();
            var eventId = Guid.Parse(eventResponse.Headers.Location!.ToString().Split('/').Last());

            // Create ticket type
            var ticketType = new TicketType
            {
                Name = "Standard",
                Price = 50.00m,
                Seats = ["A1"]
            };
            var ticketTypeJson = JsonSerializer.Serialize(ticketType, _jsonOptions);
            var ticketTypeContent = new StringContent(ticketTypeJson, Encoding.UTF8, "application/json");
            var ticketTypeResponse = await _client.PostAsync($"/events/{eventId}/tickettypes", ticketTypeContent);
            ticketTypeResponse.EnsureSuccessStatusCode();
            var ticketTypeId = Guid.Parse(ticketTypeResponse.Headers.Location!.ToString().Split('/').Last());

            // Get ticket
            var ticketsResponse = await _client.GetAsync($"/events/{eventId}/tickets?page=1&pageSize=10");
            ticketsResponse.EnsureSuccessStatusCode();
            var ticketsContent = await ticketsResponse.Content.ReadAsStringAsync();
            var tickets = JsonSerializer.Deserialize<List<Ticket>>(ticketsContent, _jsonOptions)!;
            var ticketId = tickets.First().Id;

            return (eventId, ticketTypeId, ticketId);
        }

        [Fact]
        public async Task GetTicket_WithValidId_ReturnsTicket()
        {
            var (_, _, ticketId) = await CreateEventWithTicketAsync();
            var response = await _client.GetAsync($"/tickets/{ticketId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var ticket = JsonSerializer.Deserialize<Ticket>(content, _jsonOptions);
            Assert.NotNull(ticket);
            Assert.Equal(ticketId, ticket.Id);
        }

        [Fact]
        public async Task GetTicket_WithInvalidId_ReturnsNotFound()
        {
            var invalidId = Guid.NewGuid();
            var response = await _client.GetAsync($"/tickets/{invalidId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateTicketReservation_And_Get_And_Delete_WorksCorrectly()
        {
            var (_, _, ticketId) = await CreateEventWithTicketAsync();
            var userId = Guid.NewGuid();
            var reservation = new TicketReservation { UserId = userId };
            var json = JsonSerializer.Serialize(reservation, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            // Create reservation
            var createResponse = await _client.PostAsync($"/tickets/{ticketId}/reservations", content);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            // Get reservation
            var getResponse = await _client.GetAsync($"/tickets/{ticketId}/reservations/{userId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var getReservation = JsonSerializer.Deserialize<TicketReservation>(getContent, _jsonOptions);
            Assert.NotNull(getReservation);
            Assert.Equal(userId, getReservation.UserId);
            // Delete reservation
            var deleteResponse = await _client.DeleteAsync($"/tickets/{ticketId}/reservations/{userId}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            // Get reservation after delete
            var getAfterDelete = await _client.GetAsync($"/tickets/{ticketId}/reservations/{userId}");
            Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
        }

        [Fact]
        public async Task CreateTicketReservation_WithInvalidTicketId_ReturnsBadRequest()
        {
            var invalidTicketId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var reservation = new TicketReservation { UserId = userId };
            var json = JsonSerializer.Serialize(reservation, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"/tickets/{invalidTicketId}/reservations", content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateTicketReservation_WithMissingUserId_ReturnsBadRequest()
        {
            var (_, _, ticketId) = await CreateEventWithTicketAsync();
            var json = "{}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"/tickets/{ticketId}/reservations", content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateTicketPurchase_And_Get_WorksCorrectly()
        {
            var (_, ticketTypeId, ticketId) = await CreateEventWithTicketAsync();
            var purchaserId = Guid.NewGuid();
            var purchase = new TicketPurchase
            {
                PurchaserId = purchaserId,
                PurchaseToken = "token-123",
                PurchasePrice = 50.00m
            };
            var json = JsonSerializer.Serialize(purchase, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            // Create purchase
            var createResponse = await _client.PostAsync($"/tickets/{ticketId}/purchase", content);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            // Get purchase
            var getResponse = await _client.GetAsync($"/tickets/{ticketId}/purchase");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var getPurchase = JsonSerializer.Deserialize<TicketPurchase>(getContent, _jsonOptions);
            Assert.NotNull(getPurchase);
            Assert.Equal(purchaserId, getPurchase.PurchaserId);
            Assert.Equal(50.00m, getPurchase.PurchasePrice);
        }

        [Fact]
        public async Task CreateTicketPurchase_WithWrongPrice_ReturnsBadRequest()
        {
            var (_, ticketTypeId, ticketId) = await CreateEventWithTicketAsync();
            var purchaserId = Guid.NewGuid();
            var purchase = new TicketPurchase
            {
                PurchaserId = purchaserId,
                PurchaseToken = "token-123",
                PurchasePrice = 999.99m // Wrong price
            };
            var json = JsonSerializer.Serialize(purchase, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"/tickets/{ticketId}/purchase", content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetTicketPurchase_WithNoPurchase_ReturnsNotFound()
        {
            var (_, _, ticketId) = await CreateEventWithTicketAsync();
            var response = await _client.GetAsync($"/tickets/{ticketId}/purchase");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetTicketReservation_WithInvalidTicketId_ReturnsBadRequest()
        {
            var invalidTicketId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var response = await _client.GetAsync($"/tickets/{invalidTicketId}/reservations/{userId}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTicketReservation_WithInvalidTicketId_ReturnsBadRequest()
        {
            var invalidTicketId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var response = await _client.DeleteAsync($"/tickets/{invalidTicketId}/reservations/{userId}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
} 