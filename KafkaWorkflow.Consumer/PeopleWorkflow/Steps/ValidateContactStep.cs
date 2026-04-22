using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.WebApi.Db;
using Microsoft.EntityFrameworkCore;

namespace KafkaWorkflow.Consumer.PeopleWorkflow.Steps
{
    public class ValidateContactStep(IMessageWorkflow<int, PersonState?> workflow, PeopleContext dbContext) : MessageWorkflowStep<int, PersonState?>(workflow)
    {
        public override Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Validating contacts...");
            if (Workflow.State!.ContactInfos == null || !Workflow.State.ContactInfos.Any())
            {
                Console.WriteLine("  No contact info found");
            }
            else
            {
                foreach (var contactInfo in Workflow.State.ContactInfos)
                {
                    Console.WriteLine($"  {contactInfo}");
                }
            }
            return Task.CompletedTask;
        }

        public override Task<bool> ShouldExecute()
        {
            var person = dbContext.Persons.Include(p => p.ContactInfos).ThenInclude(c => c.Addresses).FirstOrDefault(p => p.Id == Workflow.State!.PersonId);
            Workflow.State!.ContactInfos = person?.ContactInfos;
            
            return Task.FromResult(person?.ContactInfos != null && person.ContactInfos.Any());
        }
    }
}
