using UnityEngine;
using UnityEngine.InputSystem;

namespace scener.input
{
    public class DragObject : MonoBehaviour
    {
        private Camera mainCamera;
        private Transform selectedObject;
        private float zOffset;

        void Start()
        {
            mainCamera = Camera.main;
        }

        void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    selectedObject = hit.transform;
                    zOffset = Vector3.Distance(
                        mainCamera.transform.position,
                        selectedObject.position
                    );
                }
            }

            if (Mouse.current.leftButton.isPressed && selectedObject != null)
            {
                Vector3 mousePosition = Mouse.current.position.ReadValue();
                mousePosition.z = zOffset;
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
                selectedObject.position = worldPosition;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                selectedObject = null;
            }
        }
    }
}
