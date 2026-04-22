using KafkaWorkflow.WebApi.Db.Entities;

namespace KafkaWorkflow.Consumer.PeopleWorkflow
{
    public class PersonState(int PersonId)
    {
        public int PersonId { get; } = PersonId;

        public Person? Person { get; set; }
        public IEnumerable<Address>? Addresses { get; set; }
        public IEnumerable<ContactInfo>? ContactInfos { get; set; }
    }
}
