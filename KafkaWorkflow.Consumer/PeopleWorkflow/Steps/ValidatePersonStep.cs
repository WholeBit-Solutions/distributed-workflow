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
            Console.WriteLine(Workflow.State!.Person != null
                ? $"  {Workflow.State.Person}"
                : "  No person found");

            return Task.CompletedTask;
        }

        public override Task<bool> ShouldExecute()
        {
            var person = dbContext.Persons.FirstOrDefault(p => p.Id == Workflow.State!.PersonId);
            Workflow.State!.Person = person;
            return Task.FromResult(person != null);
        }
    }
}
