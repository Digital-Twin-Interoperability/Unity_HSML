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
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);


    }
}
