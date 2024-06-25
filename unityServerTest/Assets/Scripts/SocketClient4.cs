using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SocketClient4 : MonoBehaviour
{
    private const int port = 1234;
    private const string serverIP = "192.168.137.1";

    private Socket clientSocket;
    private byte[] receiveBuffer = new byte[2048]; // Increased buffer size to handle larger data

    public GameObject targetObject1;
    private Vector3 newPosition1;
    private Quaternion newRotation1;

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
            // Send the position and rotation of targetObject1
            if (targetObject1 != null)
            {
                Vector3 position = AdjustPositionAxis(targetObject1.transform.position);
                Quaternion rotation = AdjustRotationAxis(targetObject1.transform.rotation);

                // Multiply each value of XYZ by 100 before sending
                float posX = position.x * 1;
                float posY = position.y * 1;
                float posZ = position.z * 1;

                SendMessageToServer($"Rover1,{posX},{posY},{posZ},{rotation.x},{rotation.y},{rotation.z},{rotation.w}");
            }

            // Reset the timer
            timeSinceLastMessage = 0f;
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
                string message = Encoding.UTF8.GetString(data);

                // Remove {, }, (, ), [, ] from the message and split by spaces
                message = message.Replace("{", "").Replace("}", "").Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").Replace("\'", "").Replace(",", "");
                string[] parts = message.Split(' ');

                // Debug log the parsed parts
                Debug.Log(message);
            }
            clientSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ReceiveCallback, null);
        }
        catch (Exception e)
        {
            Debug.Log("Receive callback exception: " + e.ToString());
        }
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

    private void OnDestroy()
    {
        if (clientSocket != null && clientSocket.Connected)
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
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
}