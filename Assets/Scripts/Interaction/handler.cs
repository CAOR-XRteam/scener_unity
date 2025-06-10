using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


namespace scener.input {
public class DragObject : MonoBehaviour {
    private Camera mainCamera;
    private Transform object_selected;
    private float zOffset;
    private Vector3 grabOffset;

    void Start() {
        mainCamera = Camera.main;
    }

    void Update() {
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                object_selected = hit.transform;
                zOffset = Vector3.Distance(mainCamera.transform.position, object_selected.position);
                Vector3 objectScreenPos = mainCamera.WorldToScreenPoint(object_selected.position);
                Vector3 mousePos = Mouse.current.position.ReadValue();
                grabOffset = object_selected.position - mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, zOffset));
            }
        }

        if (Mouse.current.leftButton.isPressed && object_selected != null) {
            Vector3 mouse_screen = Mouse.current.position.ReadValue();
            mouse_screen.z = zOffset;
            Vector3 mouse_world = mainCamera.ScreenToWorldPoint(mouse_screen);

            object_selected.position = mouse_world + grabOffset;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame) {
            object_selected = null;
        }
    }
}
}
