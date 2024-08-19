using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;

public class SocketClient7 : MonoBehaviour
{
    private const int port = 1234;
    private const string serverIP = "192.168.137.1";

    private Socket clientSocket;
    private byte[] receiveBuffer = new byte[2048];

    public GameObject targetObject1, targetObject3, targetObject4;
    public GameObject cadreRover, Rock1, Rock2, Rock3;
    private string latestJsonMessage;
    private float messageInterval = 0.07f;
    private float timeSinceLastMessage = 0f;

    void Start()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ConnectToServer();
    }

    void Update()
    {
        timeSinceLastMessage += Time.deltaTime;

        if (timeSinceLastMessage >= messageInterval)
        {
            string jsonMessage = ConstructJsonMessage();
            SendMessageToServer(jsonMessage);
            timeSinceLastMessage = 0f;
        }

        if (!string.IsNullOrEmpty(latestJsonMessage))
        {
            UpdateGameObjects(latestJsonMessage);
        }
    }

    private string ConstructJsonMessage()
    {
        JObject jsonMessage = new JObject
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "3DModel",
            ["name"] = new JObject { ["@type"] = "Text", ["@value"] = "YourModelName" },
            ["identifier"] = new JObject
            {
                ["@type"] = "PropertyValue",
                ["propertyID"] = new JObject { ["@type"] = "Text", ["@value"] = "YourPropertyID" },
                ["value"] = new JObject { ["@type"] = "Text", ["@value"] = "YourIdentifierValue" }
            },
            ["url"] = new JObject { ["@type"] = "URL", ["@value"] = "http://example.com/3DModel" },
            ["creator"] = new JObject
            {
                ["@type"] = "Person",
                ["name"] = new JObject { ["@type"] = "Text", ["@value"] = "Creator Name" }
            },
            ["dateCreated"] = new JObject { ["@type"] = "Date", ["@value"] = DateTime.Now.ToString("yyyy-MM-dd") },
            ["dateModified"] = new JObject { ["@type"] = "Date", ["@value"] = DateTime.Now.ToString("yyyy-MM-dd") },
            ["encodingFormat"] = new JObject { ["@type"] = "Text", ["@value"] = "application/json" },
            ["contentUrl"] = new JObject { ["@type"] = "URL", ["@value"] = "http://example.com/3DModel.json" },
            ["additionalType"] = new JObject { ["@type"] = "URL", ["@value"] = "http://example.com/3DModelType" }
        };

        JArray additionalProperties = new JArray();

        additionalProperties.Add(CreatePropertyValue("latitude", cadreRover.transform.position.x));
        additionalProperties.Add(CreatePropertyValue("longitude", cadreRover.transform.position.z));
        additionalProperties.Add(CreatePropertyValue("elevation", cadreRover.transform.position.y));
        additionalProperties.Add(CreatePropertyValue("xCoordinate", cadreRover.transform.position.x));
        additionalProperties.Add(CreatePropertyValue("yCoordinate", cadreRover.transform.position.y));
        additionalProperties.Add(CreatePropertyValue("zCoordinate", cadreRover.transform.position.z));

        Quaternion rotation = cadreRover.transform.rotation;
        additionalProperties.Add(CreatePropertyValue("roll", rotation.eulerAngles.x));
        additionalProperties.Add(CreatePropertyValue("pitch", rotation.eulerAngles.y));
        additionalProperties.Add(CreatePropertyValue("yaw", rotation.eulerAngles.z));

        // You can include other properties like volume, scale, etc. here.
        additionalProperties.Add(CreatePropertyValue("volume", 1.0f, "CMT"));
        additionalProperties.Add(CreatePropertyValue("scaleX", cadreRover.transform.localScale.x));
        additionalProperties.Add(CreatePropertyValue("scaleY", cadreRover.transform.localScale.y));
        additionalProperties.Add(CreatePropertyValue("scaleZ", cadreRover.transform.localScale.z));

        jsonMessage["additionalProperty"] = additionalProperties;

        jsonMessage["description"] = new JObject { ["@type"] = "Text", ["@value"] = "A description of your 3D model." };

        return jsonMessage.ToString();
    }

    private JObject CreatePropertyValue(string name, float value, string unitCode = null)
    {
        JObject propertyValue = new JObject
        {
            ["@type"] = "PropertyValue",
            ["name"] = name,
            ["value"] = new JObject { ["@type"] = "Number", ["@value"] = value }
        };

        if (!string.IsNullOrEmpty(unitCode))
        {
            propertyValue["unitCode"] = unitCode;
        }

        return propertyValue;
    }

    private void ConnectToServer()
    {
        try
        {
            clientSocket.Connect(IPAddress.Parse(serverIP), port);
            clientSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ReceiveCallback, null);
        }
        catch (Exception e)
        {
            Debug.Log("Socket exception: " + e.ToString());
        }
    }

    private void SendMessageToServer(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            clientSocket.Send(data);
        }
        catch (Exception e)
        {
            Debug.Log("Send message exception: " + e.ToString());
        }
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        try
        {
            int received = clientSocket.EndReceive(AR);
            if (received > 0)
            {
                byte[] data = new byte[received];
                Array.Copy(receiveBuffer, data, received);
                latestJsonMessage = Encoding.UTF8.GetString(data);

                // Parse the received JSON message and print it
                PrintJsonMessage(latestJsonMessage);
            }
            clientSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ReceiveCallback, null);
        }
        catch (Exception e)
        {
            Debug.Log("Receive callback exception: " + e.ToString());
        }
    }

    private void PrintJsonMessage(string message)
    {
        try
        {
            JObject jsonMessage = JObject.Parse(message);
            Debug.Log("Received JSON data: " + jsonMessage.ToString());
        }
        catch (Exception e)
        {
            Debug.Log("Error parsing JSON message: " + e.ToString());
        }
    }

    private void UpdateGameObjects(string message)
    {
        JObject jsonMessage = JObject.Parse(message);
        ParseAndUpdateGameObject(jsonMessage, "cadrePrim", cadreRover);
        ParseAndUpdateGameObject(jsonMessage, "Rock1Prim", Rock1);
        ParseAndUpdateGameObject(jsonMessage, "Rock2Prim", Rock2);
        ParseAndUpdateGameObject(jsonMessage, "Rock3Prim", Rock3);
    }

    private void ParseAndUpdateGameObject(JObject jsonMessage, string key, GameObject gameObject)
    {
        if (jsonMessage[key] != null && jsonMessage[key].Type == JTokenType.Object)
        {
            JObject objectData = (JObject)jsonMessage[key];

            // Extract position and rotation information from the object data
            Vector3 position = new Vector3(
                objectData["xCoordinate"].Value<float>(),
                objectData["yCoordinate"].Value<float>(),
                objectData["zCoordinate"].Value<float>()
            );

            float roll = objectData["roll"].Value<float>();
            float pitch = objectData["pitch"].Value<float>();
            float yaw = objectData["yaw"].Value<float>();

            Quaternion rotation = Quaternion.Euler(roll, pitch, yaw);

            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;
        }
    }

    private void OnDestroy()
    {
        if (clientSocket != null && clientSocket.Connected)
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }
}
