// scene.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace scener.scene {
//Construct the scene graph
[Serializable]
public class SceneGraph {
  //INIT
  //=======================
  // Graph definition - top level gameobjects
  public List<SceneObject> graph = new List<SceneObject>();

  //SUBFUNCTIONS
  //=======================
  void AddChildrenRecursive(GameObject obj, SceneObject data_parent = null) {
    if (!SceneUtils.IsOk(obj) && !SceneUtils.HasChildren(obj)) return;
    //---------------------------

    SceneObject data = SceneData.FillObjectData(obj);

    foreach (Transform child in obj.transform) {
      this.AddChildrenRecursive(child.gameObject, data);
    }

    if (data_parent != null) {
      data_parent.children.Add(data);
    } else {
      graph.Add(data);
      SceneData.SimplifyScene(data);
    }

    //---------------------------
  }

  //MAIN FUNCTION
  //=======================
  public void GraphCompletion() {
    //---------------------------

    foreach (var obj in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None)) {
      bool top_level = (obj.transform.parent == null);
      bool not_hide = (obj.hideFlags == HideFlags.None);
      bool is_active = obj.activeInHierarchy;
      bool is_ghost = (obj.name == "Ghost");

      if (top_level && not_hide && is_active && !is_ghost) {
        this.AddChildrenRecursive(obj);
      }

    }

    //---------------------------
  }

}
}
