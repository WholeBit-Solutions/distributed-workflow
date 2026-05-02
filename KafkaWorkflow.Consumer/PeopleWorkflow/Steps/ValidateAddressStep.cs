using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.WebApi.Db;

namespace KafkaWorkflow.Consumer.PeopleWorkflow.Steps
{
    public class ValidateAddressStep(IMessageWorkflow<int, PersonState?> workflow, PeopleContext dbContext) : MessageWorkflowStep<int, PersonState?>(workflow)
    {
        public override Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Validating addresses...");
            foreach (var address in Workflow.StateAccessor.Value!.Addresses!)
            {
                Console.WriteLine($"  {address}");
            }

            return Task.CompletedTask;
        }

        public override async Task<bool> ShouldExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Workflow.StateAccessor.Value?.Addresses != null && Workflow.StateAccessor.Value.Addresses.Any();
        }
    }
}
