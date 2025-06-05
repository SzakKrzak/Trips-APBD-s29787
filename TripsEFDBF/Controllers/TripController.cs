using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripsEFDBF.Data;
using TripsEFDBF.Dtos;
using TripsEFDBF.Models;

namespace TripsEFDBF.Controllers;

[ApiController]
[Route("/api/trips")]
public class TripController(TripsContext data) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        var skip = (page - 1) * pageSize;

        var totalCount = await data.Trips.CountAsync();
        
        var trips = await data.Trips
            .OrderByDescending(t => t.DateFrom)
            .Skip(skip)
            .Take(pageSize)
            .Select(t => new TripGetDto
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Clients = t.ClientTrips.Select(ct => new ClientTripGetDto
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName
                }).ToList(),
                Countries = t.IdCountries.Select(c => new CountryTripGetDto
                {
                    Name = c.Name
                }).ToList()
            })
            .ToListAsync();

        
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new
        {
            pageNum = page,
            pageSize = pageSize,
            allPages = totalPages,
            trips = trips
        };

        return Ok(response);
    }

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClient([FromRoute] int idTrip,[FromBody] ClientWithTripCreateDto clientInfo)
    {
        var registerTime = DateTime.Now;
        if (await data.Clients.Where(c=>c.Pesel == clientInfo.Pesel).AnyAsync())
        {
            return Conflict($"User with pesel {clientInfo.Pesel} already exists");
        }
        // Nie ma powodu do sprawdzenia czy klient z danym peselem jest zapisany na wycieczkę ponieważ wcześniej nie chcemy dopuścić aby taki istniał.
        // Wymaganie 3.2.2 jest podzbiorem wymagania 3.2.1

        var trip = await data.Trips.Where(t => t.Name == clientInfo.TripName).FirstOrDefaultAsync();
        if (trip is null)
        {
            return NotFound($"Trip with name {clientInfo.TripName} does not exists");
        }

        if (trip.DateFrom < registerTime)
        {
            return BadRequest($"Trip [{trip.Name}] has already happened");
        }

        var client = await data.Clients.AddAsync(new Client
        {
            FirstName = clientInfo.FirstName,
            LastName = clientInfo.LastName,
            Email = clientInfo.Email,
            Pesel = clientInfo.Pesel,
            Telephone = clientInfo.Telephone
        });
        var clientTrip = await data.ClientTrips.AddAsync(new ClientTrip
        {
            IdClient = client.Entity.IdClient,
            IdTrip = trip.IdTrip,
            PaymentDate = clientInfo.PaymentDate,
            RegisteredAt = registerTime
        });
        await data.SaveChangesAsync();
        return Created();
    }
}