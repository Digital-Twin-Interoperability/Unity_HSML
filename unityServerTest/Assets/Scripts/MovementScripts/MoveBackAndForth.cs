using UnityEngine;

public class MoveBackAndForth : MonoBehaviour
{
    public float moveDistance = 0.03f;  // Distance to move in each direction
    public float moveDuration = 1.0f;  // Time to complete each movement (forward or backward)

    private Vector3 startPos;          // Starting position
    private float elapsedTime = 0.0f;  // Time counter
    private bool movingForward = true; // Determines the movement direction

    void Start()
    {
        // Record the starting position of the object
        startPos = transform.position;
    }

    void Update()
    {
        // Increment the time counter
        elapsedTime += Time.deltaTime;

        // Calculate how far along the movement cycle we are (0 to 1)
        float progress = elapsedTime / moveDuration;

        // Move forward
        if (movingForward)
        {
            // Lerp towards the target position (0.1 meters forward)
            transform.position = Vector3.Lerp(startPos, startPos + new Vector3(0, 0, moveDistance), progress);
        }
        // Move backward
        else
        {
            // Lerp towards the starting position (0.1 meters backward)
            transform.position = Vector3.Lerp(startPos + new Vector3(0, 0, moveDistance), startPos, progress);
        }

        // If we’ve completed the movement in one direction (forward or back), switch direction
        if (progress >= 1.0f)
        {
            movingForward = !movingForward;
            elapsedTime = 0.0f; // Reset the time counter for the next movement cycle
        }
    }
}
