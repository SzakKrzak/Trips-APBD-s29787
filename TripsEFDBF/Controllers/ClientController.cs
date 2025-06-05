using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripsEFDBF.Data;

namespace TripsEFDBF.Controllers;


[ApiController]
[Route("/api/client")]
public class ClientController(TripsContext data) : ControllerBase
{
    [HttpDelete("{idClient}")]
    public async Task<IActionResult> delete([FromRoute] int idClient)
    {
        var client = await data.Clients.FirstOrDefaultAsync(c => c.IdClient == idClient);
        
        if (client is null)
        {
            return NotFound($"Client with id {idClient} does not exists");
        }

        if (client.ClientTrips.Any())
        {
            return Conflict($"There's at least one trip assigned to client with id {idClient}");
        }

        data.Clients.Remove(client);
        await data.SaveChangesAsync();

        return NoContent();
    }
}