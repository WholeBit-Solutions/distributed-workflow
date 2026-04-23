using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.Consumer.PeopleWorkflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            where TWorkflowImpl : class, TWorkflow
            where TState : class
        {
            services.AddSingleton<ILogger, Logger<TWorkflowImpl>>();
            services.AddSingleton<IWorkflowLogger<int, PersonState?>, WorkflowLogger<int, PersonState?>>();
            services.AddSingleton<IMessageWorkflow<T, TState>, TWorkflowImpl>();
            services.AddSingleton<IObjectAccessor<TState>, ObjectAccessor<TState>>();

            services.AddSingleton<TWorkflow, TWorkflowImpl>(sp =>
            {
                var steps = new List<Type>();
                var options = new WorkflowOptions<T, TState>(steps);
                configure(options);

                //var tp = typeof(TWorkflowImpl).con.GetConstructors()[0].GetParameters();

                var innerLogger = sp.GetRequiredService<ILogger<TWorkflowImpl>>();
                //var logger = ActivatorUtilities.CreateInstance<WorkflowLogger<T, TState?>>(sp, innerLogger);

                //var workflow = ActivatorUtilities.CreateInstance<TWorkflowImpl>(sp, innerLogger);
                var workflow = sp.GetRequiredService<IMessageWorkflow<T, TState>>();

                var wfSteps = new List<IMessageWorkflowStep<T, TState?>>();
                steps.ForEach(stepType =>
                {
                    var step = (IMessageWorkflowStep<T, TState?>)ActivatorUtilities.CreateInstance(sp, stepType, workflow);
                    wfSteps.Add(step);
                });

                workflow.Steps = wfSteps;

                return (TWorkflowImpl)workflow;
            });
        }
    }

    public class WorkflowOptions<T, TState>(IList<Type> workflowSteps)
        where TState : class
    {
        public void RegisterStep<TWorkflowStep>()
            where TWorkflowStep : class, IMessageWorkflowStep<T, TState?>
        {
            workflowSteps.Add(typeof(TWorkflowStep));
        }
    }
}
