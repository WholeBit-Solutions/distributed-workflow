using Confluent.Kafka;
using Confluent.Kafka.Admin;


var builder = DistributedApplication.CreateBuilder(args);

var initScriptPath = Path.Join(Path.GetDirectoryName(typeof(Program).Assembly.Location), "initSql.sql");

var sqlserver = builder.AddSqlServer("database")
    .WithDataVolume(name: "sqlserver-data", isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(57242)
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

var playwright = builder.AddProject<Projects.KafkaWorkflow_PlaywrightTests>("playwright")
    .WithExplicitStart()
    .WithEnvironment("ASPIRE", "true")
    .WithReference(webapi)
    .WaitFor(webapi);

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
            ]);
        }
        catch (CreateTopicsException e)
        {
            Console.WriteLine($"An error occurred creating topic: {e.Message}");
        }
    });
}
