using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.WebApi.Db;
using Microsoft.EntityFrameworkCore;

namespace KafkaWorkflow.Consumer.PeopleWorkflow.Steps
{
    public class ValidateAddressStep(IMessageWorkflow<int, PersonState?> workflow, PeopleContext dbContext) : MessageWorkflowStep<int, PersonState?>(workflow)
    {
        public override Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Validating addresses...");
            if (Workflow.State!.Addresses == null || !Workflow.State.Addresses.Any())
            {
                Console.WriteLine("  No addresses found");
            }
            else
            {
                foreach (var address in Workflow.State!.Addresses!)
                {
                    Console.WriteLine($"  {address}");
                }
            }

            return Task.CompletedTask;
        }

        public override Task<bool> ShouldExecute()
        {
            var person = dbContext.Persons.Include(p => p.ContactInfos).FirstOrDefault(p => p.Id == Workflow.State!.PersonId);
            var contactInfoId = person?.ContactInfos?.FirstOrDefault()?.Id;
            var addresses = dbContext.Addresses.Where(a => a.ContactInfoId == contactInfoId);
            Workflow.State!.Addresses = addresses;

            return Task.FromResult(addresses.Any());
        }
    }
}
