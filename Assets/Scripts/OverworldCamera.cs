using UnityEngine;

public class OverworldCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Assign your player here

    [Header("Offset")]
    public Vector3 offset = new Vector3(0, 5, -10); // Default offset from player

    [Header("Smoothness")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f; // Lower = smoother

    void LateUpdate()
    {
        if (target == null) return;

        // Desired camera position
        Vector3 desiredPosition = target.position + offset;

        // Smoothly interpolate between current position and desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Apply position
        transform.position = smoothedPosition;
    }
}
