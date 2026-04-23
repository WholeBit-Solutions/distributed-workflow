namespace KafkaWorkflow.Consumer.Base.Workflow
{
    public interface IWorkflowLogger<T, TState>
        where TState : class
    {
        Task CollectAsync(WorkflowStage stage, string message, Exception? exception, CancellationToken cancellationToken = default);
        Task CollectAsync<TWorkflowStep>(WorkflowStage stage, string message, Exception? exception, CancellationToken cancellationToken = default) where TWorkflowStep : IMessageWorkflowStep<T, TState?>;
        Task WriteAsync(CancellationToken cancellationToken = default);
    }
}