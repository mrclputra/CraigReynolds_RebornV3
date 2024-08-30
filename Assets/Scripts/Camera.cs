using UnityEngine;

public class Camera : MonoBehaviour
{
    private Vector3 pivot;
    [SerializeField] private float sensitivity;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;

    [SerializeField] private float currentZoom;
    private float targetZoom;
    private float rX = 0f;
    private float rY = 0f;
    private float targetRX;
    private float targetRY;

    [SerializeField] private float rotationSmoothTime;
    [SerializeField] private float zoomSmoothTime;
    private float rotationSmoothVelocityX;
    private float rotationSmoothVelocityY;
    private float zoomSmoothVelocity;

    private void Awake()
    {
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        targetZoom = currentZoom;
        pivot = Vector3.zero; // set pivot to center

        // move camera to init position
        transform.position = pivot + Quaternion.Euler(rX, rY, 0f) * new Vector3(0f, 0f, -currentZoom);
        transform.LookAt(pivot);
    }

    private void Update()
    {
        if(Input.GetMouseButton(1))
        {
            targetRX += Input.GetAxis("Mouse X") * sensitivity * 0.01f;
            targetRY -= Input.GetAxis("Mouse Y") * sensitivity * 0.01f;
            targetRY = Mathf.Clamp(targetRY, -88f, 88f); // set up and down limits
        }

        // zoom function
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // smoothen rotation
        rX = Mathf.SmoothDampAngle(rX, targetRX, ref rotationSmoothVelocityX, rotationSmoothTime);
        rY = Mathf.SmoothDampAngle(rY, targetRY, ref rotationSmoothVelocityY, rotationSmoothTime);

        transform.position = pivot + Quaternion.Euler(rY, rX, 0f) * new Vector3(0f, 0f, -currentZoom);
        transform.LookAt(pivot);

        // smooth zoom
        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomSmoothVelocity, zoomSmoothTime);
    }
}
