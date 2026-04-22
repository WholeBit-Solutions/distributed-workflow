using System.Net;
using System.Text;
using System.Text.Json;
using KafkaWorkflow.PlaywrightTests.dto;
using Microsoft.Playwright;
using NUnit.Framework;

namespace KafkaWorkflow.PlaywrightTests.tests;

[TestFixture]
public class PeopleControllerErrorHandlingTests : PlaywrightFixture
{
    private const string PeopleEndpoint = "/people";
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Test]
    public async Task Post_WithInvalidJson_ReturnsBadRequest()
    {
        // Arrange
        using var client = CreateHttpClient();
        var invalidJson = "{ invalid json }";
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync(PeopleEndpoint, content);

        // Assert
        Assert.That(response.StatusCode, Is.AnyOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.InternalServerError
        ));
    }

    [Test]
    public async Task Put_WithInvalidJson_ReturnsBadRequest()
    {
        // Arrange
        using var client = CreateHttpClient();
        var invalidJson = "{ not valid }";
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync(PeopleEndpoint, content);

        // Assert
        Assert.That(response.StatusCode, Is.AnyOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity
        ));
    }

    [Test]
    public async Task Delete_WithInvalidId_HandlesProperly()
    {
        // Arrange
        using var client = CreateHttpClient();
        var invalidId = "not-a-number";

        // Act
        var response = await client.DeleteAsync($"{PeopleEndpoint}/{invalidId}");

        // Assert
        Assert.That(response.StatusCode, Is.AnyOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound
        ));
    }

    [Test]
    public async Task Post_EmptyPayload_HandlesProperly()
    {
        // Arrange
        using var client = CreateHttpClient();
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync(PeopleEndpoint, content);

        // Assert
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Get_ReturnsValidJsonStructure()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Act
        var response = await client.GetAsync(PeopleEndpoint);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.DoesNotThrow(() =>
        {
            JsonSerializer.Deserialize<List<PersonDto>>(content, _jsonOptions);
        });
    }

    [Test]
    public async Task ContentType_IsCorrect()
    {
        // Arrange
        using var client = CreateHttpClient();

        // Act
        var response = await client.GetAsync(PeopleEndpoint);

        // Assert
        Assert.That(response.Content.Headers.ContentType?.MediaType, Does.Contain("application/json"));
    }
}

[TestFixture]
public class PeopleControllerPerformanceTests : PlaywrightFixture
{
    private const string PeopleEndpoint = "/people";

    [Test]
    public async Task Get_ReturnsWithinAcceptableTime()
    {
        // Arrange
        using var client = CreateHttpClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.GetAsync(PeopleEndpoint);
        stopwatch.Stop();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000), "GET request should complete within 5 seconds");
    }

    [Test]
    public async Task Post_ReturnsWithinAcceptableTime()
    {
        // Arrange
        using var client = CreateHttpClient();
        var timestamp = DateTime.UtcNow.Ticks;
        var person = new PersonDto
        {
            Id = (int)(timestamp % int.MaxValue),
            FirstName = $"Perf_{timestamp}",
            LastName = "Test"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(person),
            Encoding.UTF8,
            "application/json"
        );
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.PostAsync(PeopleEndpoint, content);
        stopwatch.Stop();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000), "POST request should complete within 5 seconds");
    }

    [Test]
    public async Task Delete_ReturnsWithinAcceptableTime()
    {
        // Arrange
        using var client = CreateHttpClient();
        var timestamp = DateTime.UtcNow.Ticks;
        var personId = (int)((timestamp + 6000) % int.MaxValue);
        
        var person = new PersonDto
        {
            Id = personId,
            FirstName = $"Perf_{timestamp}",
            LastName = "Delete"
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(person),
            Encoding.UTF8,
            "application/json"
        );
        await client.PostAsync(PeopleEndpoint, createContent);
        await Task.Delay(300);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.DeleteAsync($"{PeopleEndpoint}/{personId}");
        stopwatch.Stop();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000), "DELETE request should complete within 5 seconds");
    }
}
