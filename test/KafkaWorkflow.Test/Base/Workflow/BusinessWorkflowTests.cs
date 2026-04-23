using KafkaWorkflow.Consumer;
using KafkaWorkflow.Consumer.Base;
using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.Consumer.PeopleWorkflow;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace KafkaWorkflow.Consumer.PeopleWorkflow.UnitTests
{
    [TestFixture]
    public partial class BusinessWorkflowTests
    {
        /// <summary>
        /// Verifies that OnExecuteAsync obtains state via OnGetStateAsync and assigns it to StateAccessor.Value.
        /// Input conditions: various int message values (min, zero, max).
        /// Expected: StateAccessor.Value is set to the state returned by OnGetStateAsync for each message.
        /// </summary>
        /// <param name="message">Integer message to pass to workflow.</param>
        [TestCase(int.MinValue)]
        [TestCase(0)]
        [TestCase(int.MaxValue)]
        public async Task OnExecuteAsync_StateAssigned_ForVariousMessages(int message)
        {
            // Arrange
            var accessorMock = new Mock<IObjectAccessor<string?>>();
            accessorMock.SetupProperty(a => a.Value, null);

            var loggerMock = new Mock<IWorkflowLogger<int, string?>>();
            loggerMock.Setup(l => l.CollectAsync<IMessageWorkflowStep<int, string?>>(It.IsAny<WorkflowStage>(), It.IsAny<string>(), It.IsAny<Exception?>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);
            loggerMock.Setup(l => l.WriteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Create mock of abstract BusinessWorkflow<int, string?>
            var workflowMock = new Mock<BusinessWorkflow<int, string?>>(accessorMock.Object, loggerMock.Object) { CallBase = true };
            string expectedState = $"{message}-state";
            workflowMock.Setup(w => w.OnGetStateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedState);

            // Ensure no steps to simplify assertion of state assignment
            workflowMock.Object.Steps = new List<IMessageWorkflowStep<int, string?>>().AsReadOnly();

            // Act
            await workflowMock.Object.OnExecuteAsync(message, CancellationToken.None);

            // Assert
            Assert.That(accessorMock.Object.Value, Is.EqualTo(expectedState));
        }

        /// <summary>
        /// Ensures that when a step's ShouldExecuteAsync returns false the step does not execute pre/execute actions.
        /// Input conditions: single step with ShouldExecuteAsync -> false.
        /// Expected: OnPreExecuteAsync and ExecuteAsync are never invoked.
        /// </summary>
        [Test]
        public async Task OnExecuteAsync_SkipsStepsWhenShouldExecuteReturnsFalse()
        {
            // Arrange
            var accessorMock = new Mock<IObjectAccessor<string?>>();
            accessorMock.SetupProperty(a => a.Value, null);

            var loggerMock = new Mock<IWorkflowLogger<int, string?>>();
            loggerMock.Setup(l => l.CollectAsync<IMessageWorkflowStep<int, string?>>(It.IsAny<WorkflowStage>(), It.IsAny<string>(), It.IsAny<Exception?>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);
            loggerMock.Setup(l => l.WriteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var stepMock = new Mock<IMessageWorkflowStep<int, string?>>();
            stepMock.Setup(s => s.ShouldExecuteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
            stepMock.Setup(s => s.OnPreExecuteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            stepMock.Setup(s => s.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            stepMock.Setup(s => s.OnCompleteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var workflowMock = new Mock<BusinessWorkflow<int, string?>>(accessorMock.Object, loggerMock.Object) { CallBase = true };
            workflowMock.Setup(w => w.OnGetStateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync("state");

            workflowMock.Object.Steps = new List<IMessageWorkflowStep<int, string?>>
            {
                stepMock.Object
            }.AsReadOnly();

            // Act
            await workflowMock.Object.OnExecuteAsync(1, CancellationToken.None);

            // Assert
            stepMock.Verify(s => s.OnPreExecuteAsync(It.IsAny<CancellationToken>()), Times.Never);
            stepMock.Verify(s => s.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Never);
            // OnCompleteAsync should still be invoked (finally block)
            stepMock.Verify(s => s.OnCompleteAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests behavior when ExecuteAsync throws: logger is notified, OnErrorAsync is invoked and returning false stops further processing for that step.
        /// Also ensures finally block runs (OnCompleteAsync, logger CollectAsync for Complete and WriteAsync).
        /// Input conditions: single step that throws from ExecuteAsync and OnErrorAsync returns false.
        /// Expected: OnErrorAsync called once, OnCompleteAsync invoked, logger's WriteAsync invoked, and error CollectAsync invoked.
        /// </summary>
        [Test]
        public async Task OnExecuteAsync_OnErrorStopsProcessing_OnErrorReturnsFalse_AndEnsuresCompletionLogged()
        {
            // Arrange
            var accessorMock = new Mock<IObjectAccessor<string?>>();
            accessorMock.SetupProperty(a => a.Value, null);

            var loggerMock = new Mock<IWorkflowLogger<int, string?>>();
            loggerMock.Setup(l => l.CollectAsync<IMessageWorkflowStep<int, string?>>(It.IsAny<WorkflowStage>(), It.IsAny<string>(), It.IsAny<Exception?>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);
            var writeCalledTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            loggerMock.Setup(l => l.WriteAsync(It.IsAny<CancellationToken>()))
                      .Callback(() => writeCalledTcs.TrySetResult(true))
                      .Returns(Task.CompletedTask);

            var stepMock = new Mock<IMessageWorkflowStep<int, string?>>();
            stepMock.Setup(s => s.ShouldExecuteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            stepMock.Setup(s => s.OnPreExecuteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            stepMock.Setup(s => s.ExecuteAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("exec-fail"));

            // OnErrorAsync returns false to indicate processing should stop for this step.
            stepMock.Setup(s => s.OnErrorAsync(It.IsAny<Exception>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var onCompleteTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            stepMock.Setup(s => s.OnCompleteAsync(It.IsAny<CancellationToken>()))
                    .Callback(() => onCompleteTcs.TrySetResult(true))
                    .Returns(Task.CompletedTask);

            var workflowMock = new Mock<BusinessWorkflow<int, string?>>(accessorMock.Object, loggerMock.Object) { CallBase = true };
            workflowMock.Setup(w => w.OnGetStateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync("state");

            workflowMock.Object.Steps = new List<IMessageWorkflowStep<int, string?>>
            {
                stepMock.Object
            }.AsReadOnly();

            // Act
            await workflowMock.Object.OnExecuteAsync(1, CancellationToken.None);

            // Wait for the OnCompleteAsync and logger.WriteAsync to be invoked from the finally block of the step's async action.
            var completed = await Task.WhenAny(onCompleteTcs.Task, Task.Delay(2000));
            var writeCompleted = await Task.WhenAny(writeCalledTcs.Task, Task.Delay(2000));

            // Assert
            stepMock.Verify(s => s.OnErrorAsync(It.IsAny<Exception>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(onCompleteTcs.Task.IsCompleted, Is.True, "OnCompleteAsync should have been invoked in finally.");
            Assert.That(writeCalledTcs.Task.IsCompleted, Is.True, "Logger.WriteAsync should have been invoked in finally.");
            // Verify that an error collect call was made (Execute error)
            loggerMock.Verify(l => l.CollectAsync<IMessageWorkflowStep<int, string?>>(WorkflowStage.Execute, It.Is<string>(s => s.Contains("Error during step processing.")), It.IsAny<Exception?>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            // Also ensure completion collect was invoked
            loggerMock.Verify(l => l.CollectAsync<IMessageWorkflowStep<int, string?>>(WorkflowStage.Complete, It.Is<string>(s => s.Contains("Step completed.")), null, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}