using UnityEngine;

namespace scener.scene {
public class CreateRectangle : MonoBehaviour {
  GameObject rectangle;
  GameObject parent;

  void Start() {
    //---------------------------

    parent = GameObject.Find("Ghost");
    if (parent == null) {
      parent = new GameObject("Ghost");
    }

    rectangle = GameObject.CreatePrimitive(PrimitiveType.Cube);
    rectangle.name = "ghost_1";
    rectangle.transform.localScale = new Vector3(1, 2, 1f);
    rectangle.transform.position = new Vector3(0, 10, 0);
    rectangle.transform.SetParent(parent.transform);

    //---------------------------
  }

  public void DeleteRectangle() {
    //---------------------------

    if (rectangle != null) {
      Destroy(rectangle);
      rectangle = null;

      if (parent.transform.childCount == 0) {
        Destroy(parent);
        parent = null;
      }
    }

    //---------------------------
  }
}
}
