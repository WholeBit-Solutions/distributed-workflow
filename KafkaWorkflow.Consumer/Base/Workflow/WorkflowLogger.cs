using Microsoft.Extensions.Logging;

namespace KafkaWorkflow.Consumer.Base.Workflow
{
    public class WorkflowLogger<T, TState>(ILogger logger) : IWorkflowLogger<T, TState>
        where TState : class
    {
        private List<(LogLevel, string, DateTime)> messages = [];

        public virtual Task CollectAsync(WorkflowStage stage, string message, Exception? exception, CancellationToken cancellationToken = default)
        {
            if (exception == null)
            {
                messages.Add((LogLevel.Information, $"Workflow at stage {stage}: {message}", DateTime.UtcNow));
            }
            else
            {
                messages.Add((LogLevel.Error, $"Workflow at stage {stage}: {message}. Exception: {exception}", DateTime.UtcNow));
            }

            return Task.CompletedTask;
        }

        public virtual Task CollectAsync<TWorkflowStep>(WorkflowStage stage, string message, Exception? exception, CancellationToken cancellationToken = default)
            where TWorkflowStep : IMessageWorkflowStep<T, TState?>
        {
            if (exception == null)
            {
                messages.Add((LogLevel.Information, $"Workflow step {typeof(TWorkflowStep).Name} at stage {stage}: {message}", DateTime.UtcNow));
            }
            else
            {
                messages.Add((LogLevel.Error, $"Workflow step {typeof(TWorkflowStep).Name} at stage {stage}: {message}. Exception: {exception}", DateTime.UtcNow));
            }

            return Task.CompletedTask;
        }

        public virtual Task WriteAsync(CancellationToken cancellationToken = default)
        {
            messages.ForEach(m =>
            {
                logger.Log(m.Item1, $"[{m.Item3:O}] {m.Item2}");
            });
            return Task.CompletedTask;
        }
    }
}
