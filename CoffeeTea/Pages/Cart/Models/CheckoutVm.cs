public class CheckoutVm
{
    public List<AddressVm> Addresses { get; set; } = new();
    public int SelectedAddressId { get; set; }
    public NewAddressVm NewAddress { get; set; } = new();
}

public class NewAddressVm
{
    public string Line1 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "Россия";
    public bool IsDefault { get; set; }
}

public class AddressVm
{
    public int Id { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public override string ToString()
    {
        return $"{Line1}, {City}, {PostalCode}";
    }
}