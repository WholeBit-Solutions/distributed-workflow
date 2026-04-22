using Confluent.Kafka;
using KafkaWorkflow.DataAccess.Enums;
using KafkaWorkflow.WebApi.Db;
using KafkaWorkflow.WebApi.Db.Entities;
using Microsoft.AspNetCore.Mvc;

namespace KafkaWorkflow.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PeopleController(PeopleContext context, IProducer<string, string> producer) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Person> Get()
    {
        return context.Persons;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Person person)
    {
        context.Persons.Add(person);
        await context.SaveChangesAsync();

        var message = new Message<string, string> { Key = person.Id.ToString(), Value = OperationType.Create.ToString() };
        var publishResult = await producer.ProduceAsync("people-topic", message);

        return Created($"/people/{person.Id}", person);
    }

    [HttpPut]
    public async Task<IActionResult> Put(Person person)
    {
        context.Persons.Update(person);
        await context.SaveChangesAsync();

        var message = new Message<string, string> { Key = person.Id.ToString(), Value = OperationType.Update.ToString() };
        var publishResult = await producer.ProduceAsync("people-topic", message);

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var person = await context.Persons.FindAsync(id);
        if (person == null)
        {
            return NotFound($"Person with id {id} not found");
        }

        context.Persons.Remove(person);
        await context.SaveChangesAsync();

        var message = new Message<string, string> { Key = person.Id.ToString(), Value = OperationType.Delete.ToString() };
        var publishResult = await producer.ProduceAsync("people-topic", message);

        return Ok();
    }
}
