using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Models;

namespace TicketManagementSystem.Controllers
{
    [ApiController]
    [Route("Venues")]
    public class VenueController : ControllerBase
    {
        private readonly ILogger<VenueController> _logger;
        private readonly Operations _operations;

        public VenueController(ILogger<VenueController> logger)
        {
            _logger = logger;
            _operations = new Operations();
        }

        [HttpGet(Name = "GetVenue")]
		[Route("{id}")]
		public Venue GetById([FromRoute] Guid id)
        {
            return _operations.GetVenue(id);
        }

        [HttpPost(Name = "CreateVenue")]
        public Venue Create([FromBody] Venue newVenue) {
            return _operations.CreateVenue(newVenue);
        }

        //[HttpPut(Name = "UpdateVenue")]
        //[Route("{id}")]
        //public Venue Update(Guid id, Venue updatedVenue) {
        //    return updatedVenue;
        //}
    }
}
