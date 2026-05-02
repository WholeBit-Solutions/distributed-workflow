using System.Text;
using System.Text.Json.Serialization;

namespace KafkaWorkflow.WebApi.Db.Entities
{
    public class ContactInfo
    {
        public int Id { get; set; }

        public int PersonId { get; set; }

        [JsonIgnore]
        public Person? Person { get; set; }

        public string? Email { get; set; }
        
        public string? Phone { get; set; }

        public List<Address> Addresses { get; set; } = [];

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ContactInfo(Id: {Id}, PersonId: {PersonId}, Email: {Email}, Phone: {Phone})");
            sb.AppendLine("Addresses: ");
            foreach (var address in Addresses)
            {
                sb.AppendLine($"  {address}");
            }
            return sb.ToString();
        }
    }
}