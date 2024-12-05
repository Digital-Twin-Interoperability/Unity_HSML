using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using Confluent.Kafka;
using Newtonsoft.Json.Linq;
using System.Numerics; // For System.Numerics.Quaternion and System.Numerics.Vector3

public class unrealKafkaConsumer : MonoBehaviour
{
    private IConsumer<string, string> consumer;
    private string kafkaTopic = "unreal-hsml-topic";
    private UnityEngine.Vector3 targetPosition; // UnityEngine.Vector3 for position
    private UnityEngine.Quaternion targetRotation; // UnityEngine.Quaternion for rotation
    private Thread consumerThread;
    private bool isRunning = true;

    // Thread-safe queues to store positions and rotations
    private ConcurrentQueue<UnityEngine.Vector3> positionQueue = new ConcurrentQueue<UnityEngine.Vector3>(); // UnityEngine.Vector3
    private ConcurrentQueue<UnityEngine.Quaternion> rotationQueue = new ConcurrentQueue<UnityEngine.Quaternion>(); // UnityEngine.Quaternion

    void Start()
    {
        // Kafka consumer configuration
        var config = new ConsumerConfig
        {
            BootstrapServers = "192.168.50.133:9092", // Replace with your Kafka server IP
            GroupId = "unreal-consumer-group",
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
        // Process queued positions and rotations in the main thread
        while (positionQueue.TryDequeue(out UnityEngine.Vector3 newPosition))
        {
            targetPosition = newPosition;
            Debug.Log($"Updated target position to: {targetPosition}");
        }

        while (rotationQueue.TryDequeue(out UnityEngine.Quaternion newRotation))
        {
            // Adjust the rotation using the function provided
            targetRotation = AdjustRotationAxis(newRotation);
            Debug.Log($"Updated target rotation to: {targetRotation.eulerAngles}");
        }

        // Smoothly move the GameObject to the target position
        transform.position = UnityEngine.Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);

        // Apply the rotation to the GameObject
        transform.rotation = UnityEngine.Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
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

                    // Extract position data
                    float x = ExtractPropertyValue(message, "xCoordinate");
                    float y = ExtractPropertyValue(message, "yCoordinate");
                    float z = ExtractPropertyValue(message, "zCoordinate");

                    // Extract quaternion data (rx, ry, rz, w)
                    float rx = ExtractPropertyValue(message, "rx");
                    float ry = ExtractPropertyValue(message, "ry");
                    float rz = ExtractPropertyValue(message, "rz");
                    float w = ExtractPropertyValue(message, "w");

                    // Convert from cm to meters for position
                    x /= 1.0f;
                    y /= 1.0f;
                    z /= 1.0f;

                    // Enqueue the position for the main thread
                    positionQueue.Enqueue(new UnityEngine.Vector3(x, z, y));

                    // Convert quaternion values to a UnityEngine.Quaternion object and enqueue it
                    UnityEngine.Quaternion rotation = new UnityEngine.Quaternion(rx, ry, rz, w);
                    rotationQueue.Enqueue(rotation);
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

    // Function to adjust the rotation as per your requirements
    private UnityEngine.Quaternion AdjustRotationAxis(UnityEngine.Quaternion rotation)
    {
        // Convert UnityEngine Quaternion to System.Numerics Quaternion for manipulation
        var originalRotQuat = new System.Numerics.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

        // Apply the rotations based on the given axis and angle
        var rotationXQuat = System.Numerics.Quaternion.CreateFromAxisAngle(new System.Numerics.Vector3(1, 0, 0), (float)-Math.PI / 2);
        var rotationYQuat = System.Numerics.Quaternion.CreateFromAxisAngle(new System.Numerics.Vector3(0, 1, 0), (float)Math.PI);

        var worldRotation = System.Numerics.Quaternion.Multiply(rotationYQuat, rotationXQuat);
        worldRotation = System.Numerics.Quaternion.Multiply(originalRotQuat, worldRotation);
        worldRotation = System.Numerics.Quaternion.Multiply(rotationXQuat, worldRotation);

        // Convert back to UnityEngine Quaternion and return
        return new UnityEngine.Quaternion(-worldRotation.X, -worldRotation.Y, worldRotation.Z, worldRotation.W);
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
