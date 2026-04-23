namespace KafkaWorkflow.Consumer.Base.Workflow
{
    /// <summary>
    /// Defines a contract for a message processing workflow that operates on messages of a specified type and maintains
    /// an associated state.
    /// </summary>
    /// <remarks>Implementations of this interface allow for the composition and execution of message
    /// processing steps, enabling flexible and extensible workflow definitions. The workflow maintains a state object
    /// that can be accessed and updated as messages are processed. This interface is typically used to coordinate
    /// complex message handling scenarios where multiple processing stages and state management are required.</remarks>
    /// <typeparam name="T">The type of messages that the workflow processes.</typeparam>
    /// <typeparam name="TState">The type of state object associated with the workflow.</typeparam>
    public interface IMessageWorkflow<T, TState>
        where TState : class
    {
        /// <summary>
        /// Gets or sets the collection of steps that define the workflow for processing messages.
        /// </summary>
        /// <remarks>Each step in the collection represents a distinct stage in the message workflow. The
        /// order of steps in the collection determines the sequence in which they are executed. Modifying this
        /// collection affects the workflow's behavior for subsequent message processing.</remarks>
        IReadOnlyCollection<IMessageWorkflowStep<T, TState?>> Steps { get; set; }

        /// <summary>
        /// Gets the state holder associated with the workflow.
        /// </summary>
        IObjectAccessor<TState> StateAccessor { get; }

        /// <summary>
        /// Gets the logger used to record workflow events and state changes for the current instance.
        /// </summary>
        IWorkflowLogger<T, TState> Logger { get; }

        /// <summary>
        /// Asynchronously retrieves the current state object.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        Task<TState?> OnGetStateAsync(T message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the workflow asynchronously for the given message.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task OnExecuteAsync(T message, CancellationToken cancellationToken = default);
    }
}
