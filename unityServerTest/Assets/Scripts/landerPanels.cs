using UnityEngine;
using System.Collections;

public class landerPanels : MonoBehaviour
{
    public GameObject panel1, panel2, roverStatic, roverDynamic;  


    private float rotationAngle = 107;    // The angle to rotate
    private float rotationAngle2 = 180f;
    private float duration = 1f;          // Duration of the rotation in seconds
    private float delay = 2f;             // Delay before starting the rotation in seconds

    private void Start()
    {
        // Start the rotation coroutine after the specified delay
        StartCoroutine(RotatePanelsAfterDelay());
    }

    private IEnumerator RotatePanelsAfterDelay()
    {
        // Wait for the delay
        yield return new WaitForSeconds(delay);

        // Get the start time
        float startTime = Time.time;

        // Calculate the end time
        float endTime = startTime + duration;

        // Store the initial rotations
        Quaternion startRotation1 = panel1.transform.localRotation;
        Quaternion startRotation2 = panel2.transform.localRotation;

        // Calculate the target rotations
        Quaternion endRotation1 = startRotation1 * Quaternion.Euler(rotationAngle, 0f, 0f);
        Quaternion endRotation2 = startRotation2 * Quaternion.Euler(rotationAngle2, 0f, 0f);

        // Rotate the panels over the duration
        while (Time.time < endTime)
        {
            // Calculate the interpolation factor
            float t = (Time.time - startTime) / duration;

            // Interpolate the rotations
            panel1.transform.localRotation = Quaternion.Lerp(startRotation1, endRotation1, t);
            panel2.transform.localRotation = Quaternion.Lerp(startRotation2, endRotation2, t);

            // Wait for the next frame
            yield return null;
        }

        // Ensure the panels reach the final rotation
        panel1.transform.localRotation = endRotation1;
        panel2.transform.localRotation = endRotation2;

        roverStatic.SetActive(false);
        roverDynamic.SetActive(true);
    }
}
