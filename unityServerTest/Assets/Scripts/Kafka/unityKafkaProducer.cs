using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Confluent.Kafka;
using Newtonsoft.Json.Linq;
using System.Numerics; // For System.Numerics.Quaternion

public class UnityKafkaProducer : MonoBehaviour
{
    private IProducer<string, string> producer;
    private string kafkaTopic = "unity-hsml-topic";

    private UnityEngine.Vector3 lastPosition;
    private UnityEngine.Quaternion lastRotation;

    void Start()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "192.168.50.133:9092" // Replace with your Kafka server IP
        };

        producer = new ProducerBuilder<string, string>(config).Build();

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        Debug.Log("Kafka producer initialized.");
    }

    void Update()
    {
        if (HasTransformChanged())
        {
            SendHSMLMessage();

            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
    }

    private bool HasTransformChanged()
    {
        return transform.position != lastPosition || transform.rotation != lastRotation;
    }

    private string GenerateUniqueSchemaId()
    {
        string inputData = $"{gameObject.name}_{transform.position}_{DateTime.Now.Ticks}";
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(inputData));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }

    private void SendHSMLMessage()
    {
        try
        {
            string schemaId = GenerateUniqueSchemaId();
            UnityEngine.Quaternion adjustedRotation = AdjustRotationAxis(transform.rotation);

            JObject hsmlMessage = new JObject
            {
                { "@context", "https://www.dropbox.com/scl/fi/fxyex9ndqp05316jyfexn/hsml.jsonld?rlkey=tmsoqgidjcfe4f4oa736tisfr&st=fpcpveff&dl=1" },
                { "@type", "Agent" },
                { "name", gameObject.name },
                {
                    "identifier", new JObject
                    {
                        { "@type", "schema:PropertyValue" },
                        { "propertyID", "schema_id" },
                        { "value", $"{gameObject.name}-001" }
                    }
                },
                { "url", $"https://example.com/models/{gameObject.name}-001" },
                {
                    "creator", new JObject
                    {
                        { "@type", "Person" },
                        { "name", "Jared Carrillo" }
                    }
                },
                { "dateCreated", DateTime.UtcNow.ToString("MM-dd-yy") },
                { "dateModified", DateTime.UtcNow.ToString("MM-dd-yy") },
                { "encodingFormat", "application/x-obj" },
                { "contentUrl", "https://example.com/models/3dmodel-001.obj" },
                { "platform", "Unity" },
                {
                    "spaceLocation", new JArray
                    {
                        new JObject
                        {
                            { "@type", "Hyperspace" },
                            { "name", "Moon" },
                            { "id", "did:example:Hyperspace123" }
                        }
                    }
                },
                { "description", "Unity game object data" },
                {
                    "position", new JArray
                    {
                        new JObject { { "@type", "schema:PropertyValue" }, { "name", "xCoordinate" }, { "value", transform.position.x } },
                        new JObject { { "@type", "schema:PropertyValue" }, { "name", "yCoordinate" }, { "value", transform.position.y } },
                        new JObject { { "@type", "schema:PropertyValue" }, { "name", "zCoordinate" }, { "value", transform.position.z } }
                    }
                },
                {
                    "rotation", new JArray
                    {
                        new JObject { { "@type", "schema:PropertyValue" }, { "name", "rx" }, { "value", adjustedRotation.x } },
                        new JObject { { "@type", "schema:PropertyValue" }, { "name", "ry" }, { "value", adjustedRotation.y } },
                        new JObject { { "@type", "schema:PropertyValue" }, { "name", "rz" }, { "value", adjustedRotation.z } }
                    }
                },
                {
                    "additionalProperty", new JArray
                    {
                        new JObject { { "@type", "schema:PropertyValue" }, { "name", "w" }, { "value", adjustedRotation.w } }
                    }
                }
            };

            string message = hsmlMessage.ToString();
            producer.Produce(kafkaTopic, new Message<string, string> { Key = schemaId, Value = message });
            Debug.Log($"Sent HSML message: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending HSML message: {e.Message}");
        }
    }

    private UnityEngine.Quaternion AdjustRotationAxis(UnityEngine.Quaternion rotation)
    {
        var originalRotQuat = new System.Numerics.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

        var rotationXQuat = System.Numerics.Quaternion.CreateFromAxisAngle(new System.Numerics.Vector3(1, 0, 0), (float)-Math.PI / 2);
        var rotationYQuat = System.Numerics.Quaternion.CreateFromAxisAngle(new System.Numerics.Vector3(0, 1, 0), (float)Math.PI);

        var worldRotation = System.Numerics.Quaternion.Multiply(rotationYQuat, rotationXQuat);
        worldRotation = System.Numerics.Quaternion.Multiply(originalRotQuat, worldRotation);
        worldRotation = System.Numerics.Quaternion.Multiply(rotationXQuat, worldRotation);

        return new UnityEngine.Quaternion(-worldRotation.X, -worldRotation.Y, worldRotation.Z, worldRotation.W);
    }

    private void OnDestroy()
    {
        producer?.Dispose();
    }
}
