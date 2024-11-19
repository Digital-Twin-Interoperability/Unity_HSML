using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public Transform target; // The target object to rotate around
    public float radius = 10.0f; // The radius distance from the target
    public float height = 5.0f; // The height of the camera from the target
    public float verticalAngle = 0.0f; // The vertical angle in degrees
    public float angleChangeInterval = 10.0f; // Time interval to change the angle
    public float angleStep = 60.0f; // The angle to increment every interval

    private float horizontalAngle = 0.0f; // The current horizontal angle
    private float timeSinceLastChange = 0.0f; // Time tracker for angle changes

    void Update()
    {
        if (target != null)
        {
            // Update the time since the last angle change
            timeSinceLastChange += Time.deltaTime;

            // Check if it's time to rotate the camera by the angle step
            if (timeSinceLastChange >= angleChangeInterval)
            {
                // Reset the timer
                timeSinceLastChange = 0.0f;

                // Increment the horizontal angle by 60 degrees
                horizontalAngle += angleStep;

                // Keep the angle within 0-360 degrees for continuity
                if (horizontalAngle >= 360.0f)
                {
                    horizontalAngle -= 360.0f;
                }
            }

            // Convert vertical angle to radians for calculation
            float verticalAngleRad = Mathf.Deg2Rad * verticalAngle;

            // Calculate the new position of the camera
            float x = target.position.x + radius * Mathf.Cos(horizontalAngle * Mathf.Deg2Rad) * Mathf.Cos(verticalAngleRad);
            float z = target.position.z + radius * Mathf.Sin(horizontalAngle * Mathf.Deg2Rad) * Mathf.Cos(verticalAngleRad);
            float y = target.position.y + height + radius * Mathf.Sin(verticalAngleRad);

            // Set the camera's position
            transform.position = new Vector3(x, y, z);

            // Make the camera look at the target
            transform.LookAt(target);
        }
    }
}
