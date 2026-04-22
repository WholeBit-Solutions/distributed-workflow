using Confluent.Kafka;
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
                //options.Config.BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? throw new InvalidOperationException("Kafka bootstrap servers not configured.");
                options.Config.GroupId = "people-topic";
                //options.Config.GroupId = "people-group";
                //options.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
                //options.Config.EnableAutoCommit = true;
            });
            builder.Services.AddScoped<IPersonWorkflow, PersonWorkflow>();
            
            builder.Services.AddHostedService<ConsumerWorker>();

            builder.Services.AddWorkflow<IPersonWorkflow, PersonWorkflow, int, PersonState?>(options =>
            {
                options.RegisterStep<ValidatePersonStep>();
                options.RegisterStep<ValidateContactStep>();
                options.RegisterStep<ValidateAddressStep>();
            });

            builder.Build().Run();
        }
    }
}
