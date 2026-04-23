using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.Consumer.PeopleWorkflow;
using KafkaWorkflow.Consumer.PeopleWorkflow.Steps;
using KafkaWorkflow.WebApi.Db;
using KafkaWorkflow.WebApi.Db.Entities;
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
        _mockWorkflow.Setup(w => w.StateAccessor.Value).Returns(_personState);

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
    public async Task ShouldExecuteAsync_WithExistingPerson_ReturnsTrue()
    {
        // Arrange
        var person = new Person
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe"
        };
        _personState.Person = person;

        // Act
        var result = await _step.ShouldExecuteAsync();

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ShouldExecuteAsync_WithoutPerson_ReturnsFalse()
    {
        // Arrange
        _personState.Person = null;

        // Act
        var result = await _step.ShouldExecuteAsync();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ShouldExecuteAsync_WithPersonSet_ReturnsTrue()
    {
        // Arrange
        var person = new Person
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith"
        };
        _personState.Person = person;

        // Act
        var result = await _step.ShouldExecuteAsync();

        // Assert
        Assert.That(result, Is.True);
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
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        await _step.ExecuteAsync(cts.Token);

        // Assert
        var output = stringWriter.ToString();
        Assert.That(output, Does.Contain("Validating person"));
    }

    [Test]
    public async Task OnPreExecuteAsync_CompletesSuccessfully()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        await _step.OnPreExecuteAsync(cts.Token);

        // Assert - No exception should be thrown
        Assert.That(_step, Is.Not.Null);
    }

    [Test]
    public async Task OnCompleteAsync_CompletesSuccessfully()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        await _step.OnCompleteAsync(cts.Token);

        // Assert - No exception should be thrown
        Assert.That(_step, Is.Not.Null);
    }
}