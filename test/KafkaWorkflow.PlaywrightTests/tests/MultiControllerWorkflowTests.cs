using System.Net;
using System.Text;
using System.Text.Json;
using KafkaWorkflow.PlaywrightTests.dto;
using NUnit.Framework;

namespace KafkaWorkflow.PlaywrightTests.tests;

[TestFixture]
public class MultiControllerWorkflowTests : PlaywrightFixture
{
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Test]
    public async Task Workflow_CreatePersonAndContact_VerifyRelationship()
    {
        // Arrange
        using var client = CreateHttpClient();

        var person = new PersonDto
        {
            FirstName = "Multi",
            LastName = "Controller"
        };

        var contact = new ContactInfoDto
        {
            Email = "multi@example.com",
            Phone = "555-1111"
        };

        // Act - Create person
        var personContent = new StringContent(
            JsonSerializer.Serialize(person),
            Encoding.UTF8,
            "application/json"
        );
        var personResponse = await client.PostAsync("/people", personContent);
        Assert.That(personResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var personCreatedContent = await personResponse.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonDto>(personCreatedContent, _jsonOptions);
        Assert.That(createdPerson, Is.Not.Null);
        var personId = createdPerson!.Id;
        await Task.Delay(300);

        // Act - Create contact
        var contactContent = new StringContent(
            JsonSerializer.Serialize(contact),
            Encoding.UTF8,
            "application/json"
        );
        var contactResponse = await client.PostAsync("/contact", contactContent);
        Assert.That(contactResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var contactCreatedContent = await contactResponse.Content.ReadAsStringAsync();
        var createdContact = JsonSerializer.Deserialize<ContactInfoDto>(contactCreatedContent, _jsonOptions);
        Assert.That(createdContact, Is.Not.Null);
        var contactId = createdContact!.Id;
        await Task.Delay(300);

        // Assert - Verify both entities exist
        var peopleResponse = await client.GetAsync("/people");
        var peopleContent = await peopleResponse.Content.ReadAsStringAsync();
        var people = JsonSerializer.Deserialize<List<PersonDto>>(peopleContent, _jsonOptions);
        Assert.That(people!.Any(p => p.Id == personId), Is.True);

        var contactsResponse = await client.GetAsync("/contact");
        var contactsContent = await contactsResponse.Content.ReadAsStringAsync();
        var contacts = JsonSerializer.Deserialize<List<ContactInfoDto>>(contactsContent, _jsonOptions);
        Assert.That(contacts!.Any(c => c.Id == contactId), Is.True);
    }

    [Test]
    public async Task Workflow_CreatePersonContactAndAddress_VerifyAll()
    {
        // Arrange
        using var client = CreateHttpClient();

        var person = new PersonDto
        {
            FirstName = "Complete",
            LastName = "Workflow"
        };

        var contact = new ContactInfoDto
        {
            Email = "complete@example.com",
            Phone = "555-2222"
        };

        var address = new AddressDto
        {
            Street = "Workflow St",
            City = "Multi City",
            State = "MC",
            ContactInfoId = 0  // Will be set after contact creation
        };

        // Act - Create person
        var personContent = new StringContent(JsonSerializer.Serialize(person), Encoding.UTF8, "application/json");
        var personResponse = await client.PostAsync("/people", personContent);
        Assert.That(personResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var personCreatedContent = await personResponse.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonDto>(personCreatedContent, _jsonOptions);
        Assert.That(createdPerson, Is.Not.Null);
        var personId = createdPerson!.Id;
        await Task.Delay(200);

        // Act - Create contact
        var contactContent = new StringContent(JsonSerializer.Serialize(contact), Encoding.UTF8, "application/json");
        var contactResponse = await client.PostAsync("/contact", contactContent);
        Assert.That(contactResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var contactCreatedContent = await contactResponse.Content.ReadAsStringAsync();
        var createdContact = JsonSerializer.Deserialize<ContactInfoDto>(contactCreatedContent, _jsonOptions);
        Assert.That(createdContact, Is.Not.Null);
        var contactId = createdContact!.Id;
        await Task.Delay(200);

        // Act - Create address with the contact ID
        address.ContactInfoId = contactId;
        var addressContent = new StringContent(JsonSerializer.Serialize(address), Encoding.UTF8, "application/json");
        var addressResponse = await client.PostAsync("/address", addressContent);
        Assert.That(addressResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var addressCreatedContent = await addressResponse.Content.ReadAsStringAsync();
        var createdAddress = JsonSerializer.Deserialize<AddressDto>(addressCreatedContent, _jsonOptions);
        Assert.That(createdAddress, Is.Not.Null);
        var addressId = createdAddress!.Id;
        await Task.Delay(300);

        // Assert - Verify all exist
        var peopleResponse = await client.GetAsync("/people");
        var peopleContent = await peopleResponse.Content.ReadAsStringAsync();
        var people = JsonSerializer.Deserialize<List<PersonDto>>(peopleContent, _jsonOptions);
        Assert.That(people!.Any(p => p.Id == personId), Is.True, "Person should exist");

        var contactsResponse = await client.GetAsync("/contact");
        var contactsContent = await contactsResponse.Content.ReadAsStringAsync();
        var contacts = JsonSerializer.Deserialize<List<ContactInfoDto>>(contactsContent, _jsonOptions);
        Assert.That(contacts!.Any(c => c.Id == contactId), Is.True, "Contact should exist");

        var addressesResponse = await client.GetAsync("/address");
        var addressesContent = await addressesResponse.Content.ReadAsStringAsync();
        var addresses = JsonSerializer.Deserialize<List<AddressDto>>(addressesContent, _jsonOptions);
        Assert.That(addresses!.Any(a => a.Id == addressId), Is.True, "Address should exist");
    }

    [Test]
    public async Task Workflow_UpdateMultipleControllers()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Create initial entities
        var person = new PersonDto { FirstName = "Update", LastName = "Test" };
        var contact = new ContactInfoDto { Email = "update@example.com", Phone = "555-3333" };

        var personContent = new StringContent(JsonSerializer.Serialize(person), Encoding.UTF8, "application/json");
        var contactContent = new StringContent(JsonSerializer.Serialize(contact), Encoding.UTF8, "application/json");

        var personResponse = await client.PostAsync("/people", personContent);
        var personCreatedContent = await personResponse.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonDto>(personCreatedContent, _jsonOptions);
        Assert.That(createdPerson, Is.Not.Null);
        var personId = createdPerson!.Id;

        var contactResponse = await client.PostAsync("/contact", contactContent);
        var contactCreatedContent = await contactResponse.Content.ReadAsStringAsync();
        var createdContact = JsonSerializer.Deserialize<ContactInfoDto>(contactCreatedContent, _jsonOptions);
        Assert.That(createdContact, Is.Not.Null);
        var contactId = createdContact!.Id;
        await Task.Delay(300);

        // Act - Update both
        var updatedPerson = new PersonDto { Id = personId, FirstName = "Updated", LastName = "Person" };
        var updatedContact = new ContactInfoDto { Id = contactId, Email = "updated@example.com", Phone = "555-4444" };

        var updatePersonContent = new StringContent(JsonSerializer.Serialize(updatedPerson), Encoding.UTF8, "application/json");
        var updateContactContent = new StringContent(JsonSerializer.Serialize(updatedContact), Encoding.UTF8, "application/json");

        var personUpdateResponse = await client.PutAsync("/people", updatePersonContent);
        var contactUpdateResponse = await client.PutAsync("/contact", updateContactContent);
        await Task.Delay(300);

        // Assert
        Assert.That(personUpdateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(contactUpdateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var peopleResponse = await client.GetAsync("/people");
        var peopleContent = await peopleResponse.Content.ReadAsStringAsync();
        var people = JsonSerializer.Deserialize<List<PersonDto>>(peopleContent, _jsonOptions);
        var updatedPersonFromDb = people!.FirstOrDefault(p => p.Id == personId);
        Assert.That(updatedPersonFromDb?.FirstName, Is.EqualTo("Updated"));
    }

    [Test]
    public async Task Workflow_DeleteAndVerify()
    {
        // Arrange
        using var client = CreateHttpClient();

        var person = new PersonDto { FirstName = "Delete", LastName = "Test" };
        var contact = new ContactInfoDto { Email = "delete@example.com", Phone = "555-5555" };

        var personContent = new StringContent(JsonSerializer.Serialize(person), Encoding.UTF8, "application/json");
        var contactContent = new StringContent(JsonSerializer.Serialize(contact), Encoding.UTF8, "application/json");

        var personResponse = await client.PostAsync("/people", personContent);
        var personCreatedContent = await personResponse.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonDto>(personCreatedContent, _jsonOptions);
        Assert.That(createdPerson, Is.Not.Null);
        var personId = createdPerson!.Id;

        var contactResponse = await client.PostAsync("/contact", contactContent);
        var contactCreatedContent = await contactResponse.Content.ReadAsStringAsync();
        var createdContact = JsonSerializer.Deserialize<ContactInfoDto>(contactCreatedContent, _jsonOptions);
        Assert.That(createdContact, Is.Not.Null);
        var contactId = createdContact!.Id;
        await Task.Delay(300);

        // Act - Delete both
        var personDeleteResponse = await client.DeleteAsync($"/people/{personId}");
        var contactDeleteResponse = await client.DeleteAsync($"/contact/{contactId}");
        await Task.Delay(300);

        // Assert
        Assert.That(personDeleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(contactDeleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Verify deletions
        var peopleResponse = await client.GetAsync("/people");
        var peopleContent = await peopleResponse.Content.ReadAsStringAsync();
        var people = JsonSerializer.Deserialize<List<PersonDto>>(peopleContent, _jsonOptions);
        Assert.That(people!.Any(p => p.Id == personId), Is.False, "Person should be deleted");

        var contactsResponse = await client.GetAsync("/contact");
        var contactsContent = await contactsResponse.Content.ReadAsStringAsync();
        var contacts = JsonSerializer.Deserialize<List<ContactInfoDto>>(contactsContent, _jsonOptions);
        Assert.That(contacts!.Any(c => c.Id == contactId), Is.False, "Contact should be deleted");
    }

    [Test]
    public async Task Workflow_ConcurrentOperationsAcrossControllers()
    {
        // Arrange
        using var client = CreateHttpClient();

        var tasks = new List<Task<HttpResponseMessage>>();

        // Create 2 people
        for (int i = 0; i < 2; i++)
        {
            var person = new PersonDto
            {
                FirstName = $"Concurrent_Person_{i}",
                LastName = "Test"
            };
            var content = new StringContent(JsonSerializer.Serialize(person), Encoding.UTF8, "application/json");
            tasks.Add(client.PostAsync("/people", content));
        }

        // Create 2 contacts
        for (int i = 0; i < 2; i++)
        {
            var contact = new ContactInfoDto
            {
                Email = $"concurrent_{i}@example.com",
                Phone = $"555-{6666 + i}"
            };
            var content = new StringContent(JsonSerializer.Serialize(contact), Encoding.UTF8, "application/json");
            tasks.Add(client.PostAsync("/contact", content));
        }

        // Create 2 addresses
        for (int i = 0; i < 2; i++)
        {
            var address = new AddressDto
            {
                Street = $"Concurrent St {i}",
                City = "Concurrent City",
                State = "CC",
                ContactInfoId = 1  // Default contact ID (will use existing or first created)
            };
            var content = new StringContent(JsonSerializer.Serialize(address), Encoding.UTF8, "application/json");
            tasks.Add(client.PostAsync("/address", content));
        }

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert - All should succeed
        Assert.That(responses.Count, Is.EqualTo(6));
        Assert.That(responses, Is.All.Property("StatusCode").EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Workflow_VerifyDataConsistency()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Get initial counts
        var initialPeopleResponse = await client.GetAsync("/people");
        var initialPeopleContent = await initialPeopleResponse.Content.ReadAsStringAsync();
        var initialPeople = JsonSerializer.Deserialize<List<PersonDto>>(initialPeopleContent, _jsonOptions);
        var initialPeopleCount = initialPeople?.Count ?? 0;

        var initialContactsResponse = await client.GetAsync("/contact");
        var initialContactsContent = await initialContactsResponse.Content.ReadAsStringAsync();
        var initialContacts = JsonSerializer.Deserialize<List<ContactInfoDto>>(initialContactsContent, _jsonOptions);
        var initialContactCount = initialContacts?.Count ?? 0;

        var initialAddressesResponse = await client.GetAsync("/address");
        var initialAddressesContent = await initialAddressesResponse.Content.ReadAsStringAsync();
        var initialAddresses = JsonSerializer.Deserialize<List<AddressDto>>(initialAddressesContent, _jsonOptions);
        var initialAddressCount = initialAddresses?.Count ?? 0;

        // Act - Create entities
        var person = new PersonDto { FirstName = "Consistency", LastName = "Test" };
        var contact = new ContactInfoDto { Email = "consistency@example.com", Phone = "555-7777" };
        var address = new AddressDto { Street = "Consistency", City = "Test", State = "CT", ContactInfoId = 1 };

        var personResponse = await client.PostAsync("/people", new StringContent(JsonSerializer.Serialize(person), Encoding.UTF8, "application/json"));
        var personCreatedContent = await personResponse.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonDto>(personCreatedContent, _jsonOptions);
        Assert.That(createdPerson, Is.Not.Null);

        var contactResponse = await client.PostAsync("/contact", new StringContent(JsonSerializer.Serialize(contact), Encoding.UTF8, "application/json"));
        var contactCreatedContent = await contactResponse.Content.ReadAsStringAsync();
        var createdContact = JsonSerializer.Deserialize<ContactInfoDto>(contactCreatedContent, _jsonOptions);
        Assert.That(createdContact, Is.Not.Null);
        var contactId = createdContact!.Id;

        address.ContactInfoId = contactId;
        await client.PostAsync("/address", new StringContent(JsonSerializer.Serialize(address), Encoding.UTF8, "application/json"));
        await Task.Delay(300);

        // Assert - Verify counts increased by 1
        var finalPeopleResponse = await client.GetAsync("/people");
        var finalPeopleContent = await finalPeopleResponse.Content.ReadAsStringAsync();
        var finalPeople = JsonSerializer.Deserialize<List<PersonDto>>(finalPeopleContent, _jsonOptions);
        Assert.That(finalPeople?.Count, Is.EqualTo(initialPeopleCount + 1), "People count should increase by 1");

        var finalContactsResponse = await client.GetAsync("/contact");
        var finalContactsContent = await finalContactsResponse.Content.ReadAsStringAsync();
        var finalContacts = JsonSerializer.Deserialize<List<ContactInfoDto>>(finalContactsContent, _jsonOptions);
        Assert.That(finalContacts?.Count, Is.EqualTo(initialContactCount + 1), "Contact count should increase by 1");

        var finalAddressesResponse = await client.GetAsync("/address");
        var finalAddressesContent = await finalAddressesResponse.Content.ReadAsStringAsync();
        var finalAddresses = JsonSerializer.Deserialize<List<AddressDto>>(finalAddressesContent, _jsonOptions);
        Assert.That(finalAddresses?.Count, Is.EqualTo(initialAddressCount + 1), "Address count should increase by 1");
    }

    [Test]
    public async Task Workflow_AllEndpointsResponsive()
    {
        // Arrange
        using var client = CreateHttpClient();
        var endpoints = new[] { "/people", "/contact", "/address" };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await client.GetAsync(endpoint);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Endpoint {endpoint} should be responsive");
        }
    }

    [Test]
    public async Task Workflow_ErrorHandlingAcrossControllers()
    {
        // Arrange
        using var client = CreateHttpClient();
        var nonExistentId = int.MaxValue - 1;

        // Act - Try to delete non-existent entities
        var personDeleteResponse = await client.DeleteAsync($"/people/{nonExistentId}");
        var contactDeleteResponse = await client.DeleteAsync($"/contact/{nonExistentId}");
        var addressDeleteResponse = await client.DeleteAsync($"/address/{nonExistentId}");

        // Assert - All should return 404
        Assert.That(personDeleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "Non-existent person should return 404");
        Assert.That(contactDeleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "Non-existent contact should return 404");
        Assert.That(addressDeleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "Non-existent address should return 404");
    }

    [Test]
    public async Task Workflow_SequentialOperationsOnAllControllers()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Create sequences for each controller
        for (int iteration = 0; iteration < 2; iteration++)
        {
            // Create
            var person = new PersonDto { FirstName = $"Seq_{iteration}", LastName = "Test" };
            var contact = new ContactInfoDto { Email = $"seq_{iteration}@example.com", Phone = "555-8888" };
            var address = new AddressDto { Street = $"Seq {iteration}", City = "Test", State = "ST", ContactInfoId = 1 };

            var personCreateResponse = await client.PostAsync("/people", new StringContent(JsonSerializer.Serialize(person), Encoding.UTF8, "application/json"));
            Assert.That(personCreateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var personCreatedContent = await personCreateResponse.Content.ReadAsStringAsync();
            var createdPerson = JsonSerializer.Deserialize<PersonDto>(personCreatedContent, _jsonOptions);
            Assert.That(createdPerson, Is.Not.Null);
            var personId = createdPerson!.Id;
            await Task.Delay(100);

            var contactCreateResponse = await client.PostAsync("/contact", new StringContent(JsonSerializer.Serialize(contact), Encoding.UTF8, "application/json"));
            Assert.That(contactCreateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var contactCreatedContent = await contactCreateResponse.Content.ReadAsStringAsync();
            var createdContact = JsonSerializer.Deserialize<ContactInfoDto>(contactCreatedContent, _jsonOptions);
            Assert.That(createdContact, Is.Not.Null);
            var contactId = createdContact!.Id;
            await Task.Delay(100);

            address.ContactInfoId = contactId;
            var addressCreateResponse = await client.PostAsync("/address", new StringContent(JsonSerializer.Serialize(address), Encoding.UTF8, "application/json"));
            Assert.That(addressCreateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            await Task.Delay(100);

            // Update
            person.FirstName = $"Updated_{iteration}";
            var personUpdateResponse = await client.PutAsync("/people", new StringContent(JsonSerializer.Serialize(person), Encoding.UTF8, "application/json"));
            Assert.That(personUpdateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            await Task.Delay(100);

            // Delete
            var personDeleteResponse = await client.DeleteAsync($"/people/{personId}");
            Assert.That(personDeleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}
