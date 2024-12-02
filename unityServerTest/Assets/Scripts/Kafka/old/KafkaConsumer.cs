using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using Confluent.Kafka;
using Newtonsoft.Json.Linq;

public class KafkaConsumer : MonoBehaviour
{
    private IConsumer<string, string> consumer;
    private string kafkaTopic = "rover-hsml-data";
    private Vector3 targetPosition;
    private Thread consumerThread;
    private bool isRunning = true;

    // Thread-safe queue to store received positions
    private ConcurrentQueue<Vector3> positionQueue = new ConcurrentQueue<Vector3>();

    void Start()
    {
        // Kafka consumer configuration
        var config = new ConsumerConfig
        {
            BootstrapServers = "192.168.50.133:9092", // Replace with your Kafka server IP
            GroupId = "unity-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(kafkaTopic);

        // Initialize the consumer thread
        consumerThread = new Thread(ReadKafkaMessages);
        consumerThread.Start();
    }

    void Update()
    {
        // Process queued positions in the main thread
        while (positionQueue.TryDequeue(out Vector3 newPosition))
        {
            targetPosition = newPosition;
            Debug.Log($"Updated target position to: {targetPosition}");
        }

        // Smoothly move the GameObject to the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
    }

    private void ReadKafkaMessages()
    {
        while (isRunning)
        {
            try
            {
                // Poll for Kafka messages
                var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(100));
                if (consumeResult != null && !string.IsNullOrEmpty(consumeResult.Value))
                {
                    Debug.Log($"Received Kafka message: {consumeResult.Value}");

                    // Parse the JSON message
                    var message = JObject.Parse(consumeResult.Value);

                    // Extract position data from additionalProperty
                    float x = ExtractPropertyValue(message, "xCoordinate");
                    float y = ExtractPropertyValue(message, "yCoordinate");
                    float z = ExtractPropertyValue(message, "zCoordinate");

                    // Convert from cm to meters
                    x /= 100.0f;
                    y /= 100.0f;
                    z /= 100.0f;

                    // Enqueue the position for the main thread
                    positionQueue.Enqueue(new Vector3(x, y, z));
                }
            }
            catch (ConsumeException e)
            {
                Debug.LogError($"Error consuming Kafka message: {e.Error.Reason}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Unexpected error: {e.Message}");
            }
        }
    }

    private float ExtractPropertyValue(JObject message, string propertyName)
    {
        try
        {
            var properties = message["additionalProperty"] as JArray;
            foreach (var property in properties)
            {
                if (property["name"]?.ToString() == propertyName)
                {
                    return property["value"]?.ToObject<float>() ?? 0f;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error extracting property '{propertyName}': {e.Message}");
        }

        Debug.LogWarning($"Property '{propertyName}' not found in message.");
        return 0f;
    }

    private void OnDestroy()
    {
        // Gracefully shut down the consumer and thread
        isRunning = false;
        consumerThread?.Join();
        consumer?.Close();
        consumer?.Dispose();
    }
}
