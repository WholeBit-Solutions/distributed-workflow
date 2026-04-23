using Confluent.Kafka;
using KafkaWorkflow.Consumer.PeopleWorkflow;
using Microsoft.Extensions.Hosting;

namespace KafkaWorkflow.Consumer
{
    internal sealed class ConsumerWorker(IConsumer<string, string> consumer, IPersonWorkflow messageWorkflow) : BackgroundService
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
                    await messageWorkflow.OnExecuteAsync(id, stoppingToken);
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error occurred: {e.Error.Reason}");
                }
            }
        }
    }
}
