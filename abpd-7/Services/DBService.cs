using apbd_7.Data;
using apbd_7.Models;
using Microsoft.EntityFrameworkCore;

namespace apbd_7.Services;

public class DBService : IDBService
{
    private readonly ApbdContext _context;

    public DBService(ApbdContext context)
    {
        _context = context;
    }

    public async Task<TripResponseDTO> GetTrips(int pageNum, int pageSize)
    {
        //Calculate total number of trips and pages
        var totalTrips = await _context.Trips.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalTrips / pageSize);
        
        //Get all trips
        var trips = await _context.Trips
            .Include(t => t.IdCountries)
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation)
            .OrderByDescending(t => t.DateFrom)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TripDto
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries.Select(c => new CountryDto { Name = c.Name }).ToList(),
                Clients = t.ClientTrips.Select(ct => new ClientDto 
                { 
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName
                }).ToList()
            })
            .ToListAsync();
        
        //Return trips
        return new TripResponseDTO()
        {
            PageNum = pageNum,
            PageSize = pageSize,
            AllPages = totalPages,
            Trips = trips
        };
    }
    public async Task<int> DeleteClient(int idClient)
    {
        // Check if client exists
        var client = await _context.Clients
            .Include(c => c.ClientTrips)
            .FirstOrDefaultAsync(c => c.IdClient == idClient);
        
        if (client == null)
        {
            return -1; 
        }

        // Check if client has any trips
        if (client.ClientTrips.Count > 0)
        {
            return 0; 
        }

        // Delete client
        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
    
        return 1;
    }
    
    public async Task<AssignClientToTripResponseDTO> AssignClientToTrip(AssignClientToTripRequestDTO request)
{
    using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        // Check if client with this PESEL already exists
        var existingClient = await _context.Clients
            .FirstOrDefaultAsync(c => c.Pesel == request.Pesel);
        
        if (existingClient != null)
        {
            return new AssignClientToTripResponseDTO
            {
                Success = false,
                Message = "Client with this PESEL already exists"
            };
        }

        // Check if trip exists
        var trip = await _context.Trips
            .FirstOrDefaultAsync(t => t.IdTrip == request.IdTrip);

        if (trip == null)
        {
            return new AssignClientToTripResponseDTO
            {
                Success = false,
                Message = "Trip not found"
            };
        }

        // Check if trip is in the future
        if (trip.DateFrom <= DateTime.Now)
        {
            return new AssignClientToTripResponseDTO
            {
                Success = false,
                Message = "Cannot assign to a trip that has already started or ended"
            };
        }

        // Create new client
        var client = new Client
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Telephone = request.Telephone,
            Pesel = request.Pesel
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Check if client is already assigned to this trip
        var existingAssignment = await _context.ClientTrips
            .FirstOrDefaultAsync(ct => ct.IdClient == client.IdClient && ct.IdTrip == request.IdTrip);

        if (existingAssignment != null)
        {
            await transaction.RollbackAsync();
            return new AssignClientToTripResponseDTO
            {
                Success = false,
                Message = "Client is already assigned to this trip"
            };
        }
        
        //Create ClientTrip association and push to database
        var clientTrip = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = request.IdTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = request.PaymentDate
        };
        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        //Return response DTO
        return new AssignClientToTripResponseDTO
        {
            Success = true,
            Message = "Client successfully assigned to trip"
        };
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return new AssignClientToTripResponseDTO
        {
            Success = false,
            Message = $"An error occurred: {ex.Message}"
        };
    }
}
}