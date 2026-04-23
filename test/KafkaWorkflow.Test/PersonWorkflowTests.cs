using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.Consumer.PeopleWorkflow;
using KafkaWorkflow.WebApi.Db;
using Moq;

namespace KafkaWorkflow.Test;

[TestFixture]
public class PersonWorkflowTests
{
    private PersonWorkflow _personWorkflow = null!;
    private Mock<IWorkflowLogger<int, PersonState?>> _mockLogger = null!;
    private const int TestPersonId = 1;
    private Mock<PeopleContext> _mockDbContext = null!;

    [Test]
    public async Task OnExecuteAsync_ExecutesAllSteps()
    {
        // Arrange
        var mockStep1 = new Mock<IMessageWorkflowStep<int, PersonState?>>();
        var mockStep2 = new Mock<IMessageWorkflowStep<int, PersonState?>>();

        mockStep1.Setup(s => s.ShouldExecuteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mockStep1.Setup(s => s.OnPreExecuteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockStep1.Setup(s => s.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockStep1.Setup(s => s.OnCompleteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        mockStep2.Setup(s => s.ShouldExecuteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mockStep2.Setup(s => s.OnPreExecuteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockStep2.Setup(s => s.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockStep2.Setup(s => s.OnCompleteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        if (_personWorkflow == null)
        {
            var accessorMock = new Moq.Mock<KafkaWorkflow.Consumer.Base.Workflow.IObjectAccessor<KafkaWorkflow.Consumer.PeopleWorkflow.PersonState?>>();
            var loggerMock = new Moq.Mock<KafkaWorkflow.Consumer.Base.Workflow.IWorkflowLogger<int, KafkaWorkflow.Consumer.PeopleWorkflow.PersonState?>>();
            var dbContextMock = new Moq.Mock<KafkaWorkflow.WebApi.Db.PeopleContext>();
            // Create a mock of the concrete PersonWorkflow (call base so the real OnExecuteAsync is used)
            var personWorkflowMock = new Moq.Mock<KafkaWorkflow.Consumer.PeopleWorkflow.PersonWorkflow>(dbContextMock.Object, accessorMock.Object, loggerMock.Object) { CallBase = true };
            // Prevent the real OnGetStateAsync from hitting EF (dbContext.Persons) by stubbing it out.
            personWorkflowMock.Setup(p => p.OnGetStateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync((PersonState?)null);
            _personWorkflow = personWorkflowMock.Object;
        }

        _personWorkflow.Steps = new List<IMessageWorkflowStep<int, PersonState?>>
        {
            mockStep1.Object,
            mockStep2.Object
        }.AsReadOnly();

        // Act
        await _personWorkflow.OnExecuteAsync(1);

        // Assert
        mockStep1.Verify(s => s.ShouldExecuteAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockStep1.Verify(s => s.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockStep2.Verify(s => s.ShouldExecuteAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockStep2.Verify(s => s.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task OnExecuteAsync_SkipsStepsWhenShouldExecuteReturnsFalse()
    {
        // Arrange
        var mockStep = new Mock<IMessageWorkflowStep<int, PersonState?>>();
        mockStep.Setup(s => s.ShouldExecuteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Always create and configure a fresh workflow mock for this test
        var accessorMock = new Moq.Mock<KafkaWorkflow.Consumer.Base.Workflow.IObjectAccessor<KafkaWorkflow.Consumer.PeopleWorkflow.PersonState?>>();
        var loggerMock = new Moq.Mock<KafkaWorkflow.Consumer.Base.Workflow.IWorkflowLogger<int, KafkaWorkflow.Consumer.PeopleWorkflow.PersonState?>>();
        var dbContextMock = new Moq.Mock<KafkaWorkflow.WebApi.Db.PeopleContext>();
        // Create a mock of the concrete PersonWorkflow (call base so the real OnExecuteAsync is used)
        var personWorkflowMock = new Moq.Mock<KafkaWorkflow.Consumer.PeopleWorkflow.PersonWorkflow>(dbContextMock.Object, accessorMock.Object, loggerMock.Object) { CallBase = true };
        // Prevent the real OnGetStateAsync from hitting EF (dbContext.Persons) by stubbing it out.
        personWorkflowMock.Setup(p => p.OnGetStateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((PersonState?)null);
        _personWorkflow = personWorkflowMock.Object;

        _personWorkflow.Steps = new List<IMessageWorkflowStep<int, PersonState?>>
        {
            mockStep.Object
        }.AsReadOnly();

        // Act
        await _personWorkflow.OnExecuteAsync(1);

        // Assert
        mockStep.Verify(s => s.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockStep.Verify(s => s.OnPreExecuteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

}
