namespace KafkaWorkflow.Consumer.Base.Workflow
{
    /// <summary>
    /// Defines a step within a message workflow, providing asynchronous methods for execution, pre- and
    /// post-processing, conditional execution, and error handling.
    /// </summary>
    /// <remarks>Implementations of this interface enable the composition of complex message processing
    /// workflows by defining discrete, reusable steps. Each step can control its execution flow, perform initialization
    /// and cleanup, and handle errors in a customizable manner. The interface is designed for asynchronous operations
    /// and supports cancellation through the use of cancellation tokens.</remarks>
    /// <typeparam name="T">The type of message processed by the workflow step.</typeparam>
    /// <typeparam name="TState">The type representing the state maintained or modified by the workflow step.</typeparam>
    public interface IMessageWorkflowStep<T, TState>
        where TState : class
    {
        /// <summary>
        /// Gets the workflow that defines the sequence of message processing steps and state transitions.
        /// </summary>
        IMessageWorkflow<T, TState> Workflow { get; }

        /// <summary>
        /// Determines asynchronously whether the associated operation should proceed based on the current context.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the
        /// operation should proceed; otherwise, <see langword="false"/>.</returns>
        Task<bool> ShouldExecuteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs asynchronous operations that must occur before the main execution begins.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the pre-execution operation.</param>
        /// <returns>A task that represents the asynchronous pre-execution operation.</returns>
        Task OnPreExecuteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the asynchronous operation represented by the current instance.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation. The default value is <see
        /// cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous execution operation.</returns>
        Task ExecuteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously completes the current operation, performing any necessary finalization or cleanup.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the completion operation.</param>
        /// <returns>A task that represents the asynchronous completion operation.</returns>
        Task OnCompleteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// This method is called when an exception occurs during the execution of the step.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the workflow should continue or not.</returns>
        Task<bool> OnErrorAsync(Exception exception, CancellationToken cancellationToken = default);
    }
}
