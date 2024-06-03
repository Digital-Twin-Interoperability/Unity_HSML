using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SocketClient2 : MonoBehaviour
{
    private const int port = 1234;
    private const string serverIP = "10.97.144.82"; // Replace with your server IP

    private Socket clientSocket;
    private byte[] receiveBuffer = new byte[2048]; // Increased buffer size to handle larger data

    public GameObject targetObject1;
    public GameObject targetObject2;

    private Vector3 newPosition1;
    private Quaternion newRotation1;
    private Vector3 newPosition2;
    private Quaternion newRotation2;

    void Start()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ConnectToServer();
    }

    void Update()
    {
        if (targetObject1 != null)
        {
            targetObject1.transform.position = newPosition1;
            targetObject1.transform.rotation = newRotation1;
        }
        if (targetObject2 != null)
        {
            targetObject2.transform.position = newPosition2;
            targetObject2.transform.rotation = newRotation2;
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

                // Remove { and } from the message
                message = message.Trim('(', ')');

                string[] parts = message.Split(',');

                // Debug log the parsed parts
                Debug.Log(message);
                Debug.Log("Parsed parts: " + string.Join(", ", parts));

                // Parse XYZ data for two objects
                if (parts.Length == 14)
                {
                    // Parse first object's position and rotation
                    float x1, y1, z1, w1, rx1, ry1, rz1;
                    if (float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x1) &&
                        float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out y1) &&
                        float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out z1) &&
                        float.TryParse(parts[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out w1) &&
                        float.TryParse(parts[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out rx1) &&
                        float.TryParse(parts[5], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out ry1) &&
                        float.TryParse(parts[6], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out rz1))
                    {
                        // Convert from cm to meters
                        x1 /= 100.0f;
                        y1 /= 100.0f;
                        z1 /= 100.0f;

                        // Update the new position and rotation for the first object
                        newPosition1 = new Vector3(-x1, y1, z1);
                        newRotation1 = new Quaternion(rx1, ry1, rz1, w1);
                    }
                    else
                    {
                        Debug.LogError("Failed to parse first object's XYZW and quaternion data as float values!");
                    }

                    // Parse second object's position and rotation
                    float x2, y2, z2, w2, rx2, ry2, rz2;
                    if (float.TryParse(parts[7], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x2) &&
                        float.TryParse(parts[8], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out y2) &&
                        float.TryParse(parts[9], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out z2) &&
                        float.TryParse(parts[10], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out w2) &&
                        float.TryParse(parts[11], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out rx2) &&
                        float.TryParse(parts[12], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out ry2) &&
                        float.TryParse(parts[13], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out rz2))
                    {
                        // Convert from cm to meters
                        x2 /= 100.0f;
                        y2 /= 100.0f;
                        z2 /= 100.0f;

                        // Update the new position and rotation for the second object
                        newPosition2 = new Vector3(-x2, y2, z2);
                        newRotation2 = new Quaternion(rx2, ry2, rz2, w2);
                    }
                    else
                    {
                        Debug.LogError("Failed to parse second object's XYZW and quaternion data as float values!");
                    }
                }
                else
                {
                    Debug.LogError("Received message does not contain valid data for two objects!");
                }
            }
            clientSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ReceiveCallback, null);
        }
        catch (Exception e)
        {
            Debug.Log("Receive callback exception: " + e.ToString());
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
