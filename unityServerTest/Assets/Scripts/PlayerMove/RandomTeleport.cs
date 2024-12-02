using System.Collections;
using UnityEngine;

public class RandomTeleport : MonoBehaviour
{
    public float teleportRadius = 5f; // The radius within which the object will teleport
    public float teleportInterval = 15f; // Time in seconds between teleports

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position; // Store the original position of the object
        StartCoroutine(TeleportRandomly());
    }

    IEnumerator TeleportRandomly()
    {
        while (true) // Infinite loop to keep teleporting every 15 seconds
        {
            yield return new WaitForSeconds(teleportInterval);

            // Generate a random position within the teleportRadius
            Vector3 randomOffset = new Vector3(
                Random.Range(-teleportRadius, teleportRadius),
                Random.Range(-teleportRadius, teleportRadius), // Optionally adjust for only horizontal teleportation by setting this to 0
                Random.Range(-teleportRadius, teleportRadius)
            );

            // Set the object's new position to the original position plus the random offset
            transform.position = originalPosition + randomOffset;
        }
    }
}
