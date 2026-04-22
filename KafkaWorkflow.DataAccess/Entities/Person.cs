using System.Text;

namespace KafkaWorkflow.WebApi.Db.Entities
{
    public class Person
    {
        public int Id { get; set; }

        public required string FirstName { get; set; }
        
        public required string LastName { get; set; }

        public int? Age { get; set; }

        public List<ContactInfo> ContactInfos { get; set; } = [];

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Person(Id: {Id}, FirstName: {FirstName}, LastName: {LastName}, Age: {Age})");
            sb.AppendLine("ContactInfos: ");
            foreach (var contactInfo in ContactInfos)
            {
                sb.AppendLine($"  {contactInfo}");
            }
            return sb.ToString();
        }
    }
}