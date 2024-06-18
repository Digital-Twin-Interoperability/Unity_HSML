using UnityEngine;

public class PrintWorldTransform : MonoBehaviour
{
    // Assign the target GameObject in the Inspector
    public GameObject targetGameObject;

    void Update()
    {
        if (targetGameObject != null)
        {
            // Get the world position and rotation of the target GameObject
            Vector3 worldPosition = targetGameObject.transform.position;
            Quaternion worldRotation = targetGameObject.transform.rotation;

            // Format the output string
            string output = string.Format("Position and Rotation: {0},{1},{2},{3},{4},{5},{6}",
                                           worldPosition.x, worldPosition.y, worldPosition.z,
                                           worldRotation.x, worldRotation.y, worldRotation.z, worldRotation.w);

            // Print the formatted string
            Debug.Log(output);
        }
        else
        {
            Debug.LogWarning("Target GameObject is not assigned.");
        }
    }
}
