using Confluent.Kafka;
using KafkaWorkflow.Consumer.PeopleWorkflow;
using KafkaWorkflow.WebApi.Db.Entities;
using Microsoft.Extensions.Hosting;

namespace KafkaWorkflow.Consumer.Workers
{
    internal sealed class PeopleWorker(IConsumer<int, Person> consumer, IPersonWorkflow messageWorkflow) : BackgroundService
    {
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            consumer.Subscribe("people-topic");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            consumer.Close();
            return base.StopAsync(cancellationToken);
        }

        // Use consumer...
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    var msg = consumer.Consume(stoppingToken);
                    Console.WriteLine($"Consumed message '{msg.Message.Value}' at: '{msg.TopicPartitionOffset}'.");

                    var id = Convert.ToInt32(msg.Message.Key);
                    await messageWorkflow.OnExecuteAsync(msg.Message.Key, stoppingToken);
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error occurred: {e.Error.Reason}");
                }
            }
        }
    }
}
