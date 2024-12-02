using UnityEngine;

namespace Complete
{
    public class viperController : MonoBehaviour
    {
        public float m_Speed = 12f;                 // How fast the tank moves forward and back.
        public float m_TurnSpeed = 180f;            // How fast the tank turns in degrees per second.
        public float m_WheelRotationSpeed = 360f;   // Speed at which the wheels rotate.

        public GameObject frontLeftWheel;           // Front left wheel
        public GameObject frontRightWheel;          // Front right wheel
        public GameObject backLeftWheel;            // Back left wheel
        public GameObject backRightWheel;           // Back right wheel

        private string m_MovementAxisName;          // The name of the input axis for moving forward and back.
        private string m_TurnAxisName;              // The name of the input axis for turning.
        private Rigidbody m_Rigidbody;              // Reference used to move the tank.
        private float m_MovementInputValue;         // The current value of the movement input.
        private float m_TurnInputValue;             // The current value of the turn input.

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            // When the tank is turned on, make sure it's not kinematic.
            m_Rigidbody.isKinematic = false;

            // Also reset the input values.
            m_MovementInputValue = 0f;
            m_TurnInputValue = 0f;
        }

        private void OnDisable()
        {
            // When the tank is turned off, set it to kinematic so it stops moving.
            m_Rigidbody.isKinematic = true;
        }

        private void Start()
        {
            // The axes names are based on player number.
            m_MovementAxisName = "Vertical";
            m_TurnAxisName = "Horizontal";
        }

        private void Update()
        {
            // Store the value of both input axes.
            m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
            m_TurnInputValue = Input.GetAxis(m_TurnAxisName);
        }

        private void FixedUpdate()
        {
            // Adjust the rigidbodies position and orientation in FixedUpdate.
            Move();
            Turn();
            RotateWheels();
        }

        private void Move()
        {
            // Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
            Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;

            // Apply this movement to the rigidbody's position.
            m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
        }

        private void Turn()
        {
            // Determine the number of degrees to be turned based on the input, speed and time between frames.
            float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

            // Make this into a rotation in the y axis.
            Quaternion turnRotation = Quaternion.Euler(-turn, 0f, 0f);

            // Apply this rotation to the rigidbody's rotation.
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
        }

        private void RotateWheels()
        {
            float wheelRotation = -m_MovementInputValue * m_WheelRotationSpeed * Time.deltaTime;  // Calculate wheel rotation angle
            float turnRotation = m_TurnInputValue * m_WheelRotationSpeed * Time.deltaTime;

            // Rotate wheels for forward/backward movement
            frontLeftWheel.transform.Rotate(Vector3.up * wheelRotation);
            frontRightWheel.transform.Rotate(Vector3.up * wheelRotation);
            backLeftWheel.transform.Rotate(Vector3.up * wheelRotation);
            backRightWheel.transform.Rotate(Vector3.up * wheelRotation);

            // Rotate wheels for turning
            if (m_TurnInputValue != 0)
            {
                frontLeftWheel.transform.Rotate(Vector3.up * -turnRotation);
                backLeftWheel.transform.Rotate(Vector3.up * -turnRotation);
                frontRightWheel.transform.Rotate(Vector3.up * turnRotation);
                backRightWheel.transform.Rotate(Vector3.up * turnRotation);
            }
        }
    }
}
