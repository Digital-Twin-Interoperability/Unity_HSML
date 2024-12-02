using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateSolarPanel : MonoBehaviour
{
    public float rotationAngle = 90f;  // The angle to rotate
    public float rotationDuration = 5f;  // The duration over which to rotate
    public float startDelay = 5f;

    private float elapsedTime = 0f;  // To track the elapsed time
    private bool rotationStarted = false;  // To track if the rotation has started
    private Quaternion startRotation;  // The initial rotation of the object
    private Quaternion endRotation;  // The target rotation of the object

    void Start()
    {
        startRotation = transform.localRotation;
        endRotation = Quaternion.Euler(transform.localEulerAngles + new Vector3(0, 0, rotationAngle));
        StartCoroutine(RotateAfterDelay());
    }

    private IEnumerator RotateAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        rotationStarted = true;
    }

    void Update()
    {
        if (rotationStarted)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / rotationDuration);
            transform.localRotation = Quaternion.Slerp(startRotation, endRotation, t);

            // Stop updating after the rotation is complete
            if (t >= 1f)
            {
                rotationStarted = false;
            }
        }
    }
}
