
namespace ApiCoffeeTea.DTO;

public record AddressDto(
    int Id, string Line1, string City, string PostalCode, string Country, bool IsDefault
);

public class AddressCreateDto
{
    public string Line1 { get; set; } = "";
    public string City { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Country { get; set; } = "";
    public bool IsDefault { get; set; } = false;
}
