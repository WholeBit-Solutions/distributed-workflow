namespace KafkaWorkflow.Consumer.Base.Workflow
{
    public abstract class MessageWorkflowStep<T, TState> : IMessageWorkflowStep<T, TState>
        where TState : class
    {
        ///<inheritdoc />
        public IMessageWorkflow<T, TState> Workflow { get; }

        protected MessageWorkflowStep(IMessageWorkflow<T, TState> workflow)
        {
            Workflow = workflow;
        }

        ///<inheritdoc />
        public abstract Task<bool> ShouldExecuteAsync(CancellationToken cancellationToken = default);

        ///<inheritdoc />
        public abstract Task ExecuteAsync(CancellationToken cancellationToken = default);

        ///<inheritdoc />
        public virtual Task OnPreExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        ///<inheritdoc />
        public virtual Task OnCompleteAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        ///<inheritdoc />
        public virtual Task<bool> OnErrorAsync(Exception exception, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }
}
