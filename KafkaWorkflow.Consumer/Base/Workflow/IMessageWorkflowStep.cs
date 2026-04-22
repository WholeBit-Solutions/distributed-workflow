using System;
using System.Collections.Generic;
using System.Text;

namespace KafkaWorkflow.Consumer.Base.Workflow
{

    public interface IMessageWorkflowStep<T, TState>
    {
        IMessageWorkflow<T, TState> Workflow { get; }
        Task<bool> ShouldExecute();

        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }

    public abstract class MessageWorkflowStep<T, TState> : IMessageWorkflowStep<T, TState>
    {
        public IMessageWorkflow<T, TState> Workflow { get; }

        protected MessageWorkflowStep(IMessageWorkflow<T, TState> workflow)
        {
            Workflow = workflow;
        }

        public abstract Task<bool> ShouldExecute();

        public abstract Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
