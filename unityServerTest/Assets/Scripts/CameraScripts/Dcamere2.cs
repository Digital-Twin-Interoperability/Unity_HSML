using UnityEngine;

public class Dcamere2 : MonoBehaviour
{
    public Transform target; // The target object to follow
    public float radius = 2.0f; // Initial radius distance from the target
    public float height = 0.0f; // Initial height of the camera from the target
    public float[] angles; // Array of angles for camera positions

    private int currentAngleIndex = 0;

    void Start()
    {
        if (angles.Length == 0)
        {
            angles = new float[] { Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f) }; // Randomized angles between 0 and 360 degrees
        }

        // Initial randomization of radius and height
        radius = Random.Range(2.0f, 3.0f);
        height = Random.Range(0.0f, 1.0f);

        // Initial camera position
        SnapToNewPosition();
    }

    void Update()
    {
        if (target != null)
        {
            // Check if the "4" key is pressed
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                // Update the current angle index
                currentAngleIndex = (currentAngleIndex + 1) % angles.Length;

                // Randomize the angles again for the next cycle
                if (currentAngleIndex == 0)
                {
                    for (int i = 0; i < angles.Length; i++)
                    {
                        angles[i] = Random.Range(0.0f, 360.0f);
                    }
                }

                // Randomize the radius and height
                radius = Random.Range(2.0f, 3.0f);
                height = Random.Range(0.0f, 1.0f);

                // Snap to the new position
                SnapToNewPosition();
            }
        }
    }

    void SnapToNewPosition()
    {
        // Calculate the offset position around the target based on the random angle
        float angle = angles[currentAngleIndex];
        Vector3 offset = new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * angle) * radius,
            height,
            Mathf.Sin(Mathf.Deg2Rad * angle) * radius
        );

        // Set the camera's position relative to the target
        transform.position = target.position + offset;

        // Make the camera look at the target
        transform.LookAt(target);

        // Ensure the camera angle is always straight (adjust pitch and roll to 0)
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }
}