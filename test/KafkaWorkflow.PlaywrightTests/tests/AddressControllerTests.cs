using System.Net;
using System.Text;
using System.Text.Json;
using KafkaWorkflow.PlaywrightTests.dto;
using Microsoft.Playwright;
using NUnit.Framework;

namespace KafkaWorkflow.PlaywrightTests.tests;

[TestFixture]
public class AddressControllerTests : PlaywrightFixture
{
    private const string AddressEndpoint = "/address";
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Test]
    public async Task Get_Returns_AllAddresses_With_ValidResponse()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Act
        var response = await client.GetAsync(AddressEndpoint);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Does.Contain("application/json"));

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.Not.Empty);
        
        // Verify response is valid JSON array
        var addresses = JsonSerializer.Deserialize<List<AddressDto>>(content, _jsonOptions);
        Assert.That(addresses, Is.Not.Null);
    }

    [Test]
    public async Task Get_Returns_ListOfAddresses()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Act
        var response = await client.GetAsync(AddressEndpoint);
        var content = await response.Content.ReadAsStringAsync();
        var addresses = JsonSerializer.Deserialize<List<AddressDto>>(content, _jsonOptions);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(addresses, Is.TypeOf<List<AddressDto>>());
    }

    [Test]
    public async Task Get_ResponseContains_AddressFields()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Act
        var response = await client.GetAsync(AddressEndpoint);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(content, Does.Contain("id").IgnoreCase);
        Assert.That(content, Does.Contain("street").IgnoreCase);
        Assert.That(content, Does.Contain("city").IgnoreCase);
        Assert.That(content, Does.Contain("state").IgnoreCase);
    }

    [Test]
    public async Task Get_EndpointIsAccessible()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Act
        var response = await client.GetAsync(AddressEndpoint);

        // Assert
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.NotFound));
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task Post_CreatesNewAddress_ReturnsCreatedResponse()
    {
        // Arrange
        using var client = CreateHttpClient();
        var newAddress = new AddressDto
        {
            Street = "123 Test Street",
            City = "Test City",
            State = "TS",
            ContactInfoId = 1
        };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(newAddress),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync(AddressEndpoint, jsonContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Post_WithValidAddress_AddressIsAdded()
    {
        // Arrange
        using var client = CreateHttpClient();
        var newAddress = new AddressDto
        {
            Street = $"Test Street {DateTime.UtcNow.Ticks}",
            City = "Test City",
            State = "TS",
            ContactInfoId = 1
        };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(newAddress),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var postResponse = await client.PostAsync(AddressEndpoint, jsonContent);
        await Task.Delay(500);

        var getResponse = await client.GetAsync(AddressEndpoint);
        var content = await getResponse.Content.ReadAsStringAsync();
        var addresses = JsonSerializer.Deserialize<List<AddressDto>>(content, _jsonOptions);

        // Assert
        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(addresses, Is.Not.Null);
        Assert.That(addresses!.Any(a => a.Street == newAddress.Street), Is.True);
    }

    [Test]
    public async Task Post_WithRequiredFields_AddressIsCreated()
    {
        // Arrange
        using var client = CreateHttpClient();
        var newAddress = new AddressDto
        {
            Street = "456 Main Ave",
            City = "Main City",
            State = "MC",
            ContactInfoId = 2
        };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(newAddress),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync(AddressEndpoint, jsonContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Put_UpdatesExistingAddress_ReturnsOkResponse()
    {
        // Arrange
        using var client = CreateHttpClient();

        // First create an address
        var newAddress = new AddressDto
        {
            Street = "Original Street",
            City = "Original City",
            State = "OS",
            ContactInfoId = 1
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(newAddress),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(AddressEndpoint, createContent);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdAddress = JsonSerializer.Deserialize<AddressDto>(createdContent, _jsonOptions);
        Assert.That(createdAddress, Is.Not.Null);
        var addressId = createdAddress!.Id;
        await Task.Delay(500);

        // Update the address
        var updatedAddress = new AddressDto
        {
            Id = addressId,
            Street = "Updated Street",
            City = "Updated City",
            State = "US",
            ContactInfoId = 1
        };
        var updateContent = new StringContent(
            JsonSerializer.Serialize(updatedAddress),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PutAsync(AddressEndpoint, updateContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Put_UpdatesAddress_ChangesArePersisted()
    {
        // Arrange
        using var client = CreateHttpClient();

        var originalAddress = new AddressDto
        {
            Street = "Original",
            City = "Original",
            State = "OR",
            ContactInfoId = 1
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(originalAddress),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(AddressEndpoint, createContent);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdAddress = JsonSerializer.Deserialize<AddressDto>(createdContent, _jsonOptions);
        Assert.That(createdAddress, Is.Not.Null);
        var addressId = createdAddress!.Id;
        await Task.Delay(500);

        var updatedAddress = new AddressDto
        {
            Id = addressId,
            Street = "New Street Name",
            City = "New City",
            State = "NC",
            ContactInfoId = 1
        };
        var updateContent = new StringContent(
            JsonSerializer.Serialize(updatedAddress),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        await client.PutAsync(AddressEndpoint, updateContent);
        await Task.Delay(500);

        var getResponse = await client.GetAsync(AddressEndpoint);
        var content = await getResponse.Content.ReadAsStringAsync();
        var addresses = JsonSerializer.Deserialize<List<AddressDto>>(content, _jsonOptions);

        // Assert
        var updatedAddressFromDb = addresses?.FirstOrDefault(a => a.Id == addressId);
        Assert.That(updatedAddressFromDb, Is.Not.Null);
        Assert.That(updatedAddressFromDb!.Street, Is.EqualTo(updatedAddress.Street));
        Assert.That(updatedAddressFromDb.City, Is.EqualTo(updatedAddress.City));
    }

    [Test]
    public async Task Delete_RemovesExistingAddress_ReturnsOkResponse()
    {
        // Arrange
        using var client = CreateHttpClient();

        var address = new AddressDto
        {
            Street = "Delete Test",
            City = "Delete City",
            State = "DC",
            ContactInfoId = 1
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(address),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(AddressEndpoint, createContent);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdAddress = JsonSerializer.Deserialize<AddressDto>(createdContent, _jsonOptions);
        Assert.That(createdAddress, Is.Not.Null);
        var addressId = createdAddress!.Id;
        await Task.Delay(500);

        // Act
        var response = await client.DeleteAsync($"{AddressEndpoint}/{addressId}");

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
        var response = await client.DeleteAsync($"{AddressEndpoint}/{nonExistentId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_RemovesAddress_AddressNoLongerInList()
    {
        // Arrange
        using var client = CreateHttpClient();

        var address = new AddressDto
        {
            Street = "Delete",
            City = "Delete",
            State = "DL",
            ContactInfoId = 1
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(address),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(AddressEndpoint, createContent);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdAddress = JsonSerializer.Deserialize<AddressDto>(createdContent, _jsonOptions);
        Assert.That(createdAddress, Is.Not.Null);
        var addressId = createdAddress!.Id;
        await Task.Delay(500);

        // Act
        await client.DeleteAsync($"{AddressEndpoint}/{addressId}");
        await Task.Delay(500);

        var getResponse = await client.GetAsync(AddressEndpoint);
        var content = await getResponse.Content.ReadAsStringAsync();
        var addresses = JsonSerializer.Deserialize<List<AddressDto>>(content, _jsonOptions);

        // Assert
        Assert.That(addresses, Is.Not.Null);
        Assert.That(addresses!.Any(a => a.Id == addressId), Is.False);
    }

    [Test]
    public async Task Crud_FullWorkflow_CreateUpdateDelete()
    {
        // Arrange
        using var client = CreateHttpClient();

        var originalAddress = new AddressDto
        {
            Street = "Workflow",
            City = "Workflow Start",
            State = "WS",
            ContactInfoId = 1
        };

        // Act & Assert - Create
        var createContent = new StringContent(
            JsonSerializer.Serialize(originalAddress),
            Encoding.UTF8,
            "application/json"
        );
        var createResponse = await client.PostAsync(AddressEndpoint, createContent);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdAddress = JsonSerializer.Deserialize<AddressDto>(createdContent, _jsonOptions);
        Assert.That(createdAddress, Is.Not.Null);
        var addressId = createdAddress!.Id;
        await Task.Delay(300);

        // Act & Assert - Read
        var getResponse = await client.GetAsync(AddressEndpoint);
        var content = await getResponse.Content.ReadAsStringAsync();
        var addresses = JsonSerializer.Deserialize<List<AddressDto>>(content, _jsonOptions);
        Assert.That(addresses!.Any(a => a.Id == addressId), Is.True);
        await Task.Delay(300);

        // Act & Assert - Update
        var updatedAddress = new AddressDto
        {
            Id = addressId,
            Street = "Updated Workflow",
            City = "Workflow Updated",
            State = "WU",
            ContactInfoId = 1
        };
        var updateContent = new StringContent(
            JsonSerializer.Serialize(updatedAddress),
            Encoding.UTF8,
            "application/json"
        );
        var updateResponse = await client.PutAsync(AddressEndpoint, updateContent);
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await Task.Delay(300);

        // Act & Assert - Delete
        var deleteResponse = await client.DeleteAsync($"{AddressEndpoint}/{addressId}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await Task.Delay(300);

        // Act & Assert - Verify deletion
        var finalGetResponse = await client.GetAsync(AddressEndpoint);
        var finalContent = await finalGetResponse.Content.ReadAsStringAsync();
        var finalAddresses = JsonSerializer.Deserialize<List<AddressDto>>(finalContent, _jsonOptions);
        Assert.That(finalAddresses!.Any(a => a.Id == addressId), Is.False);
    }

    [Test]
    public async Task Get_PerformanceTest_ReturnsWithinAcceptableTime()
    {
        // Arrange
        using var client = CreateHttpClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.GetAsync(AddressEndpoint);
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
        var address = new AddressDto
        {
            Street = "Perf",
            City = "Perf Test",
            State = "PT",
            ContactInfoId = 1
        };
        var content = new StringContent(
            JsonSerializer.Serialize(address),
            Encoding.UTF8,
            "application/json"
        );
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.PostAsync(AddressEndpoint, content);
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
            var address = new AddressDto
            {
                Street = $"Concurrent {i}",
                City = "Concurrent Test",
                State = "CT",
                ContactInfoId = 1
            };
            var content = new StringContent(
                JsonSerializer.Serialize(address),
                Encoding.UTF8,
                "application/json"
            );

            tasks.Add(client.PostAsync(AddressEndpoint, content));
        }

        // Assert
        var responses = await Task.WhenAll(tasks);
        Assert.That(responses, Is.All.Property("StatusCode").EqualTo(HttpStatusCode.Created));
    }
}
