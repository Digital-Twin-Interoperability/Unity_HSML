using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Confluent.Kafka;
using Newtonsoft.Json.Linq;

public class KafkaHSMLSender : MonoBehaviour
{
    private IProducer<string, string> producer;
    private string kafkaTopic = "rover-hsml-data";

    void Start()
    {
        // Kafka producer configuration
        var config = new ProducerConfig
        {
            BootstrapServers = "192.168.50.133:9092" // Replace with your Kafka server IP
        };

        producer = new ProducerBuilder<string, string>(config).Build();

        // Send full HSML message on start
        SendFullHSMLMessage();
    }

    void Update()
    {
        // Continuously update position and rotation (delta updates) in each frame
        SendDeltaHSMLMessage();
    }

    // Function to generate a unique schema ID based on the GameObject's name and data
    string GenerateUniqueSchemaId()
    {
        // Create a hash based on the GameObject's name and current position
        string inputData = $"{gameObject.name}_{transform.position}_{DateTime.Now.Ticks}";
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(inputData));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2")); // Convert bytes to hexadecimal
            }
            return sb.ToString(); // Return the unique hash as a schema ID
        }
    }

    // Function to send the full HSML message (schema and all details)
    void SendFullHSMLMessage()
    {
        try
        {
            string schemaId = GenerateUniqueSchemaId(); // Generate unique schema ID
            JObject hsmlMessage = new JObject
            {
                { "@context", "https://schema.org" },
                { "@type", "3DModel" },
                { "name", gameObject.name },
                { "identifier", new JObject { { "@type", "PropertyValue" }, { "propertyID", schemaId }, { "value", $"{gameObject.name}-001" } } },
                { "url", $"https://example.com/models/{gameObject.name}-001" },
                { "creator", new JObject { { "@type", "Person" }, { "name", "Your Name" } } },
                { "dateCreated", DateTime.UtcNow.ToString("yyyy-MM-dd") },
                { "dateModified", DateTime.UtcNow.ToString("yyyy-MM-dd") },
                { "encodingFormat", "application/x-obj" },
                { "contentUrl", "https://example.com/models/3dmodel-001.obj" },
                { "additionalType", "https://schema.org/CreativeWork" },
                {
                    "additionalProperty", new JArray
                    {
                        new JObject { { "@type", "PropertyValue" }, { "name", "xCoordinate" }, { "value", transform.position.x } },
                        new JObject { { "@type", "PropertyValue" }, { "name", "yCoordinate" }, { "value", transform.position.y } },
                        new JObject { { "@type", "PropertyValue" }, { "name", "zCoordinate" }, { "value", transform.position.z } },
                        new JObject { { "@type", "PropertyValue" }, { "name", "rx" }, { "value", transform.rotation.x } },
                        new JObject { { "@type", "PropertyValue" }, { "name", "ry" }, { "value", transform.rotation.y } },
                        new JObject { { "@type", "PropertyValue" }, { "name", "rz" }, { "value", transform.rotation.z } },
                        new JObject { { "@type", "PropertyValue" }, { "name", "w" }, { "value", transform.rotation.w } }
                    }
                },
                { "description", "Initial game object data with full schema" }
            };

            string message = hsmlMessage.ToString();
            producer.Produce(kafkaTopic, new Message<string, string> { Key = schemaId, Value = message });
            Debug.Log($"Sent full HSML message: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending full HSML message: {e.Message}");
        }
    }

    // Function to send delta HSML message (only changed fields)
    void SendDeltaHSMLMessage()
    {
        try
        {
            string schemaId = GenerateUniqueSchemaId(); // Use the unique schema ID
            JObject deltaMessage = new JObject
            {
                { "rover", gameObject.name },
                { "delta", new JObject
                    {
                        { "xCoordinate", transform.position.x },
                        { "yCoordinate", transform.position.y },
                        { "zCoordinate", transform.position.z },
                        { "rx", transform.rotation.x },
                        { "ry", transform.rotation.y },
                        { "rz", transform.rotation.z },
                        { "w", transform.rotation.w }
                    }
                }
            };

            string message = deltaMessage.ToString();
            producer.Produce(kafkaTopic, new Message<string, string> { Key = schemaId, Value = message });
            Debug.Log($"Sent delta HSML message: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending delta HSML message: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        // Dispose of the Kafka producer when the object is destroyed
        producer?.Dispose();
    }
}
