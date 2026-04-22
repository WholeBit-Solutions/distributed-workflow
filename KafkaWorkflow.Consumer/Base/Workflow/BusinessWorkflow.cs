using KafkaWorkflow.Consumer.Base.Workflow;

namespace KafkaWorkflow.Consumer.PeopleWorkflow
{
    public abstract class BusinessWorkflow<T, TState> : IMessageWorkflow<T, TState?>
    {
        public IReadOnlyCollection<IMessageWorkflowStep<T, TState?>> Steps { get; set; }

        public virtual TState? State { get; set; }

        public virtual async Task ExecuteAsync(T message, CancellationToken cancellationToken = default)
        {
            Steps.ToList().ForEach(async step =>
            {
                if (await step.ShouldExecute())
                {
                    await step.ExecuteAsync(cancellationToken);
                }
            });
        }
    }
}
