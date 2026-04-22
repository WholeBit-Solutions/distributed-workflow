using System.Net;
using System.Text;
using System.Text.Json;
using KafkaWorkflow.PlaywrightTests.dto;
using Microsoft.Playwright;
using NUnit.Framework;

namespace KafkaWorkflow.PlaywrightTests.tests;

[TestFixture]
public class PeopleControllerTests : PlaywrightFixture
{
    private const string PeopleEndpoint = "/people";
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Test]
    public async Task Get_Returns_AllPersons_With_ValidResponse()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Act
        var response = await client.GetAsync(PeopleEndpoint);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Does.Contain("application/json"));

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.Not.Empty);
        
        // Verify response is valid JSON array
        var persons = JsonSerializer.Deserialize<List<PersonDto>>(content, _jsonOptions);
        Assert.That(persons, Is.Not.Null);
    }

    [Test]
    public async Task Get_Returns_ListOfPersons()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Act
        var response = await client.GetAsync(PeopleEndpoint);
        var content = await response.Content.ReadAsStringAsync();
        var persons = JsonSerializer.Deserialize<List<PersonDto>>(content, _jsonOptions);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(persons, Is.TypeOf<List<PersonDto>>());
    }

    [Test]
    public async Task Get_ResponseContains_PersonFields()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Act
        var response = await client.GetAsync(PeopleEndpoint);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(content, Does.Contain("id").IgnoreCase);
        Assert.That(content, Does.Contain("firstName").IgnoreCase);
        Assert.That(content, Does.Contain("lastName").IgnoreCase);
    }

    [Test]
    public async Task Post_CreatesNewPerson_ReturnsCreatedResponse()
    {
        // Arrange
        using var client = CreateHttpClient();
        var newPerson = new PersonDto
        {
            FirstName = "Integration",
            LastName = "Test"
        };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(newPerson),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync(PeopleEndpoint, jsonContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Post_WithValidPerson_PersonIsAdded()
    {
        // Arrange
        using var client = CreateHttpClient();
        var newPerson = new PersonDto
        {
            FirstName = $"Test_{DateTime.UtcNow.Ticks}",
            LastName = "Integration"
        };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(newPerson),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var postResponse = await client.PostAsync(PeopleEndpoint, jsonContent);
        await Task.Delay(500); // Wait for message processing

        var getResponse = await client.GetAsync(PeopleEndpoint);
        var content = await getResponse.Content.ReadAsStringAsync();
        var persons = JsonSerializer.Deserialize<List<PersonDto>>(content, _jsonOptions);

        // Assert
        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(persons, Is.Not.Null);
        Assert.That(persons!.Any(p => p.FirstName == newPerson.FirstName), Is.True);
    }

    [Test]
    public async Task Post_WithMissingFirstName_StillAccepts()
    {
        // Arrange
        using var client = CreateHttpClient();
        var personData = new { lastName = "Test" };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(personData),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync(PeopleEndpoint, jsonContent);

        // Assert
        // Note: Behavior depends on model validation
        Assert.That(response.StatusCode, Is.AnyOf(HttpStatusCode.Created, HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Put_UpdatesExistingPerson_ReturnsOkResponse()
    {
        // Arrange
        using var client = CreateHttpClient();

        // First create a person
        var newPerson = new PersonDto
        {
            FirstName = "Original",
            LastName = "Test"
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(newPerson),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(PeopleEndpoint, createContent);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonDto>(createdContent, _jsonOptions);
        Assert.That(createdPerson, Is.Not.Null);
        var personId = createdPerson!.Id;
        await Task.Delay(500);

        // Update the person
        var updatedPerson = new PersonDto
        {
            Id = personId,
            FirstName = "Updated",
            LastName = "Modified"
        };
        var updateContent = new StringContent(
            JsonSerializer.Serialize(updatedPerson),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PutAsync(PeopleEndpoint, updateContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Put_UpdatesPerson_ChangesArePersisted()
    {
        // Arrange
        using var client = CreateHttpClient();

        var originalPerson = new PersonDto
        {
            FirstName = "Original",
            LastName = "Test"
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(originalPerson),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(PeopleEndpoint, createContent);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonDto>(createdContent, _jsonOptions);
        Assert.That(createdPerson, Is.Not.Null);
        var personId = createdPerson!.Id;
        await Task.Delay(500);

        var updatedPerson = new PersonDto
        {
            Id = personId,
            FirstName = "Updated",
            LastName = "NewLastName"
        };
        var updateContent = new StringContent(
            JsonSerializer.Serialize(updatedPerson),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        await client.PutAsync(PeopleEndpoint, updateContent);
        await Task.Delay(500);

        var getResponse = await client.GetAsync(PeopleEndpoint);
        var content = await getResponse.Content.ReadAsStringAsync();
        var persons = JsonSerializer.Deserialize<List<PersonDto>>(content, _jsonOptions);

        // Assert
        var updatedPersonFromDb = persons?.FirstOrDefault(p => p.Id == personId);
        Assert.That(updatedPersonFromDb, Is.Not.Null);
        Assert.That(updatedPersonFromDb!.FirstName, Is.EqualTo(updatedPerson.FirstName));
        Assert.That(updatedPersonFromDb.LastName, Is.EqualTo(updatedPerson.LastName));
    }

    [Test]
    public async Task Delete_RemovesExistingPerson_ReturnsOkResponse()
    {
        // Arrange
        using var client = CreateHttpClient();

        var person = new PersonDto
        {
            FirstName = "ToDelete",
            LastName = "Test"
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(person),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(PeopleEndpoint, createContent);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonDto>(createdContent, _jsonOptions);
        Assert.That(createdPerson, Is.Not.Null);
        var personId = createdPerson!.Id;
        await Task.Delay(500);

        // Act
        var response = await client.DeleteAsync($"{PeopleEndpoint}/{personId}");

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
        var response = await client.DeleteAsync($"{PeopleEndpoint}/{nonExistentId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_RemovesPerson_PersonNoLongerInList()
    {
        // Arrange
        using var client = CreateHttpClient();

        var person = new PersonDto
        {
            FirstName = "ToDelete",
            LastName = "Test"
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(person),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(PeopleEndpoint, createContent);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonDto>(createdContent, _jsonOptions);
        Assert.That(createdPerson, Is.Not.Null);
        var personId = createdPerson!.Id;
        await Task.Delay(500);

        // Act
        await client.DeleteAsync($"{PeopleEndpoint}/{personId}");
        await Task.Delay(500);

        var getResponse = await client.GetAsync(PeopleEndpoint);
        var content = await getResponse.Content.ReadAsStringAsync();
        var persons = JsonSerializer.Deserialize<List<PersonDto>>(content, _jsonOptions);

        // Assert
        Assert.That(persons, Is.Not.Null);
        Assert.That(persons!.Any(p => p.Id == personId), Is.False);
    }

    [Test]
    public async Task Crud_FullWorkflow_CreateUpdateDelete()
    {
        // Arrange
        using var client = CreateHttpClient();

        var originalPerson = new PersonDto
        {
            FirstName = "Workflow",
            LastName = "Start"
        };

        // Act & Assert - Create
        var createContent = new StringContent(
            JsonSerializer.Serialize(originalPerson),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(PeopleEndpoint, createContent);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonDto>(createdContent, _jsonOptions);
        Assert.That(createdPerson, Is.Not.Null);
        var personId = createdPerson!.Id;
        await Task.Delay(300);

        // Act & Assert - Read
        var getResponse = await client.GetAsync(PeopleEndpoint);
        var content = await getResponse.Content.ReadAsStringAsync();
        var persons = JsonSerializer.Deserialize<List<PersonDto>>(content, _jsonOptions);
        Assert.That(persons!.Any(p => p.Id == personId), Is.True);
        await Task.Delay(300);

        // Act & Assert - Update
        var updatedPerson = new PersonDto
        {
            Id = personId,
            FirstName = "Workflow",
            LastName = "Updated"
        };
        var updateContent = new StringContent(
            JsonSerializer.Serialize(updatedPerson),
            Encoding.UTF8,
            "application/json"
        );
        var updateResponse = await client.PutAsync(PeopleEndpoint, updateContent);
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await Task.Delay(300);

        // Act & Assert - Delete
        var deleteResponse = await client.DeleteAsync($"{PeopleEndpoint}/{personId}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await Task.Delay(300);

        // Act & Assert - Verify deletion
        var finalGetResponse = await client.GetAsync(PeopleEndpoint);
        var finalContent = await finalGetResponse.Content.ReadAsStringAsync();
        var finalPersons = JsonSerializer.Deserialize<List<PersonDto>>(finalContent, _jsonOptions);
        Assert.That(finalPersons!.Any(p => p.Id == personId), Is.False);
    }

    [Test]
    public async Task Get_EndpointIsAccessible()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Act
        var response = await client.GetAsync(PeopleEndpoint);

        // Assert
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.NotFound));
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task MultipleOperations_ConcurrentRequests_HandledCorrectly()
    {
        // Arrange
        using var client = CreateHttpClient();

        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 3; i++)
        {
            var person = new PersonDto
            {
                FirstName = $"Concurrent_{i}",
                LastName = "Test"
            };
            var content = new StringContent(
                JsonSerializer.Serialize(person),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            tasks.Add(client.PostAsync(PeopleEndpoint, content));
        }

        // Assert
        var responses = await Task.WhenAll(tasks);
        Assert.That(responses, Is.All.Property("StatusCode").EqualTo(HttpStatusCode.Created));
    }
}
