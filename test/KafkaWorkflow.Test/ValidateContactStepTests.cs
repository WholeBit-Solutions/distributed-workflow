using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.Consumer.PeopleWorkflow;
using KafkaWorkflow.Consumer.PeopleWorkflow.Steps;
using KafkaWorkflow.WebApi.Db;
using KafkaWorkflow.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KafkaWorkflow.Test;

[TestFixture]
public class ValidateContactStepTests
{
    private Mock<IMessageWorkflow<int, PersonState?>> _mockWorkflow = null!;
    private Mock<PeopleContext> _mockDbContext = null!;
    private ValidateContactStep _step = null!;
    private PersonState _personState = null!;

    [SetUp]
    public void Setup()
    {
        _mockWorkflow = new Mock<IMessageWorkflow<int, PersonState?>>();
        _mockDbContext = new Mock<PeopleContext>();
        _personState = new PersonState(1);
        _mockWorkflow.Setup(w => w.StateAccessor.Value).Returns(_personState);

        _step = new ValidateContactStep(_mockWorkflow.Object, _mockDbContext.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Console.SetOut(Console.Out);
    }

    [Test]
    public async Task ShouldExecuteAsync_WithContactInfo_ReturnsTrue()
    {
        // Arrange
        var contactInfo = new ContactInfo { Id = 1, Email = "john@example.com", Phone = "555-1234" };
        _personState.ContactInfos = [contactInfo];

        // Act
        var result = await _step.ShouldExecuteAsync();

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ShouldExecuteAsync_WithoutContactInfo_ReturnsFalse()
    {
        // Arrange
        _personState.ContactInfos = [];

        // Act
        var result = await _step.ShouldExecuteAsync();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ShouldExecuteAsync_WithNullContactInfo_ReturnsFalse()
    {
        // Arrange
        _personState.ContactInfos = null;

        // Act
        var result = await _step.ShouldExecuteAsync();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ShouldExecuteAsync_WithMultipleContacts_ReturnsTrue()
    {
        // Arrange
        var contact1 = new ContactInfo { Id = 1, Email = "contact1@example.com" };
        var contact2 = new ContactInfo { Id = 2, Email = "contact2@example.com" };
        _personState.ContactInfos = [contact1, contact2];

        // Act
        var result = await _step.ShouldExecuteAsync();

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ExecuteAsync_WithContactInfo_WritesOutput()
    {
        // Arrange
        var contactInfo = new ContactInfo { Id = 1, Email = "john@example.com", Phone = "555-1234" };
        _personState.ContactInfos = [contactInfo];
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        await _step.ExecuteAsync();

        // Assert
        var output = stringWriter.ToString();
        Assert.That(output, Does.Contain("Validating contacts"));
        Assert.That(output, Does.Contain("john@example.com"));
    }

    [Test]
    public async Task ExecuteAsync_WithMultipleContacts_WritesAllContacts()
    {
        // Arrange
        var contact1 = new ContactInfo { Id = 1, Email = "contact1@example.com" };
        var contact2 = new ContactInfo { Id = 2, Email = "contact2@example.com" };
        _personState.ContactInfos = [contact1, contact2];
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        await _step.ExecuteAsync();

        // Assert
        var output = stringWriter.ToString();
        Assert.That(output, Does.Contain("Validating contacts"));
        Assert.That(output, Does.Contain("contact1@example.com"));
        Assert.That(output, Does.Contain("contact2@example.com"));
    }

    [Test]
    public async Task ExecuteAsync_WithCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var contactInfo = new ContactInfo { Id = 1, Email = "test@example.com" };
        _personState.ContactInfos = [contactInfo];
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        await _step.ExecuteAsync(cts.Token);

        // Assert
        var output = stringWriter.ToString();
        Assert.That(output, Does.Contain("Validating contacts"));
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
