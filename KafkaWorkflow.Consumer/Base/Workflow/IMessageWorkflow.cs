namespace KafkaWorkflow.Consumer.Base.Workflow
{
    public interface IMessageWorkflow<T, TState>
    {
        IReadOnlyCollection<IMessageWorkflowStep<T, TState?>> Steps { get; set; }

        TState State { get; set; }

        Task ExecuteAsync(T message, CancellationToken cancellationToken = default);
    }
}
