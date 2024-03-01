using Confluent.Kafka;

namespace kafkadotnet.Infrastructures.kafka;

public class KafkaConsumer
{
    public static Object? Consume(string topic)
    {
        var brokerList = "127.0.0.1:9092";
        var conf = new ConsumerConfig
        {
            GroupId = "email_consumer_group",
            BootstrapServers = brokerList,
            // EnableAutoOffsetStore = false,
            EnableAutoCommit = true,
            StatisticsIntervalMs = 5000,
            SessionTimeoutMs = 6000,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true,
            PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky
        };


        // Note: If a key or value deserializer is not set (as is the case below), the 
        // deserializer corresponding to the appropriate type from Confluent.Kafka.Deserializers
        // will be used automatically (where available). The default deserializer for string
        // is UTF8. The default deserializer for Ignore returns null for all input data
        // (including non-null data).
        using (var consumer = new ConsumerBuilder<Ignore, string>(conf)
            // Note: All handlers are called on the main .Consume thread.
            .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
            .SetStatisticsHandler((_, json) => Console.WriteLine($"Statistics: {json}"))
            .SetPartitionsAssignedHandler((c, partitions) =>
            {
                // Since a cooperative assignor (CooperativeSticky) has been configured, the
                // partition assignment is incremental (adds partitions to any existing assignment).
                // Console.WriteLine(
                //     "Partitions incrementally assigned: [" +
                //     string.Join(',', partitions.Select(p => p.Partition.Value)) +
                //     "], all: [" +
                //     string.Join(',', c.Assignment.Concat(partitions).Select(p => p.Partition.Value)) +
                //     "]");

                // Possibly manually specify start offsets by returning a list of topic/partition/offsets
                // to assign to, e.g.:
                // return partitions.Select(tp => new TopicPartitionOffset(tp, externalOffsets[tp]));
            })
            .SetPartitionsRevokedHandler((c, partitions) =>
            {
                // Since a cooperative assignor (CooperativeSticky) has been configured, the revoked
                // assignment is incremental (may remove only some partitions of the current assignment).
                var remaining = c.Assignment.Where(atp => partitions.Where(rtp => rtp.TopicPartition == atp).Count() == 0);
                // Console.WriteLine(
                //     "Partitions incrementally revoked: [" +
                //     string.Join(',', partitions.Select(p => p.Partition.Value)) +
                //     "], remaining: [" +
                //     string.Join(',', remaining.Select(p => p.Partition.Value)) +
                //     "]");
            })
            .SetPartitionsLostHandler((c, partitions) =>
            {
                // The lost partitions handler is called when the consumer detects that it has lost ownership
                // of its assignment (fallen out of the group).
                // Console.WriteLine($"Partitions were lost: [{string.Join(", ", partitions)}]");
            })
            .Build())
        {
            consumer.Subscribe(topic);

            try
            {
                while (true)
                {
                    try
                    {
                        var cancellationToken = new CancellationTokenSource();
                        var consumeResult = consumer.Consume(cancellationToken.Token);

                        if (consumeResult.IsPartitionEOF)
                        {
                            Console.WriteLine(
                                $"Reached end of topic {consumeResult.Topic}, partition {consumeResult.Partition}, offset {consumeResult.Offset}.");

                            break;
                        }

                        Console.WriteLine($"Send email: {consumeResult.Message.Value}");

                        try
                        {
                            // Store the offset associated with consumeResult to a local cache. Stored offsets are committed to Kafka by a background thread every AutoCommitIntervalMs. 
                            // The offset stored is actually the offset of the consumeResult + 1 since by convention, committed offsets specify the next message to consume. 
                            // If EnableAutoOffsetStore had been set to the default value true, the .NET client would automatically store offsets immediately prior to delivering messages to the application. 
                            // Explicitly storing offsets after processing gives at-least once semantics, the default behavior does not.
                            // consumer.StoreOffset(consumeResult);
                            consumer.Commit();
                        }
                        catch (KafkaException e)
                        {
                            Console.WriteLine($"Store Offset error: {e.Error.Reason}");
                        }
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Consume error: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Closing consumer.");
                consumer.Close();
            }
        }
        return null;
    }
}