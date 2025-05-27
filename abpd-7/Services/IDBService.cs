using apbd_7.Models;

namespace apbd_7.Services;

public interface IDBService
{
    Task<TripResponseDTO> GetTrips(int pageNum, int pageSize);
    Task<int> DeleteClient(int idClient);
    Task<AssignClientToTripResponseDTO> AssignClientToTrip(AssignClientToTripRequestDTO request);
}