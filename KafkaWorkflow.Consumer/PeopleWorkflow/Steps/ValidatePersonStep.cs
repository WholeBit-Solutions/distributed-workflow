using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.WebApi.Db;
using Microsoft.EntityFrameworkCore;

namespace KafkaWorkflow.Consumer.PeopleWorkflow.Steps
{
    public class ValidatePersonStep(IMessageWorkflow<int, PersonState?> workflow, PeopleContext dbContext) : MessageWorkflowStep<int, PersonState?>(workflow)
    {
        public override Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Validating person...");
            Console.WriteLine(Workflow.StateAccessor.Value!.Person != null
                ? $"  {Workflow.StateAccessor.Value.Person}"
                : "  No person found");

            return Task.CompletedTask;
        }

        public override async Task<bool> ShouldExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Workflow.StateAccessor.Value!.Person != null;
        }
    }
}
