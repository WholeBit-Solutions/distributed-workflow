using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.Consumer.PeopleWorkflow;
using KafkaWorkflow.Consumer.PeopleWorkflow.Steps;
using KafkaWorkflow.WebApi.Db;
using KafkaWorkflow.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KafkaWorkflow.Test;

[TestFixture]
public class ValidatePersonStepTests
{
    private Mock<IMessageWorkflow<int, PersonState?>> _mockWorkflow = null!;
    private Mock<PeopleContext> _mockDbContext = null!;
    private ValidatePersonStep _step = null!;
    private PersonState _personState = null!;

    private TextWriter? _originalOut;

    [SetUp]
    public void Setup()
    {
        _originalOut = Console.Out;
        _mockWorkflow = new Mock<IMessageWorkflow<int, PersonState?>>();
        _mockDbContext = new Mock<PeopleContext>();
        _personState = new PersonState(1);
        _mockWorkflow.Setup(w => w.State).Returns(_personState);

        _step = new ValidatePersonStep(_mockWorkflow.Object, _mockDbContext.Object);
    }

    [TearDown]
    public void TearDown()
    {
        if (_originalOut != null)
        {
            Console.SetOut(_originalOut);
        }
    }

    [Test]
    public async Task ShouldExecute_WithExistingPerson_ReturnsTrue()
    {
        // Arrange
        var person = new Person 
        { 
            Id = 1, 
            FirstName = "John",
            LastName = "Doe"
        };
        var mockPersons = CreateMockDbSet(new[] { person });
        _mockDbContext.Setup(c => c.Persons).Returns(mockPersons.Object);

        // Act
        var result = await _step.ShouldExecute();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_personState.Person, Is.EqualTo(person));
    }

    [Test]
    public async Task ShouldExecute_WithNonExistingPerson_ReturnsFalse()
    {
        // Arrange
        _mockDbContext.Setup(c => c.Persons).Returns(CreateMockDbSet(new Person[] { }).Object);

        // Act
        var result = await _step.ShouldExecute();

        // Assert
        Assert.That(result, Is.False);
        Assert.That(_personState.Person, Is.Null);
    }

    [Test]
    public async Task ShouldExecute_SetsPersonInState()
    {
        // Arrange
        var person = new Person 
        { 
            Id = 1, 
            FirstName = "Jane",
            LastName = "Smith"
        };
        var mockPersons = CreateMockDbSet(new[] { person });
        _mockDbContext.Setup(c => c.Persons).Returns(mockPersons.Object);

        // Act
        var result = await _step.ShouldExecute();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_personState.Person, Is.Not.Null);
        Assert.That(_personState.Person.FirstName, Is.EqualTo("Jane"));
        Assert.That(_personState.Person.LastName, Is.EqualTo("Smith"));
    }

    [Test]
    public async Task ExecuteAsync_WithPersonFound_WritesOutput()
    {
        // Arrange
        var person = new Person 
        { 
            Id = 1, 
            FirstName = "John",
            LastName = "Doe"
        };
        _personState.Person = person;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        await _step.ExecuteAsync();

        // Assert
        var output = stringWriter.ToString();
        Assert.That(output, Does.Contain("Validating person"));
        Assert.That(output, Does.Contain("John"));
    }

    [Test]
    public async Task ExecuteAsync_WithoutPerson_WritesNoPersonFound()
    {
        // Arrange
        _personState.Person = null;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        await _step.ExecuteAsync();

        // Assert
        var output = stringWriter.ToString();
        Assert.That(output, Does.Contain("No person found"));
    }

    [Test]
    public async Task ExecuteAsync_WithCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        _personState.Person = new Person 
        { 
            Id = 1,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        await _step.ExecuteAsync(cts.Token);

        // Assert
        Assert.That(_personState.Person, Is.Not.Null);
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
