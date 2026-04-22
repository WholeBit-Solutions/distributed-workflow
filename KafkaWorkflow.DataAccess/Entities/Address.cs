namespace KafkaWorkflow.WebApi.Db.Entities
{
    public class Address
    {
        public int Id { get; set; }

        public required ContactInfo ContactInfo { get; set; }

        public int ContactInfoId { get; set; }

        public required string Street { get; set; }

        public required string City { get; set; }

        public required string State { get; set; }

        public string? ZipCode { get; set; }

        public override string ToString()
        {
            return $"Address(Id: {Id}, ContactInfoId: {ContactInfoId}, Street: {Street}, City: {City}, State: {State}, ZipCode: {ZipCode})";
        }
    }
}