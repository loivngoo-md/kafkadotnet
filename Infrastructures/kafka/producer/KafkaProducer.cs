using Confluent.Kafka;

namespace kafkadotnet.Infrastructures.kafka;

class KafkaProducer
{
    public static async Task<DeliveryResult<string, string>?> SendToKafka(string topic, string message)
    {
        var config = new ProducerConfig { BootstrapServers = "127.0.0.1:9092" };

        using (var producer = new ProducerBuilder<string, string>(config).Build())
        {
            try
            {
                var mailer = string.Format("User-{0}", Guid.NewGuid().ToString());
                var deliveryResult = await producer.ProduceAsync(topic, new Message<string, string> { Key = mailer, Value = message });
                
                Console.WriteLine($"Delivered '{deliveryResult.Value}' to '{deliveryResult.TopicPartitionOffset}'");
                producer.Flush(TimeSpan.FromSeconds(10));
                return deliveryResult;
            }
            catch (ProduceException<string, string> e)
            {
                //save message into database
                Console.WriteLine($"Oops, something went wrong: {e.Error}");
            }
        }

        return null;
    }
}