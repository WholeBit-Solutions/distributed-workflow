using KafkaWorkflow.Consumer.Base.Workflow;

namespace KafkaWorkflow.Consumer.PeopleWorkflow
{
    public interface IPersonWorkflow : IMessageWorkflow<int, PersonState?>
    {
    }

    public class PersonWorkflow : BusinessWorkflow<int, PersonState>, IPersonWorkflow
    {
        public override Task ExecuteAsync(int message, CancellationToken cancellationToken = default)
        {
            State = new PersonState(message);

            return base.ExecuteAsync(message, cancellationToken);
        }
    }
}
