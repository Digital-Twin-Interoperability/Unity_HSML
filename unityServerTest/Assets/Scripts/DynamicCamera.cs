using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    public Transform target; // The target object to follow
    public float radius = 5.0f; // Initial radius distance from the target
    public float height = 3.0f; // Initial height of the camera from the target
    public float angleChangeInterval = 15.0f; // Time interval to change camera position
    public float[] angles; // Array of angles for camera positions

    private int currentAngleIndex = 0;
    private float timeSinceLastChange = 0.0f;

    void Start()
    {
        if (angles.Length == 0)
        {
            angles = new float[] { Random.Range(-45.0f, 45.0f), Random.Range(-45.0f, 45.0f), Random.Range(-45.0f, 45.0f), Random.Range(-45.0f, 45.0f) }; // Randomized angles between -45 and 45 degrees
        }
    }

    void Update()
    {
        if (target != null)
        {
            // Update the time since the last change
            timeSinceLastChange += Time.deltaTime;

            // Check if it's time to change the camera position
            if (timeSinceLastChange >= angleChangeInterval)
            {
                // Reset the timer
                timeSinceLastChange = 0.0f;

                // Update the current angle index
                currentAngleIndex = (currentAngleIndex + 1) % angles.Length;

                // Randomize the angles again for the next cycle
                if (currentAngleIndex == 0)
                {
                    for (int i = 0; i < angles.Length; i++)
                    {
                        angles[i] = Random.Range(-45.0f, 45.0f);
                    }
                }

                // Randomize the radius and height
                radius = Random.Range(3.0f, 7.0f);
                height = Random.Range(2.0f, 4.0f);
            }

            // Calculate the offset position behind the target based on its forward direction
            Vector3 offset = -target.forward * radius;
            offset.y = height;

            // Calculate the new position of the camera
            float angle = angles[currentAngleIndex];
            offset = Quaternion.AngleAxis(angle, Vector3.up) * offset;

            // Set the camera's position relative to the target
            transform.position = target.position + offset;

            // Make the camera look at the target
            transform.LookAt(target);
        }
    }
}
