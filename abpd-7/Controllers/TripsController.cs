using apbd_7.Models;
using Microsoft.AspNetCore.Mvc;
using apbd_7.Services;

namespace apbd_7.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly IDBService _dbService;
        
        public TripsController(IDBService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Page and pageSize must be greater than 0");
            }
            var result = await _dbService.GetTrips(page, pageSize);
            return Ok(result);
        }
        
        [HttpPost("{id}/clients")]
        public async Task<IActionResult> AssignClientToTrip(int id, [FromBody] AssignClientToTripRequestDTO request)
        {
            if (request.IdTrip != id)
            {
                return BadRequest("Trip ID in URL does not match the request body");
            }
            
            var result = await _dbService.AssignClientToTrip(request);
            if (result.Success)
            {
                return Ok(result.Message);
            }
            return BadRequest(result.Message);
        }
    }
}