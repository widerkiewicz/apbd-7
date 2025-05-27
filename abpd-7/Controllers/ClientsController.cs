using Microsoft.AspNetCore.Mvc;
using apbd_7.Services;

namespace apbd_7.Controllers
{
    [Route("api/clients")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IDBService _dbService;
        
        public ClientsController(IDBService dbService)
        {
            _dbService = dbService;
        }
        
        //Delete client by id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var result = await _dbService.DeleteClient(id);
            return result switch
            {
                -1 => NotFound($"Client with ID {id} not found"),
                0 => BadRequest("Client cannot be deleted because they have assigned trips"),
                1 => NoContent(),
                _ => StatusCode(500, "Unexpected error occurred")
            };
        }
    }
}