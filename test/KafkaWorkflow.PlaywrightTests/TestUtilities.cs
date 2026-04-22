using System.Net;
using System.Text.Json;

namespace KafkaWorkflow.PlaywrightTests;

/// <summary>
/// Helper utilities for integration testing
/// </summary>
public static class TestUtilities
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Generates a unique person ID based on current timestamp
    /// </summary>
    public static int GenerateUniquepersonId()
    {
        var ticks = DateTime.UtcNow.Ticks;
        return (int)(ticks % int.MaxValue);
    }

    /// <summary>
    /// Generates a unique test identifier
    /// </summary>
    public static string GenerateTestIdentifier(string prefix = "Test")
    {
        var timestamp = DateTime.UtcNow.Ticks;
        return $"{prefix}_{timestamp}";
    }

    /// <summary>
    /// Deserializes JSON content to specified type
    /// </summary>
    public static T? DeserializeJson<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Serializes object to JSON
    /// </summary>
    public static string SerializeJson<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, JsonOptions);
    }

    /// <summary>
    /// Checks if response indicates success
    /// </summary>
    public static bool IsSuccessResponse(HttpStatusCode statusCode)
    {
        return statusCode >= HttpStatusCode.OK && statusCode < HttpStatusCode.BadRequest;
    }

    /// <summary>
    /// Checks if response indicates client error
    /// </summary>
    public static bool IsClientError(HttpStatusCode statusCode)
    {
        return statusCode >= HttpStatusCode.BadRequest && statusCode < HttpStatusCode.InternalServerError;
    }

    /// <summary>
    /// Checks if response indicates server error
    /// </summary>
    public static bool IsServerError(HttpStatusCode statusCode)
    {
        return statusCode >= HttpStatusCode.InternalServerError;
    }

    /// <summary>
    /// Waits for a specific time period
    /// </summary>
    public static async Task DelayAsync(int milliseconds)
    {
        await Task.Delay(milliseconds);
    }

    /// <summary>
    /// Retries an async operation with exponential backoff
    /// </summary>
    public static async Task<T> RetryAsync<T>(Func<Task<T>> operation, int maxAttempts = 3, int delayMs = 1000)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (HttpRequestException ex) when (attempt < maxAttempts)
            {
                await DelayAsync(delayMs * attempt);
            }
        }

        return await operation();
    }

    /// <summary>
    /// Extracts error message from response
    /// </summary>
    public static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
                return response.StatusCode.ToString();

            var json = DeserializeJson<Dictionary<string, object>>(content);
            return json?["message"]?.ToString() ?? response.StatusCode.ToString();
        }
        catch
        {
            return response.StatusCode.ToString();
        }
    }

    /// <summary>
    /// Creates HTTP content from object
    /// </summary>
    public static HttpContent CreateJsonContent<T>(T obj)
    {
        var json = SerializeJson(obj);
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Validates HTTP response status code
    /// </summary>
    public static void AssertStatusCode(HttpStatusCode actual, HttpStatusCode expected, string? message = null)
    {
        if (actual != expected)
        {
            var errorMsg = message ?? $"Expected status code {expected} but got {actual}";
            throw new AssertionException($"{errorMsg}");
        }
    }

    /// <summary>
    /// Validates response contains expected content
    /// </summary>
    public static void AssertResponseContains(string content, string expectedText, bool ignoreCase = true)
    {
        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!content.Contains(expectedText, comparison))
        {
            throw new AssertionException($"Response does not contain expected text: '{expectedText}'");
        }
    }

    /// <summary>
    /// Measures execution time of an async operation
    /// </summary>
    public static async Task<(T Result, long ElapsedMs)> MeasureAsync<T>(Func<Task<T>> operation)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await operation();
        stopwatch.Stop();
        return (result, stopwatch.ElapsedMilliseconds);
    }
}

/// <summary>
/// Custom assertion exception
/// </summary>
public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
}
