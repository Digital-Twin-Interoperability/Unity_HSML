using System;
using System.Threading;
using UnityEngine;
using Confluent.Kafka;
using Newtonsoft.Json.Linq;

public class KafkaConsumer : MonoBehaviour
{
    private IConsumer<Ignore, string> consumer;
    private Thread kafkaThread;
    private bool isRunning = true;

    // Public variable to select the GameObject to move
    public GameObject targetObject;

    // Kafka server details
    private string bootstrapServers = "192.168.50.133:9092"; // Your Kafka server IP
    private string topic = "rover-hsml-data"; // The Kafka topic to read from

    // Variables to store the latest received coordinates
    private Vector3 newPosition;
    private bool isDataUpdated = false;
    private SynchronizationContext unityContext;

    void Start()
    {
        // Capture the Unity main thread context
        unityContext = SynchronizationContext.Current;

        // Kafka consumer configuration
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "unity-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest // Start reading from the earliest offset if no previous offset is found
        };

        // Create the Kafka consumer
        consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        // Start a background thread to consume messages
        kafkaThread = new Thread(() =>
        {
            try
            {
                consumer.Subscribe(topic);

                while (isRunning)
                {
                    // Poll for new messages from the Kafka topic
                    var consumeResult = consumer.Consume();

                    if (consumeResult != null)
                    {
                        // Handle the received message (parse the JSON and extract xyz data)
                        ProcessMessage(consumeResult.Message.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in Kafka consumer: {e.Message}");
            }
            finally
            {
                consumer.Close();
            }
        });

        kafkaThread.Start();
    }

    // Process the incoming message and extract xyz coordinates
    void ProcessMessage(string message)
    {
        try
        {
            // Parse the incoming JSON message
            JObject json = JObject.Parse(message);

            // Ensure additionalProperty exists and is an array
            if (json["additionalProperty"] is JArray additionalProperties)
            {
                float x = 0f, y = 0f, z = 0f;
                bool xFound = false, yFound = false, zFound = false;

                // Extract x, y, z values
                foreach (var prop in additionalProperties)
                {
                    if (prop["name"] == null || prop["value"] == null)
                        continue;

                    string name = (string)prop["name"];
                    float value = (float)prop["value"];

                    if (name == "xCoordinate")
                    {
                        x = value;
                        xFound = true;
                    }
                    else if (name == "yCoordinate")
                    {
                        y = value;
                        yFound = true;
                    }
                    else if (name == "zCoordinate")
                    {
                        z = value;
                        zFound = true;
                    }
                }

                // Update newPosition only if all coordinates were found
                if (xFound && yFound && zFound)
                {
                    newPosition = new Vector3(x, y, z);
                    isDataUpdated = true;
                }
                else
                {
                    Debug.LogWarning("Incomplete position data in message.");
                }
            }
            else
            {
                Debug.LogWarning("additionalProperty array not found in JSON message.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing message: {e.Message}");
        }
    }

    void Update()
    {
        // Only apply the new position if data has been updated
        if (isDataUpdated && targetObject != null)
        {
            targetObject.transform.position = newPosition;
            isDataUpdated = false;  // Reset the flag after applying the update
        }
    }

    void OnDestroy()
    {
        // Clean up the Kafka consumer and stop the thread
        isRunning = false;
        if (kafkaThread != null && kafkaThread.IsAlive)
        {
            kafkaThread.Join();
        }

        consumer?.Close();
    }
}
