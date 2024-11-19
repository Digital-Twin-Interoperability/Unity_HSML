using UnityEngine;
using Confluent.Kafka;
using System;
using System.Threading.Tasks;

public class KafkaProducer : MonoBehaviour
{
    private IProducer<Null, string> producer;

    void Start()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "192.168.50.133:9092"
        };

        producer = new ProducerBuilder<Null, string>(config).Build();

        // Start streaming data every 1 second
        InvokeRepeating(nameof(SendDataToKafka), 1.0f, 1.0f);
    }

    async void SendDataToKafka()
    {
        string message = "This is data from Unity: " + DateTime.Now.ToString();

        try
        {
            var deliveryResult = await producer.ProduceAsync("unity-data", new Message<Null, string> { Value = message });
            Debug.Log($"Delivered message to {deliveryResult.TopicPartitionOffset}");
        }
        catch (ProduceException<Null, string> e)
        {
            Debug.LogError($"Error producing message: {e.Error.Reason}");
        }
    }

    private void OnDestroy()
    {
        // Dispose of the producer when the object is destroyed
        producer.Dispose();
    }
}
