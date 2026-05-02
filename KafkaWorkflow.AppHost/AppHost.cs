using Confluent.Kafka;
using Confluent.Kafka.Admin;

var builder = DistributedApplication.CreateBuilder(args);

var initScriptPath = Path.Join(Path.GetDirectoryName(typeof(Program).Assembly.Location), "initSql.sql");
var password = builder.AddParameter("sql-password", secret: true, value: "ZSPBkfWD+0nACtUs.Urp1y");

var sqlserver = builder.AddSqlServer("database")
    .WithImage("mssql/server", "2025-latest")
    .WithDataVolume(name: "sqlserver-data", isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint(targetPort: 1433, port: 55245, name: "sqlserver-endpoint", isProxied: false)
    .WithPassword(password)
    .AddDatabase("People")
    .WithCreationScript(File.ReadAllText(initScriptPath));

var kafka = builder.AddKafka("kafka")
    .WithDataVolume(name: "kafka-data", isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithKafkaUI();

var webapi = builder.AddProject<Projects.KafkaWorkflow_WebApi>("webapi")
    .WithReference(sqlserver)
    .WithReference(kafka)
    .WaitFor(sqlserver)
    .WaitFor(kafka);

var kafkaConsumer = builder.AddProject<Projects.KafkaWorkflow_Consumer>("kafka-consumer")
    .WithReference(sqlserver)
    .WithReference(kafka)
    .WaitFor(sqlserver)
    .WaitFor(kafka);

ConfigureKafkaTopic();

builder.Build().Run();

void ConfigureKafkaTopic()
{
    builder.Eventing.Subscribe<ResourceReadyEvent>(kafka.Resource, async (@event, ct) =>
    {
        var cs = await kafka.Resource.ConnectionStringExpression.GetValueAsync(ct);

        var config = new AdminClientConfig
        {
            BootstrapServers = cs
        };

        using var adminClient = new AdminClientBuilder(config).Build();
        try
        {
            await adminClient.CreateTopicsAsync(
            [
                new TopicSpecification { Name = "people-topic", NumPartitions = 1, ReplicationFactor = 1 },
                new TopicSpecification { Name = "contact-topic", NumPartitions = 1, ReplicationFactor = 1 },
                new TopicSpecification { Name = "address-topic", NumPartitions = 1, ReplicationFactor = 1 },
            ]);
        }
        catch (CreateTopicsException e)
        {
            Console.WriteLine($"An error occurred creating topic: {e.Message}");
        }
    });
}
