using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class landingLander : MonoBehaviour
{
    public float rotationAngle = 130f;  // The angle to rotate
    public float rotationDuration = 15f;  // The duration over which to rotate
    public float startDelay = 4f;
    public float switchDelay = 20f;// The delay before starting the rotation

    public GameObject roverDrive;  // The rover drive game object
    public GameObject roverStatic;  // The rover static game object

    private float elapsedTime = 0f;  // To track the elapsed time
    private bool rotationStarted = false;  // To track if the rotation has started
    private Quaternion startRotation;  // The initial rotation of the object
    private Quaternion endRotation;  // The target rotation of the object

    void Start()
    {
        startRotation = transform.rotation;
        endRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(rotationAngle, 0, 0));
        StartCoroutine(RotateAfterDelay());
    }

    private IEnumerator RotateAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        rotationStarted = true;
        yield return new WaitForSeconds(switchDelay);
        roverDrive.SetActive(true);
        roverStatic.SetActive(false);
    }

    void Update()
    {
        if (rotationStarted)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / rotationDuration);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            // Stop updating after the rotation is complete
            if (t >= 1f)
            {
                rotationStarted = false;
            }
        }
    }
}