namespace TripsEFDBF.Dtos;

public class TripGetDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;  // poprawiona literówka
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<CountryTripGetDto> Countries { get; set; } = new List<CountryTripGetDto>();
    public List<ClientTripGetDto> Clients { get; set; } = new List<ClientTripGetDto>();
}

public class CountryTripGetDto
{
    public string Name { get; set; } = null!;
}

public class ClientTripGetDto 
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}