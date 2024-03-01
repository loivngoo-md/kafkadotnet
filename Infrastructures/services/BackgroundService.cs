
using kafkadotnet.Infrastructures.kafka;

namespace kafkadotnet.Infrastructures.Services;
public class KafkaConsumerBackgroundService : BackgroundService
{
    private readonly string topic = "EmailTopic";
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {

        return Task.FromResult(KafkaConsumer.Consume(topic));
    }
}