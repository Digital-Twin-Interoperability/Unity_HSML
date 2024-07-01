using System.Collections;
using UnityEngine;

public class viperPlanning : MonoBehaviour
{
    public float m_Speed = 1f;                 // How fast the tank moves forward and back.
    public float m_TurnSpeed = 100f;            // How fast the tank turns in degrees per second.

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
            // Move forward for 5 seconds
            yield return MoveForward(10f);

            // Turn right (90 degrees)
            yield return Turn(90f);

            // Move forward for another 5 seconds
            yield return MoveForward(10f);

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
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
