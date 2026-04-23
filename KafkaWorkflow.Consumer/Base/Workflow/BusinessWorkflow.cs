using KafkaWorkflow.Consumer.Base.Workflow;

namespace KafkaWorkflow.Consumer.PeopleWorkflow
{
    public abstract class BusinessWorkflow<T, TState>(IObjectAccessor<TState> stateAccessor, IWorkflowLogger<T, TState> logger) : IMessageWorkflow<T, TState>
        where TState : class
    {
        public IReadOnlyCollection<IMessageWorkflowStep<T, TState>> Steps { get; set; }

        public virtual IObjectAccessor<TState> StateAccessor { get; } = stateAccessor;

        public IWorkflowLogger<T, TState> Logger { get; } = logger;

        public virtual async Task OnExecuteAsync(T message, CancellationToken cancellationToken = default)
        {
            var state = await OnGetStateAsync(message, cancellationToken);
            StateAccessor.Value = state;

            Steps.ToList().ForEach(async step =>
            {
                try
                {
                    if (await step.ShouldExecuteAsync())
                    {
                        await Logger.CollectAsync<IMessageWorkflowStep<T, TState?>>(WorkflowStage.PreExecute, "Starting pre-execution of step.", null, cancellationToken);
                        await step.OnPreExecuteAsync(cancellationToken);

                        await Logger.CollectAsync<IMessageWorkflowStep<T, TState?>>(WorkflowStage.Execute, "Starting execution of step.", null, cancellationToken);
                        await step.ExecuteAsync(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    await Logger.CollectAsync<IMessageWorkflowStep<T, TState?>>(WorkflowStage.Execute, "Error during step processing.", ex, cancellationToken);
                    bool shouldContinue = await step.OnErrorAsync(ex, cancellationToken);
                    if (!shouldContinue)
                    {
                        return;
                    }
                }
                finally
                {
                    await step.OnCompleteAsync(cancellationToken);
                    
                    await Logger.CollectAsync<IMessageWorkflowStep<T, TState>>(WorkflowStage.Complete, "Step completed.", null, cancellationToken);
                    await Logger.WriteAsync(cancellationToken);
                }
            });
        }

        public abstract Task<TState?> OnGetStateAsync(T message, CancellationToken cancellationToken = default);
    }
}
