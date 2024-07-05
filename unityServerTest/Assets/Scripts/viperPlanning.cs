using System.Collections;
using UnityEngine;

public class viperPlanning : MonoBehaviour
{
    public float m_Speed = 1f;                 // How fast the tank moves forward and back.
    public float m_TurnSpeed = 100f;            // How fast the tank turns in degrees per second.
    public GameObject[] leftWheels;             // Array for the left wheels
    public GameObject[] rightWheels;            // Array for the right wheels
    public float wheelTurnSpeed = 500f;         // Speed of the wheel rotation
    public float wheelTurnSpeed2 = 10f;

    private Rigidbody m_Rigidbody;              // Reference used to move the tank.

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        // When the tank is turned on, make sure it's not kinematic.
        m_Rigidbody.isKinematic = false;

        // Start the movement coroutine
        StartCoroutine(FollowPath());
    }

    private void OnDisable()
    {
        // When the tank is turned off, set it to kinematic so it stops moving.
        m_Rigidbody.isKinematic = true;

        // Stop the movement coroutine
        StopCoroutine(FollowPath());
    }

    private void FixedUpdate()
    {
        // Adjust the rigidbody's position and orientation in FixedUpdate.
        // The movement will be handled by the coroutine
    }

    private IEnumerator FollowPath()
    {
        while (true)
        {
            // Move forward for 15 seconds
            yield return MoveForward(15f);

            // Turn right (90 degrees)
            yield return Turn(-90f);

            // Move forward for another 100 seconds
            yield return MoveForward(100f);

            // Add more steps as needed
        }
    }

    private IEnumerator MoveForward(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            Vector3 movement = transform.forward * m_Speed * Time.deltaTime;
            m_Rigidbody.MovePosition(m_Rigidbody.position + movement);

            RotateWheelsForward(m_Speed);

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator Turn(float angle)
    {
        float targetAngle = m_Rigidbody.rotation.eulerAngles.y + angle;
        float timer = 0f;

        while (Mathf.Abs(m_Rigidbody.rotation.eulerAngles.y - targetAngle) > 0.1f)
        {
            float turn = m_TurnSpeed * Time.deltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, -turn, 0f);
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);

            RotateWheelsForTurn(m_TurnSpeed);

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void RotateWheelsForward(float speed)
    {
        float rotation = speed * wheelTurnSpeed * Time.deltaTime;

        foreach (var wheel in leftWheels)
        {
            wheel.transform.Rotate(rotation, 0f, 0f);
        }

        foreach (var wheel in rightWheels)
        {
            wheel.transform.Rotate(rotation, 0f, 0f);
        }
    }

    private void RotateWheelsForTurn(float turnSpeed)
    {
        float rotation = turnSpeed * wheelTurnSpeed2 * Time.deltaTime;

        foreach (var wheel in leftWheels)
        {
            wheel.transform.Rotate(-rotation, 0f, 0f);
        }

        foreach (var wheel in rightWheels)
        {
            wheel.transform.Rotate(rotation, 0f, 0f);
        }
    }
}
