using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SocketClient : MonoBehaviour
{
    private const int port = 1234;
    private const string serverIP = "192.168.137.1"; // Replace with your server IP

    private Socket clientSocket;
    private byte[] receiveBuffer = new byte[1024];

    public GameObject targetObject;
    private Vector3 newPosition;
    private Quaternion newRotation;

    void Start()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ConnectToServer();
    }

    void Update()
    {
        if (targetObject != null)
        {
            targetObject.transform.position = newPosition;
            targetObject.transform.rotation = newRotation;
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
                //message = message.Replace("{", "").Replace("}", "");
                message = message.Trim('(', ')');

                string[] parts = message.Split(',');

                // Debug log the parsed parts
                Debug.Log(message);
                Debug.Log("Parsed parts: " + string.Join(", ", parts));

                // Parse XYZ data
                if (parts.Length == 7)
                {
                    float x, y, z, rx, ry, rz, w;
                    if (float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x) &&
                        float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out y) &&
                        float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out z) &&
                        float.TryParse(parts[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out w) &&
                        float.TryParse(parts[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out rx) &&
                        float.TryParse(parts[5], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out ry) &&
                        float.TryParse(parts[6], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out rz))
                    {
                        // Convert from cm to meters
                        x /= 100.0f;
                        y /= 100.0f;
                        z /= 100.0f;

                        // Update the new position and rotation
                        newPosition = new Vector3(x, -y, z);
                        newRotation = new Quaternion(rz, ry, rx, w);
                    }
                    else
                    {
                        Debug.LogError("Failed to parse XYZW and quaternion data as float values!");
                    }
                }
                else
                {
                    Debug.LogError("Received message does not contain valid XYZW and quaternion data!");
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
