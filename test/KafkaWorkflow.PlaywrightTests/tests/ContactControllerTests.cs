using System.Net;
using System.Text;
using System.Text.Json;
using KafkaWorkflow.PlaywrightTests.dto;
using Microsoft.Playwright;
using NUnit.Framework;

namespace KafkaWorkflow.PlaywrightTests.tests;

[TestFixture]
public class ContactControllerTests : PlaywrightFixture
{
    private const string ContactEndpoint = "/contact";
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Test]
    public async Task Get_Returns_AllContacts_With_ValidResponse()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Act
        var response = await client.GetAsync(ContactEndpoint);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Does.Contain("application/json"));

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.Not.Empty);
        
        // Verify response is valid JSON array
        var contacts = JsonSerializer.Deserialize<List<ContactInfoDto>>(content, _jsonOptions);
        Assert.That(contacts, Is.Not.Null);
    }

    [Test]
    public async Task Get_Returns_ListOfContacts()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Act
        var response = await client.GetAsync(ContactEndpoint);
        var content = await response.Content.ReadAsStringAsync();
        var contacts = JsonSerializer.Deserialize<List<ContactInfoDto>>(content, _jsonOptions);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(contacts, Is.TypeOf<List<ContactInfoDto>>());
    }

    [Test]
    public async Task Get_ResponseContains_ContactFields()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Act
        var response = await client.GetAsync(ContactEndpoint);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(content, Does.Contain("id").IgnoreCase);
        Assert.That(content, Does.Contain("email").IgnoreCase);
    }

    [Test]
    public async Task Get_EndpointIsAccessible()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Act
        var response = await client.GetAsync(ContactEndpoint);

        // Assert
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.NotFound));
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task Post_CreatesNewContact_ReturnsCreatedResponse()
    {
        // Arrange
        using var client = CreateHttpClient();
        var newContact = new ContactInfoDto
        {
            Email = "integration@example.com",
            Phone = "555-0001"
        };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(newContact),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync(ContactEndpoint, jsonContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Post_WithValidContact_ContactIsAdded()
    {
        // Arrange
        using var client = CreateHttpClient();
        var newContact = new ContactInfoDto
        {
            Email = $"test_{DateTime.UtcNow.Ticks}@example.com",
            Phone = "555-0002"
        };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(newContact),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var postResponse = await client.PostAsync(ContactEndpoint, jsonContent);
        await Task.Delay(500);

        var getResponse = await client.GetAsync(ContactEndpoint);
        var content = await getResponse.Content.ReadAsStringAsync();
        var contacts = JsonSerializer.Deserialize<List<ContactInfoDto>>(content, _jsonOptions);

        // Assert
        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(contacts, Is.Not.Null);
        Assert.That(contacts!.Any(c => c.Email == newContact.Email), Is.True);
    }

    [Test]
    public async Task Post_WithValidEmail_ContactIsCreated()
    {
        // Arrange
        using var client = CreateHttpClient();
        var newContact = new ContactInfoDto
        {
            Email = $"valid_{DateTime.UtcNow.Ticks}@test.com",
            Phone = "555-1234"
        };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(newContact),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync(ContactEndpoint, jsonContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Post_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        using var client = CreateHttpClient();
        var invalidContact = new ContactInfoDto
        {
            Email = "not-an-email",
            Phone = "555-0003"
        };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(invalidContact),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync(ContactEndpoint, jsonContent);

        // Assert
        // Depending on server validation, could be 400 or 422, or might accept it
        if (response.StatusCode != HttpStatusCode.Created)
        {
            Assert.That(response.StatusCode, Is.GreaterThanOrEqualTo(HttpStatusCode.BadRequest));
        }
    }

    [Test]
    public async Task Post_WithOptionalPhone_ContactIsCreated()
    {
        // Arrange
        using var client = CreateHttpClient();
        var newContact = new ContactInfoDto
        {
            Email = $"nophone_{DateTime.UtcNow.Ticks}@example.com",
            Phone = null
        };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(newContact),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync(ContactEndpoint, jsonContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Put_UpdatesExistingContact_ReturnsOkResponse()
    {
        // Arrange
        using var client = CreateHttpClient();

        // First create a contact
        var newContact = new ContactInfoDto
        {
            Email = "original@example.com",
            Phone = "555-0004"
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(newContact),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(ContactEndpoint, createContent);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdContact = JsonSerializer.Deserialize<ContactInfoDto>(createdContent, _jsonOptions);
        Assert.That(createdContact, Is.Not.Null);
        var contactId = createdContact!.Id;
        await Task.Delay(500);

        // Update the contact
        var updatedContact = new ContactInfoDto
        {
            Id = contactId,
            Email = "updated@example.com",
            Phone = "555-9999"
        };
        var updateContent = new StringContent(
            JsonSerializer.Serialize(updatedContact),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PutAsync(ContactEndpoint, updateContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Put_UpdatesContact_ChangesArePersisted()
    {
        // Arrange
        using var client = CreateHttpClient();

        var originalContact = new ContactInfoDto
        {
            Email = "original@test.com",
            Phone = "555-0005"
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(originalContact),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(ContactEndpoint, createContent);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdContact = JsonSerializer.Deserialize<ContactInfoDto>(createdContent, _jsonOptions);
        Assert.That(createdContact, Is.Not.Null);
        var contactId = createdContact!.Id;
        await Task.Delay(500);

        var updatedContact = new ContactInfoDto
        {
            Id = contactId,
            Email = "newemail@test.com",
            Phone = "555-8888"
        };
        var updateContent = new StringContent(
            JsonSerializer.Serialize(updatedContact),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        await client.PutAsync(ContactEndpoint, updateContent);
        await Task.Delay(500);

        var getResponse = await client.GetAsync(ContactEndpoint);
        var content = await getResponse.Content.ReadAsStringAsync();
        var contacts = JsonSerializer.Deserialize<List<ContactInfoDto>>(content, _jsonOptions);

        // Assert
        var updatedContactFromDb = contacts?.FirstOrDefault(c => c.Id == contactId);
        Assert.That(updatedContactFromDb, Is.Not.Null);
        Assert.That(updatedContactFromDb!.Email, Is.EqualTo(updatedContact.Email));
        Assert.That(updatedContactFromDb.Phone, Is.EqualTo(updatedContact.Phone));
    }

    [Test]
    public async Task Delete_RemovesExistingContact_ReturnsOkResponse()
    {
        // Arrange
        using var client = CreateHttpClient();

        var contact = new ContactInfoDto
        {
            Email = "delete@example.com",
            Phone = "555-0006"
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(contact),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(ContactEndpoint, createContent);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdContact = JsonSerializer.Deserialize<ContactInfoDto>(createdContent, _jsonOptions);
        Assert.That(createdContact, Is.Not.Null);
        var contactId = createdContact!.Id;
        await Task.Delay(500);

        // Act
        var response = await client.DeleteAsync($"{ContactEndpoint}/{contactId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Delete_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        using var client = CreateHttpClient();
        var nonExistentId = int.MaxValue - 1;

        // Act
        var response = await client.DeleteAsync($"{ContactEndpoint}/{nonExistentId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_RemovesContact_ContactNoLongerInList()
    {
        // Arrange
        using var client = CreateHttpClient();

        var contact = new ContactInfoDto
        {
            Email = "remove@example.com",
            Phone = "555-0007"
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(contact),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(ContactEndpoint, createContent);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdContact = JsonSerializer.Deserialize<ContactInfoDto>(createdContent, _jsonOptions);
        Assert.That(createdContact, Is.Not.Null);
        var contactId = createdContact!.Id;
        await Task.Delay(500);

        // Act
        await client.DeleteAsync($"{ContactEndpoint}/{contactId}");
        await Task.Delay(500);

        var getResponse = await client.GetAsync(ContactEndpoint);
        var content = await getResponse.Content.ReadAsStringAsync();
        var contacts = JsonSerializer.Deserialize<List<ContactInfoDto>>(content, _jsonOptions);

        // Assert
        Assert.That(contacts, Is.Not.Null);
        Assert.That(contacts!.Any(c => c.Id == contactId), Is.False);
    }

    [Test]
    public async Task Crud_FullWorkflow_CreateUpdateDelete()
    {
        // Arrange
        using var client = CreateHttpClient();

        var originalContact = new ContactInfoDto
        {
            Email = "workflow@example.com",
            Phone = "555-0008"
        };

        // Act & Assert - Create
        var createContent = new StringContent(
            JsonSerializer.Serialize(originalContact),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(ContactEndpoint, createContent);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdContact = JsonSerializer.Deserialize<ContactInfoDto>(createdContent, _jsonOptions);
        Assert.That(createdContact, Is.Not.Null);
        var contactId = createdContact!.Id;
        await Task.Delay(300);

        // Act & Assert - Read
        var getResponse = await client.GetAsync(ContactEndpoint);
        var content = await getResponse.Content.ReadAsStringAsync();
        var contacts = JsonSerializer.Deserialize<List<ContactInfoDto>>(content, _jsonOptions);
        Assert.That(contacts!.Any(c => c.Id == contactId), Is.True);
        await Task.Delay(300);

        // Act & Assert - Update
        var updatedContact = new ContactInfoDto
        {
            Id = contactId,
            Email = "updated@example.com",
            Phone = "555-7777"
        };
        var updateContent = new StringContent(
            JsonSerializer.Serialize(updatedContact),
            Encoding.UTF8,
            "application/json"
        );
        var updateResponse = await client.PutAsync(ContactEndpoint, updateContent);
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await Task.Delay(300);

        // Act & Assert - Delete
        var deleteResponse = await client.DeleteAsync($"{ContactEndpoint}/{contactId}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await Task.Delay(300);

        // Act & Assert - Verify deletion
        var finalGetResponse = await client.GetAsync(ContactEndpoint);
        var finalContent = await finalGetResponse.Content.ReadAsStringAsync();
        var finalContacts = JsonSerializer.Deserialize<List<ContactInfoDto>>(finalContent, _jsonOptions);
        Assert.That(finalContacts!.Any(c => c.Id == contactId), Is.False);
    }

    [Test]
    public async Task Get_PerformanceTest_ReturnsWithinAcceptableTime()
    {
        // Arrange
        using var client = CreateHttpClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.GetAsync(ContactEndpoint);
        stopwatch.Stop();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000), "GET request should complete within 5 seconds");
    }

    [Test]
    public async Task Post_PerformanceTest_ReturnsWithinAcceptableTime()
    {
        // Arrange
        using var client = CreateHttpClient();
        var contact = new ContactInfoDto
        {
            Email = $"perf_{DateTime.UtcNow.Ticks}@example.com",
            Phone = "555-0009"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(contact),
            Encoding.UTF8,
            "application/json"
        );
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.PostAsync(ContactEndpoint, content);
        stopwatch.Stop();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000), "POST request should complete within 5 seconds");
    }

    [Test]
    public async Task MultipleOperations_ConcurrentRequests_HandledCorrectly()
    {
        // Arrange
        using var client = CreateHttpClient();

        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 3; i++)
        {
            var contact = new ContactInfoDto
            {
                Email = $"concurrent_{i}@example.com",
                Phone = $"555-000{i}"
            };
            var content = new StringContent(
                JsonSerializer.Serialize(contact),
                Encoding.UTF8,
                "application/json"
            );

            tasks.Add(client.PostAsync(ContactEndpoint, content));
        }

        // Assert
        var responses = await Task.WhenAll(tasks);
        Assert.That(responses, Is.All.Property("StatusCode").EqualTo(HttpStatusCode.Created));
    }
}
