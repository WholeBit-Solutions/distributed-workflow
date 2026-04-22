using KafkaWorkflow.Consumer.Base;
using KafkaWorkflow.Consumer.PeopleWorkflow;
using KafkaWorkflow.Consumer.PeopleWorkflow.Steps;
using KafkaWorkflow.WebApi.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KafkaWorkflow.Consumer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.AddServiceDefaults();

            // Add services to the container.
            builder.Services.AddDbContextPool<PeopleContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("People") ?? throw new InvalidOperationException("Connection string 'database' not found.");
                options.UseSqlServer(connectionString);
            });

            builder.AddKafkaConsumer<string, string>("kafka", configureSettings: options =>
            {
                options.Config.GroupId = "people-topic";
            });
            builder.Services.AddScoped<IPersonWorkflow, PersonWorkflow>();
            builder.Services.AddHostedService<ConsumerWorker>();

            //Register workflow and steps
            builder.Services.AddWorkflow<IPersonWorkflow, PersonWorkflow, int, PersonState?>(options =>
            {
                // The execution order of the steps is determined by the order in which they are registered here.
                options.RegisterStep<ValidatePersonStep>();
                options.RegisterStep<ValidateContactStep>();
                options.RegisterStep<ValidateAddressStep>();
            });

            builder.Build().Run();
        }
    }
}
