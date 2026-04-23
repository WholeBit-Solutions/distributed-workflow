using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.WebApi.Db;

namespace KafkaWorkflow.Consumer.PeopleWorkflow.Steps
{
    public class ValidateContactStep(IMessageWorkflow<int, PersonState?> workflow, PeopleContext dbContext) : MessageWorkflowStep<int, PersonState?>(workflow)
    {
        public override Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Validating contacts...");

            foreach (var contactInfo in Workflow.StateAccessor.Value!.ContactInfos!)
            {
                Console.WriteLine($"  {contactInfo}");
            }
            return Task.CompletedTask;
        }

        public override async Task<bool> ShouldExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Workflow.StateAccessor.Value?.ContactInfos != null && Workflow.StateAccessor.Value.ContactInfos.Any();
        }
    }
}
