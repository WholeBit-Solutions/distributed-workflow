using KafkaWorkflow.Consumer.Base.Workflow;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace KafkaWorkflow.Consumer.Base
{
    public static class WorkflowHelpers
    {
        public static void LogWorkflowExecution<TMessage, TState>(string workflowName, TMessage message, TState? state)
        {
            Console.WriteLine($"Executing workflow '{workflowName}' with message: {message} and state: {state}");
        }

        public static void LogWorkflowCompletion<TMessage, TState>(string workflowName, TMessage message, TState? state)
        {
            Console.WriteLine($"Completed workflow '{workflowName}' with message: {message} and state: {state}");
        }

        public static void LogWorkflowError<TMessage, TState>(string workflowName, TMessage message, TState? state, Exception ex)
        {
            Console.WriteLine($"Error in workflow '{workflowName}' with message: {message} and state: {state}. Exception: {ex}");
        }

        public static void LogWorkflowStateChange<TMessage, TState>(string workflowName, TMessage message, TState? oldState, TState? newState)
        {
            Console.WriteLine($"Workflow '{workflowName}' state changed for message: {message}. Old state: {oldState}, New state: {newState}");
        }

        public static void AddWorkflow<TWorkflow, TWorkflowImpl, T, TState>(this IServiceCollection services, Action<WorkflowOptions<T, TState>> configure)
            where TWorkflow : class, IMessageWorkflow<T, TState>
            where TWorkflowImpl : class, TWorkflow, new()
        {
            services.AddSingleton<TWorkflow, TWorkflowImpl>(sp =>
            {
                var steps = new List<Type>();
                var options = new WorkflowOptions<T, TState>(steps);
                configure(options);
                var workflow = ActivatorUtilities.CreateInstance<TWorkflowImpl>(sp);

                var wfSteps = new List<IMessageWorkflowStep<T, TState?>>();
                steps.ForEach(stepType =>
                {
                    var step = (IMessageWorkflowStep<T, TState?>)ActivatorUtilities.CreateInstance(sp, stepType, workflow);
                    wfSteps.Add(step);
                });

                workflow.Steps = wfSteps;

                return workflow;
            });
        }
    }

    public class WorkflowOptions<T, TState>(IList<Type> workflowSteps)
    {
        public void RegisterStep<TWorkflowStep>()
            where TWorkflowStep : class, IMessageWorkflowStep<T, TState?>
        {
            workflowSteps.Add(typeof(TWorkflowStep));
        }
    }
}
