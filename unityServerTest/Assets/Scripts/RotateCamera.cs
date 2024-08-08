using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public Transform target; // The target object to rotate around
    public float radius = 10.0f; // The radius distance from the target
    public float rotatingSpeed = 10.0f; // The speed of horizontal rotation
    public float height = 5.0f; // The height of the camera from the target
    public float verticalAngle = 0.0f; // The vertical angle in degrees

    private float horizontalAngle = 0.0f;

    void Update()
    {
        if (target != null)
        {
            // Increment the horizontal angle based on the rotating speed and time
            horizontalAngle += rotatingSpeed * Time.deltaTime;

            // Convert vertical angle to radians for calculation
            float verticalAngleRad = Mathf.Deg2Rad * verticalAngle;

            // Calculate the new position of the camera
            float x = target.position.x + radius * Mathf.Cos(horizontalAngle) * Mathf.Cos(verticalAngleRad);
            float z = target.position.z + radius * Mathf.Sin(horizontalAngle) * Mathf.Cos(verticalAngleRad);
            float y = target.position.y + height + radius * Mathf.Sin(verticalAngleRad);

            // Set the camera's position
            transform.position = new Vector3(x, y, z);

            // Make the camera look at the target
            transform.LookAt(target);
        }
    }
}
