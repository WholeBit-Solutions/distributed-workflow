using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.WebApi.Db;
using Microsoft.EntityFrameworkCore;

namespace KafkaWorkflow.Consumer.PeopleWorkflow
{
    public interface IPersonWorkflow : IMessageWorkflow<int, PersonState?>
    {
    }

    public class PersonWorkflow(PeopleContext dbContext, IObjectAccessor<PersonState?> stateAccessor, IWorkflowLogger<int, PersonState?> logger) : BusinessWorkflow<int, PersonState>(stateAccessor, logger), IPersonWorkflow
    {
        public override async Task<PersonState?> OnGetStateAsync(int message, CancellationToken cancellationToken = default)
        {
            var person = await dbContext.Persons.Include(p => p.ContactInfos).ThenInclude(c => c.Addresses).FirstOrDefaultAsync(p => p.Id == message, cancellationToken);
            var state = new PersonState(message)
            {
                Person = person,
                ContactInfos = person?.ContactInfos,
                Addresses = person?.ContactInfos?.SelectMany(c => c.Addresses).ToList()
            };

            return state;
        }
    }
}
