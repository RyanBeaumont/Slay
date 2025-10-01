using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    private Camera cam;

    // Target state
    private Vector3 targetPos;
    private float targetZoom;

    [Header("Decay Settings")]
    [SerializeField] private float moveDecay = 5f; // bigger = faster approach
    [SerializeField] private float zoomDecay = 5f;

    void Awake()
    {
        cam = GetComponent<Camera>();
        targetPos = transform.position;
        targetZoom = cam.orthographic ? cam.orthographicSize : cam.fieldOfView;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Exponential decay towards target
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            1f - Mathf.Exp(-moveDecay * dt)
        );

        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                targetZoom,
                1f - Mathf.Exp(-zoomDecay * dt)
            );
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(
                cam.fieldOfView,
                targetZoom,
                1f - Mathf.Exp(-zoomDecay * dt)
            );
        }
    }

    /// <summary>
    /// Moves the camera toward a world position and zoom value.
    /// For orthographic cameras, zoom is orthographicSize.
    /// For perspective, zoom is fieldOfView.
    /// </summary>
    public void MoveCamera(Vector2 position, float zoom)
    {
        targetPos = new Vector3(position.x, position.y, transform.position.z);
        targetZoom = zoom;
    }
}