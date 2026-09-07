using UnityEngine;

public sealed class Manipulation : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotateSensitivity = 0.2f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 10f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private GameInput _controls;

    private void Awake() => _controls = new GameInput();

    private void OnEnable() => _controls.ControllOf3dObj.Enable();

    private void OnDisable() => _controls.ControllOf3dObj.Disable();

    private void Update()
    {
        ProcessRotation();
        ProcessZoom();
    }

    private void ProcessRotation()
    {
        if (!_controls.ControllOf3dObj.TwistHold.IsPressed()) return;

        Vector2 delta = _controls.ControllOf3dObj.TwistDelta.ReadValue<Vector2>();
        if (delta == Vector2.zero) return;

     
        transform.Rotate(Vector3.right, delta.y * rotateSensitivity, Space.Self);
        transform.Rotate(Vector3.up, -delta.x * rotateSensitivity, Space.Self);
    }

    private void ProcessZoom()
    {
        Vector2 scrollVector = _controls.ControllOf3dObj.Scroll.ReadValue<Vector2>();

        if (Mathf.Abs(scrollVector.y) < 0.01f) return;

        float scrollDirection = Mathf.Sign(scrollVector.y);

        Vector3 directionToCamera = (cameraTransform.position - transform.position).normalized;

        Vector3 newPosition = transform.position + (directionToCamera * (scrollDirection * zoomSpeed * Time.deltaTime));

        float currentDistance = Vector3.Distance(newPosition, cameraTransform.position);
        if (currentDistance >= minDistance && currentDistance <= maxDistance)
        {
            transform.position = newPosition;
        }
    }
}