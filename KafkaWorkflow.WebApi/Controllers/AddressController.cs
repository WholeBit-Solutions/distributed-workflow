using Confluent.Kafka;
using KafkaWorkflow.DataAccess.Enums;
using KafkaWorkflow.WebApi.Db;
using KafkaWorkflow.WebApi.Db.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KafkaWorkflow.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AddressController(PeopleContext context, IProducer<string, string> producer) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Address> Get()
    {
        return context.Addresses;
    }

    [HttpPost]
    public async Task<IActionResult> Post(int contactInfoId, Address address)
    {
        var contactInfo = await context.ContactInfos.Include(ci => ci.Person).FirstOrDefaultAsync(ci => ci.Id == contactInfoId);
        if (contactInfo != null)
        {
            contactInfo.Addresses.Add(address);
            await context.SaveChangesAsync();

            var message = new Message<string, string> { Key = contactInfo.PersonId.ToString(), Value = OperationType.Update.ToString() };
            var publishResult = await producer.ProduceAsync("people-topic", message);

            return Created($"/address/{address.Id}", address);
        }
        return NotFound($"ContactInfo with id {contactInfoId} not found");
    }

    [HttpPut]
    public async Task<IActionResult> Put(Address address)
    {
        context.Addresses.Update(address);
        await context.SaveChangesAsync();

        var personId = context.ContactInfos.Where(ci => ci.Id == address.ContactInfoId).Select(ci => ci.PersonId).FirstOrDefault();

        var message = new Message<string, string> { Key = personId.ToString(), Value = OperationType.Update.ToString() };
        var publishResult = await producer.ProduceAsync("people-topic", message);

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var address = await context.Addresses.FindAsync(id);
        if (address == null)
        {
            return NotFound($"Address with id {id} not found");
        }

        context.Addresses.Remove(address);
        await context.SaveChangesAsync();

        var personId = context.ContactInfos.Where(ci => ci.Id == address.ContactInfoId).Select(ci => ci.PersonId).FirstOrDefault();

        var message = new Message<string, string> { Key = personId.ToString(), Value = OperationType.Update.ToString() };
        var publishResult = await producer.ProduceAsync("people-topic", message);

        return Ok();
    }
}
