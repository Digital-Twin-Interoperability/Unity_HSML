using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SocketClient3 : MonoBehaviour
{
    private const int port = 1234;
    private const string serverIP = "10.97.145.30";

    private Socket clientSocket;
    private byte[] receiveBuffer = new byte[2048]; // Increased buffer size to handle larger data

    public GameObject targetObject1;
    private Vector3 newPosition1;
    private Quaternion newRotation1;

    void Start()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ConnectToServer();
    }

    void Update()
    {
        if (targetObject1 != null)
        {
            Vector3 adjustedPosition = AdjustPositionAxis(newPosition1);
            Quaternion adjustedRotation = AdjustRotationAxis(newRotation1);

            targetObject1.transform.position = targetObject1.transform.parent.TransformPoint(adjustedPosition);
            targetObject1.transform.rotation = targetObject1.transform.parent.rotation * adjustedRotation;
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

                // Remove {, }, (, ), [, ] from the message and split by spaces
                message = message.Replace("{", "").Replace("}", "").Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").Replace("\'", "").Replace(",", "");
                string[] parts = message.Split(' ');

                // Debug log the parsed parts
                Debug.Log(message);

                // Parse XYZ data for one object
                if (parts.Length == 7)
                {
                    // Parse first object's position and rotation
                    float x1, y1, z1, rx1, ry1, rz1, w1;
                    if (float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x1) &&
                        float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out y1) &&
                        float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out z1) &&
                        float.TryParse(parts[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out rx1) &&
                        float.TryParse(parts[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out ry1) &&
                        float.TryParse(parts[5], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out rz1) &&
                        float.TryParse(parts[6], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out w1))
                    {
                        // Convert from cm to meters
                        //x1 /= 100.0f;
                        //y1 /= 100.0f;
                        //z1 /= 100.0f;

                        // Update the new position and rotation for the first object
                        newPosition1 = new Vector3(x1, y1, z1);

                        // newRotation1 = new Quaternion(rx1, ry1, rz1, w1);
                        Vector3 axis1 = new Vector3(rx1, ry1, rz1).normalized;
                        newRotation1 = Quaternion.AngleAxis(w1, axis1);
                    }

                    else
                    {
                        Debug.LogError("Failed to parse first object's XYZW and quaternion data as float values!");
                    }
                }
                else
                {
                    Debug.LogError("Received message does not contain valid data for the object!");
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
        return new Quaternion(worldRotation.X, -worldRotation.Y, -worldRotation.Z, worldRotation.W);
    }
}
