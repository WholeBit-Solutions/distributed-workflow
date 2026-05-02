using KafkaWorkflow.Consumer.Base;
using KafkaWorkflow.Consumer.PeopleWorkflow;
using KafkaWorkflow.Consumer.PeopleWorkflow.Steps;
using KafkaWorkflow.Consumer.Workers;
using KafkaWorkflow.ServiceDefaults.KafkaSerialization;
using KafkaWorkflow.WebApi.Db;
using KafkaWorkflow.WebApi.Db.Entities;
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

            builder.AddKafkaConsumer<int, Person>("kafka", configureSettings: options =>
            {
                options.Config.GroupId = "people";
            }, builder =>
            {
                builder.SetValueDeserializer(new KafkaJsonDeserializer<Person>());
            });
            builder.AddKafkaConsumer<int, ContactInfo>("kafka", configureSettings: options =>
            {
                options.Config.GroupId = "people";
            }, builder =>
            {
                builder.SetValueDeserializer(new KafkaJsonDeserializer<ContactInfo>());
            });
            builder.AddKafkaConsumer<int, Address>("kafka", configureSettings: options =>
            {
                options.Config.GroupId = "people";
            }, builder =>
            {
                builder.SetValueDeserializer(new KafkaJsonDeserializer<Address>());
            });
            builder.Services.AddHostedService<PeopleWorker>();
            builder.Services.AddHostedService<ContactWorker>();
            builder.Services.AddHostedService<AddressWorker>();

            //Register workflow and steps
            builder.Services.AddWorkflow<IPersonWorkflow, PersonWorkflow, int, PersonState?>(options =>
            {
                // The execution order of the steps is determined by the order in which they are registered here.
                options.RegisterStep<ValidatePersonStep>();
                options.RegisterStep<ValidateContactStep>();
                options.RegisterStep<ValidateAddressStep>();
            });

            var serviceProvider = builder.Services.BuildServiceProvider();

            builder.Build().Run();


        }
    }
}
