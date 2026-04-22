using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.Consumer.PeopleWorkflow;
using Moq;
using NUnit.Framework;

namespace KafkaWorkflow.Test;

[TestFixture]
public class PersonWorkflowTests
{
    private PersonWorkflow _personWorkflow = null!;
    private const int TestPersonId = 1;

    [SetUp]
    public void Setup()
    {
        _personWorkflow = new PersonWorkflow
        {
            Steps = []
        };
    }

    [Test]
    public async Task ExecuteAsync_InitializesStateWithPersonId()
    {
        // Arrange
        int personId = TestPersonId;

        // Act
        await _personWorkflow.ExecuteAsync(personId);

        // Assert
        Assert.That(_personWorkflow.State, Is.Not.Null);
        Assert.That(_personWorkflow.State.PersonId, Is.EqualTo(personId));
    }

    [Test]
    public async Task ExecuteAsync_CreatesPersonStateWithGivenMessage()
    {
        // Arrange
        int testMessage = 42;

        // Act
        await _personWorkflow.ExecuteAsync(testMessage);

        // Assert
        Assert.That(_personWorkflow.State, Is.Not.Null);
        Assert.That(_personWorkflow.State, Is.TypeOf<PersonState>());
        Assert.That(_personWorkflow.State.PersonId, Is.EqualTo(42));
    }

    [Test]
    public async Task ExecuteAsync_CallsBaseExecuteAsync()
    {
        // Arrange
        _personWorkflow.Steps = new List<IMessageWorkflowStep<int, PersonState?>>().AsReadOnly();

        // Act
        await _personWorkflow.ExecuteAsync(TestPersonId);

        // Assert
        Assert.That(_personWorkflow.State, Is.Not.Null);
        Assert.That(_personWorkflow.State.PersonId, Is.EqualTo(TestPersonId));
    }

    [Test]
    public async Task ExecuteAsync_WithCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        _personWorkflow.Steps = new List<IMessageWorkflowStep<int, PersonState?>>().AsReadOnly();

        // Act
        await _personWorkflow.ExecuteAsync(TestPersonId, cts.Token);

        // Assert
        Assert.That(_personWorkflow.State, Is.Not.Null);
        Assert.That(_personWorkflow.State.PersonId, Is.EqualTo(TestPersonId));
    }

    [Test]
    public async Task ExecuteAsync_ExecutesAllSteps()
    {
        // Arrange
        var mockStep1 = new Mock<IMessageWorkflowStep<int, PersonState?>>();
        var mockStep2 = new Mock<IMessageWorkflowStep<int, PersonState?>>();

        mockStep1.Setup(s => s.ShouldExecute()).ReturnsAsync(true);
        mockStep1.Setup(s => s.ExecuteAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockStep2.Setup(s => s.ShouldExecute()).ReturnsAsync(true);
        mockStep2.Setup(s => s.ExecuteAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _personWorkflow.Steps = new List<IMessageWorkflowStep<int, PersonState?>>
        {
            mockStep1.Object,
            mockStep2.Object
        }.AsReadOnly();

        // Act
        await _personWorkflow.ExecuteAsync(TestPersonId);

        // Assert
        mockStep1.Verify(s => s.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockStep2.Verify(s => s.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_WithZeroPersonId()
    {
        // Act
        await _personWorkflow.ExecuteAsync(0);

        // Assert
        Assert.That(_personWorkflow.State, Is.Not.Null);
        Assert.That(_personWorkflow.State.PersonId, Is.EqualTo(0));
    }
}
