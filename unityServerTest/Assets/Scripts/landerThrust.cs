using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class landerThrust : MonoBehaviour
{
    public float thrustForce = 0.5f; // Adjust this value to control the upward force
    private Rigidbody rb;
    public GameObject thrustFire;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(thrustTurnOff());
        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found! Please attach a Rigidbody component to the lander.");
        }
        
    }
    private IEnumerator thrustTurnOff()
    {
        yield return new WaitForSeconds(3);
        thrustFire.SetActive(false);
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            // Apply an upward force
            rb.AddForce(Vector3.up * thrustForce, ForceMode.Acceleration);
        }
    }
}