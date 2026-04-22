namespace KafkaWorkflow.PlaywrightTests.dto;

public class AddressDto
{
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public int ContactInfoId { get; set; }
}
