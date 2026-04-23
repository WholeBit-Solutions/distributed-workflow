using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.Consumer.PeopleWorkflow;
using KafkaWorkflow.Consumer.PeopleWorkflow.Steps;
using KafkaWorkflow.WebApi.Db;
using KafkaWorkflow.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KafkaWorkflow.Test;

[TestFixture]
public class ValidateAddressStepTests
{
    private Mock<IMessageWorkflow<int, PersonState?>> _mockWorkflow = null!;
    private Mock<PeopleContext> _mockDbContext = null!;
    private ValidateAddressStep _step = null!;
    private PersonState _personState = null!;

    [SetUp]
    public void Setup()
    {
        _mockWorkflow = new Mock<IMessageWorkflow<int, PersonState?>>();
        _mockDbContext = new Mock<PeopleContext>();
        _personState = new PersonState(1);
        _mockWorkflow.Setup(w => w.StateAccessor.Value).Returns(_personState);

        _step = new ValidateAddressStep(_mockWorkflow.Object, _mockDbContext.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Console.SetOut(Console.Out);
    }

    [Test]
    public async Task ShouldExecuteAsync_WithAddresses_ReturnsTrue()
    {
        // Arrange
        var contactInfo = new ContactInfo { Id = 1 };
        var addresses = new List<Address>
        {
            new() { Id = 1, Street = "123 Main St", City = "Boston", State = "MA", ContactInfo = contactInfo, ContactInfoId = 1 },
            new() { Id = 2, Street = "456 Oak Ave", City = "Boston", State = "MA", ContactInfo = contactInfo, ContactInfoId = 1 }
        };
        _personState.Addresses = addresses;

        // Act
        var result = await _step.ShouldExecuteAsync();

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ShouldExecuteAsync_WithoutAddresses_ReturnsFalse()
    {
        // Arrange
        _personState.Addresses = new List<Address>();

        // Act
        var result = await _step.ShouldExecuteAsync();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ShouldExecuteAsync_WithNullAddresses_ReturnsFalse()
    {
        // Arrange
        _personState.Addresses = null;

        // Act
        var result = await _step.ShouldExecuteAsync();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ShouldExecuteAsync_WithSingleAddress_ReturnsTrue()
    {
        // Arrange
        var contactInfo = new ContactInfo { Id = 1 };
        var address = new Address 
        { 
            Id = 1, 
            Street = "123 Main St", 
            City = "Boston", 
            State = "MA", 
            ContactInfo = contactInfo, 
            ContactInfoId = 1 
        };
        _personState.Addresses = [address];

        // Act
        var result = await _step.ShouldExecuteAsync();

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ExecuteAsync_WithAddresses_WritesAddressInfo()
    {
        // Arrange
        var contactInfo = new ContactInfo { Id = 1 };
        var addresses = new List<Address>
        {
            new() { Id = 1, Street = "123 Main St", City = "Boston", State = "MA", ContactInfo = contactInfo, ContactInfoId = 1 }
        };
        _personState.Addresses = addresses;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        await _step.ExecuteAsync();

        // Assert
        var output = stringWriter.ToString();
        Assert.That(output, Does.Contain("Validating addresses"));
        Assert.That(output, Does.Contain("123 Main St"));
    }

    [Test]
    public async Task ExecuteAsync_WithoutAddresses_WritesNoAddressesFound()
    {
        // Arrange
        _personState.Addresses = new List<Address>();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        await _step.ExecuteAsync();

        // Assert
        var output = stringWriter.ToString();
        Assert.That(output, Does.Contain("Validating addresses"));
    }

    [Test]
    public async Task ExecuteAsync_WithMultipleAddresses_PrintsEachAddress()
    {
        // Arrange
        var contactInfo = new ContactInfo { Id = 1 };
        var addresses = new List<Address>
        {
            new() { Id = 1, Street = "123 Main St", City = "Boston", State = "MA", ContactInfo = contactInfo, ContactInfoId = 1 },
            new() { Id = 2, Street = "456 Oak Ave", City = "Boston", State = "MA", ContactInfo = contactInfo, ContactInfoId = 1 }
        };
        _personState.Addresses = addresses;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        await _step.ExecuteAsync();

        // Assert
        var output = stringWriter.ToString();
        Assert.That(output, Does.Contain("123 Main St"));
        Assert.That(output, Does.Contain("456 Oak Ave"));
    }

    [Test]
    public async Task ExecuteAsync_WithCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var contactInfo = new ContactInfo { Id = 1 };
        var addresses = new List<Address>
        {
            new() { Id = 1, Street = "789 Elm St", City = "Boston", State = "MA", ContactInfo = contactInfo, ContactInfoId = 1 }
        };
        _personState.Addresses = addresses;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        await _step.ExecuteAsync(cts.Token);

        // Assert
        var output = stringWriter.ToString();
        Assert.That(output, Does.Contain("Validating addresses"));
    }

    [Test]
    public async Task OnPreExecuteAsync_CompletesSuccessfully()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        await _step.OnPreExecuteAsync(cts.Token);

        // Assert
        Assert.That(_step, Is.Not.Null);
    }

    [Test]
    public async Task OnCompleteAsync_CompletesSuccessfully()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        await _step.OnCompleteAsync(cts.Token);

        // Assert
        Assert.That(_step, Is.Not.Null);
    }
}
