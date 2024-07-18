using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;  // Ensure Newtonsoft.Json is included in your Unity project

public class SocketClient6 : MonoBehaviour
{
    private const int port = 1234;
    private const string serverIP = "192.168.137.1";

    private Socket clientSocket;
    private byte[] receiveBuffer = new byte[2048]; // Increased buffer size to handle larger data

    public GameObject targetObject1, targetObject3, targetObject4;
    public GameObject cadreRover, Rock1, Rock2, Rock3, rightWheel, leftWheel;
    private string latestJsonMessage;  // Variable to store the latest JSON message
    private float messageInterval = 0.07f; // Interval in seconds between messages
    private float timeSinceLastMessage = 0f;

    void Start()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ConnectToServer();
    }

    void Update()
    {
        // Increment the time since the last message
        timeSinceLastMessage += Time.deltaTime;

        // Check if the interval has passed
        if (timeSinceLastMessage >= messageInterval)
        {
            // Create a JSON string to hold all objects' data
            string jsonMessage = "{";
            jsonMessage += GetObjectDataAsJson(targetObject1, "Viper") + ",";
            // Add rotation data for leftWheel
            jsonMessage += GetRotationDataAsJson(leftWheel, "LeftWheel") + ",";
            // Add rotation data for rightWheel
            jsonMessage += GetRotationDataAsJson(rightWheel, "RightWheel");
            jsonMessage += "}";

            // Send the JSON message to the server
            SendMessageToServer(jsonMessage);

            // Reset the timer
            timeSinceLastMessage = 0f;
        }

        // Update game objects based on the latest JSON message
        if (!string.IsNullOrEmpty(latestJsonMessage))
        {
            UpdateGameObjects(latestJsonMessage);
        }
    }


    private string GetObjectDataAsJson(GameObject obj, string objectName)
    {
        if (obj != null)
        {
            Vector3 position = AdjustPositionAxis(obj.transform.position);
            Quaternion rotation = AdjustRotationAxis(obj.transform.rotation);

            string positionJson = $"\"position\": {{\"x\": {position.x}, \"y\": {position.y}, \"z\": {position.z}}}";
            string rotationJson = $"\"rotation\": {{\"x\": {rotation.x}, \"y\": {rotation.y}, \"z\": {rotation.z}, \"w\": {rotation.w}}}";

            return $"\"{objectName}\": {{ {positionJson}, {rotationJson} }}";
        }
        return $"\"{objectName}\": {{}}";
    }

    private string GetRotationDataAsJson(GameObject obj, string objectName)
    {
        if (obj != null)
        {
            Quaternion rotation = obj.transform.rotation;
            float yRotation = rotation.eulerAngles.y; // Get the y-axis rotation angle

            string rotationJson = $"\"rotation\": {{\"y\": {yRotation}}}";
            return $"\"{objectName}\": {{ {rotationJson} }}";
        }
        return $"\"{objectName}\": {{}}";
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
        if (jsonMessage[key] != null && jsonMessage[key].Type == JTokenType.String)
        {
            string[] parts = jsonMessage[key].ToString().Split(',');
            if (parts.Length == 7)
            {
                Vector3 position = new Vector3(
                    float.Parse(parts[0]),
                    float.Parse(parts[1]),
                    float.Parse(parts[2])
                );

                float[] rotationArray = new float[4];
                rotationArray[0] = float.Parse(parts[3]);
                rotationArray[1] = float.Parse(parts[4]);
                rotationArray[2] = float.Parse(parts[5]);
                rotationArray[3] = float.Parse(parts[6]); // W component in degrees

                gameObject.transform.position = AdjustPositionAxis(position);
                gameObject.transform.rotation = AdjustRotationAxisOmni(rotationArray);
            }
        }
    }

    private Vector3 AdjustPositionAxis(Vector3 position)
    {
        return new Vector3(position.x, position.z, position.y);
    }

    private Quaternion AdjustRotationAxis(Quaternion rotation)
    {
        var originalRotQuat = new System.Numerics.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
        var rotationXQuat = System.Numerics.Quaternion.CreateFromAxisAngle(new System.Numerics.Vector3(1, 0, 0), (float)-Math.PI / 2);
        var rotationYQuat = System.Numerics.Quaternion.CreateFromAxisAngle(new System.Numerics.Vector3(0, 1, 0), (float)Math.PI);
        var worldRotation = System.Numerics.Quaternion.Multiply(rotationYQuat, rotationXQuat);
        worldRotation = System.Numerics.Quaternion.Multiply(originalRotQuat, worldRotation);
        worldRotation = System.Numerics.Quaternion.Multiply(rotationXQuat, worldRotation);
        return new Quaternion(-worldRotation.X, -worldRotation.Y, worldRotation.Z, worldRotation.W);
    }

    private Quaternion AdjustRotationAxisOmni(float[] rotationArray)
    {
        // Convert degrees to radians for the W component
        float wInRadians = rotationArray[3] * Mathf.Deg2Rad;

        // Create the original quaternion using System.Numerics.Quaternion
        var originalRotQuat = new System.Numerics.Quaternion(rotationArray[0], rotationArray[1], rotationArray[2], wInRadians);

        // Create the rotation quaternions for X and Y axes
        var rotationXQuat = System.Numerics.Quaternion.CreateFromAxisAngle(new System.Numerics.Vector3(1, 0, 0), (float)-Math.PI / 2);
        var rotationYQuat = System.Numerics.Quaternion.CreateFromAxisAngle(new System.Numerics.Vector3(0, 1, 0), (float)Math.PI);

        // Multiply the quaternions to get the world rotation
        var worldRotation = System.Numerics.Quaternion.Multiply(rotationYQuat, rotationXQuat);
        worldRotation = System.Numerics.Quaternion.Multiply(originalRotQuat, worldRotation);
        worldRotation = System.Numerics.Quaternion.Multiply(rotationXQuat, worldRotation);

        // Convert the resulting quaternion back to Unity's Quaternion
        return new Quaternion(-worldRotation.X, -worldRotation.Y, worldRotation.Z, worldRotation.W);
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
