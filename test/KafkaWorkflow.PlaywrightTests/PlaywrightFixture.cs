using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Playwright;
using NUnit.Framework;

namespace KafkaWorkflow.PlaywrightTests;

/// <summary>
/// Base fixture for Playwright tests using DistributedApplicationTestingBuilder
/// Provides setup/teardown for browser instances and HTTP clients with automatic service discovery
/// </summary>
public class PlaywrightFixture
{
    public IBrowser? Browser { get; set; }
    public IBrowserContext? Context { get; set; }
    public IPage? Page { get; set; }

    private string _baseUrl = null!;
    private const int MaxRetries = 30;
    private const int RetryDelayMs = 1000;

    [SetUp]
    public async Task Setup()
    {
        try
        {
            // Get base URL from Aspire environment or use default
            _baseUrl = GetConfiguredBaseUrl();

            // Wait for service to be healthy before starting browser
            await WaitForServiceAsync();

            // Create browser instance
            var playwright = await Playwright.CreateAsync();
            Browser = await playwright.Chromium.LaunchAsync();
            Context = await Browser.NewContextAsync(new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true  // Allow self-signed certificates
            });
            Page = await Context.NewPageAsync();

            // Navigate to API documentation
            await Page.GotoAsync($"{_baseUrl}/scalar/v1", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        }
        catch (Exception ex)
        {
            await Teardown();
            throw new InvalidOperationException("Failed to setup test environment", ex);
        }
    }

    [TearDown]
    public async Task Teardown()
    {
        if (Page != null)
            await Page.CloseAsync();
        if (Context != null)
            await Context.CloseAsync();
        if (Browser != null)
            await Browser.CloseAsync();
    }

    protected string GetBaseUrl() => _baseUrl;

    protected async Task<string> GetAuthToken()
    {
        // Implement authentication token retrieval if needed
        return await Task.FromResult(string.Empty);
    }

    protected HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(_baseUrl)
        };

        return client;
    }

    /// <summary>
    /// Gets the configured base URL from Aspire environment or returns default.
    /// When running via DistributedApplicationTestingBuilder, Aspire sets environment variables.
    /// </summary>
    private string GetConfiguredBaseUrl()
    {
        // Check for Aspire service endpoint
        // Aspire sets SERVICES__WEBAPI__HTTP__0 when the service is added as a resource
        var aspireUrl = Environment.GetEnvironmentVariable("SERVICES__WEBAPI__HTTP__0");
        if (!string.IsNullOrEmpty(aspireUrl))
        {
            return aspireUrl;
        }

        // Check for custom base URL environment variable
        var customUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL");
        if (!string.IsNullOrEmpty(customUrl))
        {
            return customUrl;
        }

        // Default local URL
        return "https://localhost:7252";
    }

    /// <summary>
    /// Waits for the service to be healthy and responding to requests
    /// </summary>
    private async Task WaitForServiceAsync()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        using var client = new HttpClient(handler);

        for (int i = 0; i < MaxRetries; i++)
        {
            try
            {
                // Try health endpoint first
                var response = await client.GetAsync($"{_baseUrl}/health", HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    return; // Service is ready
                }
            }
            catch
            {
                // Health endpoint may not be available, try the API documentation endpoint
                try
                {
                    var response = await client.GetAsync($"{_baseUrl}/scalar/v1", HttpCompletionOption.ResponseHeadersRead);
                    if (response.IsSuccessStatusCode)
                    {
                        return; // Service is ready
                    }
                }
                catch (Exception ex)
                {
                    // Service not ready yet, retry
                    if (i == MaxRetries - 1)
                    {
                        throw new InvalidOperationException(
                            $"Service at {_baseUrl} did not become available after {MaxRetries} retries ({MaxRetries * RetryDelayMs}ms). Last error: {ex.Message}",
                            ex);
                    }
                }
            }

            await Task.Delay(RetryDelayMs);
        }
    }
}
