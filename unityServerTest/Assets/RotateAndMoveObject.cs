using UnityEngine;

public class RotateAndMoveObject : MonoBehaviour
{
    public float rotationSpeed = 60f; // Degrees per second
    public KeyCode activationKey = KeyCode.B;

    private Quaternion originalRotation;
    private bool isActivated = false;

    void Start()
    {
        // Store the original position and rotation of the object
        originalRotation = transform.rotation;
    }

    void Update()
    {
        // Check if the activation key (B) is being held down
        if (Input.GetKey(activationKey))
        {
            isActivated = true;

            // Rotate the object around its Y-axis
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);


        }
        else if (isActivated)
        {
            // When the key is released, reset the object to its original position and rotation
            transform.rotation = originalRotation;
            isActivated = false;
        }
    }
}
