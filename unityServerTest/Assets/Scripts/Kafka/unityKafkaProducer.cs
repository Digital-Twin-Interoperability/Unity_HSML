using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Confluent.Kafka;
using Newtonsoft.Json.Linq;
using System.Numerics; // For System.Numerics.Quaternion

public class unityKafkaProducer : MonoBehaviour
{
    private IProducer<string, string> producer;
    private string kafkaTopic = "unity-hsml-topic";

    // Variables to store the last sent position and rotation
    private UnityEngine.Vector3 lastPosition;
    private UnityEngine.Quaternion lastRotation;

    void Start()
    {
        // Kafka producer configuration
        var config = new ProducerConfig
        {
            BootstrapServers = "192.168.50.133:9092" // Replace with your Kafka server IP
        };

        producer = new ProducerBuilder<string, string>(config).Build();

        // Initialize last position and rotation
        lastPosition = transform.position;
        lastRotation = transform.rotation;

        Debug.Log("Kafka producer initialized.");
    }

    void Update()
    {
        // Check for changes in position or rotation
        if (HasTransformChanged())
        {
            SendFullHSMLMessage();

            // Update the last known position and rotation
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
    }

    // Function to check if the transform has changed
    private bool HasTransformChanged()
    {
        return transform.position != lastPosition || transform.rotation != lastRotation;
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

            // Adjust the rotation using the provided quaternion transformation
            UnityEngine.Quaternion adjustedRotation = AdjustRotationAxis(transform.rotation);

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
                        new JObject { { "@type", "PropertyValue" }, { "name", "yCoordinate" }, { "value", transform.position.z } },
                        new JObject { { "@type", "PropertyValue" }, { "name", "zCoordinate" }, { "value", transform.position.y } },
                        new JObject { { "@type", "PropertyValue" }, { "name", "rx" }, { "value", adjustedRotation.x } },
                        new JObject { { "@type", "PropertyValue" }, { "name", "ry" }, { "value", adjustedRotation.y } },
                        new JObject { { "@type", "PropertyValue" }, { "name", "rz" }, { "value", adjustedRotation.z } },
                        new JObject { { "@type", "PropertyValue" }, { "name", "w" }, { "value", adjustedRotation.w } }
                    }
                },
                { "description", "Continuous game object data with full schema" }
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

    // Function to adjust the rotation quaternion before sending it
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
        // Dispose of the Kafka producer when the object is destroyed
        producer?.Dispose();
    }
}
