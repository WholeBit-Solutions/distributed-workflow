using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KafkaWorkflow.WebApi.Db;
using KafkaWorkflow.WebApi.Db.Entities;

namespace KafkaWorkflow.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AddressController(PeopleContext context, IProducer<int, Address> producer) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Address> Get()
    {
        return context.Addresses;
    }

    [HttpGet("{addressId}")]
    public async Task<Address?> Get(int addressId)
    {
        return await context.Addresses.FindAsync(addressId);
    }

    [HttpPost("{contactInfoId}")]
    public async Task<IActionResult> Post(int contactInfoId, Address address)
    {
        var contactInfo = await context.ContactInfos.Include(ci => ci.Person).FirstOrDefaultAsync(ci => ci.Id == contactInfoId);
        if (contactInfo != null)
        {
            contactInfo.Addresses.Add(address);
            await context.SaveChangesAsync();

            var message = new Message<int, Address> { Key = contactInfo.PersonId, Value = address };
            var publishResult = await producer.ProduceAsync("address-topic", message);

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

        var message = new Message<int, Address> { Key = personId, Value = address };
        var publishResult = await producer.ProduceAsync("address-topic", message);

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

        var message = new Message<int, Address> { Key = personId, Value = address };
        var publishResult = await producer.ProduceAsync("address-topic", message);

        return Ok();
    }
}
