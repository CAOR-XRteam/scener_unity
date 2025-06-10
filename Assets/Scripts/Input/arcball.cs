using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace scener.input {
public class ArcballCameraNew : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private float distance = 5f;

    [SerializeField]
    private float zoomSpeed = 30f;

    [SerializeField]
    private float minDistance = 1f;

    [SerializeField]
    private float maxDistance = 20f;

    [SerializeField]
    private float xSpeed = 80f;

    [SerializeField]
    private float ySpeed = 80f;

    [SerializeField]
    private float yMinLimit = -20f;

    [SerializeField]
    private float yMaxLimit = 80f;

    private float x = 0.0f;
    private float y = 0.0f;

    private InputAction lookAction;
    private InputAction orbitHoldAction;
    private InputAction zoomAction;

    public InputActionAsset inputActions;

    void OnEnable()
    {
        if (inputActions == null)
        {
            Debug.LogError("InputActions asset not assigned.");
            return;
        }

        var map = inputActions.FindActionMap("Camera");
        if (map == null)
        {
            Debug.LogError("Cannot find ActionMap 'Camera'");
            return;
        }

        lookAction = map.FindAction("Look");
        orbitHoldAction = map.FindAction("OrbitHold");
        zoomAction = map.FindAction("Zoom");

        if (lookAction == null)
            Debug.LogError("Look action missing!");
        if (orbitHoldAction == null)
            Debug.LogError("OrbitHold action missing!");
        if (zoomAction == null)
            Debug.LogError("Zoom action missing!");

        map.Enable();
    }

    void OnDisable()
    {
        var map = inputActions?.FindActionMap("Camera");
        map?.Disable();
    }

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        if (target == null)
        {
            GameObject go = new GameObject("Camera Target");
            go.transform.position = transform.position + transform.forward * distance;
            target = go.transform;
        }
    }

    void LateUpdate()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // If the pointer is over UI, do not process camera input
            //return;
        }

        if (lookAction == null || orbitHoldAction == null || zoomAction == null)
        {
            Debug.LogError("Input actions not assigned!");
            return;
        }

        bool orbitHeld = orbitHoldAction.ReadValue<float>() > 0.5f;
        float zoomDelta = zoomAction.ReadValue<float>();

        if (orbitHeld)
        {
            if (Mathf.Abs(zoomDelta) > 0.01f)
            {
                distance -= zoomDelta * zoomSpeed * Time.deltaTime;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
            else
            {
                Vector2 lookDelta = lookAction.ReadValue<Vector2>();
                x += lookDelta.x * xSpeed * 0.01f;
                y -= lookDelta.y * ySpeed * 0.01f;
                y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
            }
        }

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.rotation = rotation;
        transform.position = target.position + offset;
    }
}
}
