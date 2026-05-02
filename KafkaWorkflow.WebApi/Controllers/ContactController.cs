using Confluent.Kafka;
using KafkaWorkflow.WebApi.Db;
using KafkaWorkflow.WebApi.Db.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KafkaWorkflow.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ContactController(PeopleContext context, IProducer<int, ContactInfo> producer) : ControllerBase
{
    [HttpGet]
    public IEnumerable<ContactInfo> Get()
    {
        return context.ContactInfos;
    }

    [HttpGet("{contactInfoId}")]
    public async Task<ContactInfo?> Get(int contactInfoId)
    {
        return await context.ContactInfos.FindAsync(contactInfoId);
    }

    [HttpPost("{personId}")]
    public async Task<IActionResult> Post(int personId, ContactInfo contactInfo)
    {
        var person = await context.Persons.Include(p => p.ContactInfos).FirstOrDefaultAsync(p => p.Id == personId);
        if (person != null)
        {
            person.ContactInfos.Add(contactInfo);

            await context.SaveChangesAsync();

            var message = new Message<int, ContactInfo> { Key = personId, Value = contactInfo };
            var publishResult = await producer.ProduceAsync("contact-topic", message);

            return Created($"/contact/{contactInfo.Id}", contactInfo);
        }
        return NotFound($"Person with id {personId} not found");
    }

    [HttpPut]
    public async Task<IActionResult> Put(ContactInfo contactInfo)
    {
        context.ContactInfos.Update(contactInfo);
        await context.SaveChangesAsync();

        var message = new Message<int, ContactInfo> { Key = contactInfo.PersonId, Value = contactInfo };
        var publishResult = await producer.ProduceAsync("contact-topic", message);

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var contactInfo = await context.ContactInfos.FindAsync(id);
        if (contactInfo == null)
        {
            return NotFound($"ContactInfo with id {id} not found");
        }

        context.ContactInfos.Remove(contactInfo);
        await context.SaveChangesAsync();

        var message = new Message<int, ContactInfo> { Key = contactInfo.PersonId, Value = contactInfo };
        var publishResult = await producer.ProduceAsync("contact-topic", message);

        return Ok();
    }
}
