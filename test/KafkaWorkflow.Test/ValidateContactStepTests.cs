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
        _mockWorkflow.Setup(w => w.State).Returns(_personState);

        _step = new ValidateContactStep(_mockWorkflow.Object, _mockDbContext.Object);
    }

    [Test]
    public async Task ShouldExecute_WithContactInfo_ReturnsTrue()
    {
        // Arrange
        var contactInfo = new ContactInfo { Id = 1, Email = "john@example.com", Phone = "555-1234" };
        var person = new Person 
        { 
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            ContactInfos = [contactInfo]
        };
        var mockPersons = CreateMockDbSet(new[] { person });
        _mockDbContext.Setup(c => c.Persons).Returns(mockPersons.Object);

        // Act
        var result = await _step.ShouldExecute();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_personState.ContactInfos?.Count(), Is.EqualTo(1));
        Assert.That(_personState.ContactInfos?.First(), Is.EqualTo(contactInfo));
    }

    [Test]
    public async Task ShouldExecute_WithoutContactInfo_ReturnsFalse()
    {
        // Arrange
        var person = new Person 
        { 
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            ContactInfos = []
        };
        var mockPersons = CreateMockDbSet(new[] { person });
        _mockDbContext.Setup(c => c.Persons).Returns(mockPersons.Object);

        // Act
        var result = await _step.ShouldExecute();

        // Assert
        Assert.That(result, Is.False);
        Assert.That(_personState.ContactInfos?.Count() ?? 0, Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldExecute_WithNonExistentPerson_ReturnsFalse()
    {
        // Arrange
        _mockDbContext.Setup(c => c.Persons).Returns(CreateMockDbSet(new Person[] { }).Object);

        // Act
        var result = await _step.ShouldExecute();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ShouldExecute_SetsContactInfoInState()
    {
        // Arrange
        var contactInfo = new ContactInfo { Id = 1, Email = "jane@example.com", Phone = "555-5678" };
        var person = new Person 
        { 
            Id = 1,
            FirstName = "Jane",
            LastName = "Doe",
            ContactInfos = [contactInfo]
        };
        var mockPersons = CreateMockDbSet(new[] { person });
        _mockDbContext.Setup(c => c.Persons).Returns(mockPersons.Object);

        // Act
        await _step.ShouldExecute();

        // Assert
        Assert.That(_personState.ContactInfos, Is.Not.Null);
        Assert.That(_personState.ContactInfos?.First().Email, Is.EqualTo("jane@example.com"));
        Assert.That(_personState.ContactInfos?.First().Phone, Is.EqualTo("555-5678"));
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
        Assert.That(output, Does.Contain("Validating contact"));
        Assert.That(output, Does.Contain("john@example.com"));
    }

    [Test]
    public async Task ExecuteAsync_WithoutContactInfo_WritesNoContactFound()
    {
        // Arrange
        _personState.ContactInfos = [];
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        await _step.ExecuteAsync();

        // Assert
        var output = stringWriter.ToString();
        Assert.That(output, Does.Contain("No contact info found"));
    }

    [Test]
    public async Task ExecuteAsync_WithCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        _personState.ContactInfos = [new ContactInfo { Id = 1, Email = "test@example.com" }];

        // Act
        await _step.ExecuteAsync(cts.Token);

        // Assert
        Assert.That(_personState.ContactInfos, Is.Not.Null);
        Assert.That(_personState.ContactInfos?.Count(), Is.GreaterThan(0));
    }

    [Test]
    public async Task ExecuteAsync_Completes_Successfully()
    {
        // Arrange
        var contactInfo = new ContactInfo { Id = 1, Email = "test@example.com" };
        _personState.ContactInfos = [contactInfo];

        // Act
        var task = _step.ExecuteAsync();
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(5)));

        // Assert
        Assert.That(task, Is.EqualTo(completed));
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> sourceList) where T : class
    {
        var queryable = sourceList.AsQueryable();
        var mock = new Mock<DbSet<T>>();

        mock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        return mock;
    }
}
