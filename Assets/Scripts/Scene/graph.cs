// scene.cs
using System;
using System.Collections.Generic;
using UnityEngine;

//Construct the scene graph
[Serializable]
public class SceneGraph {
  //INIT
  //=======================
  // Graph definition - top level gameobjects
  public List<SceneObject> graph = new List<SceneObject>();

  // List of to-keep scene objects
  public static List<Type> filter = new List<Type> {
    //---------------------------

    typeof(MeshRenderer),
    typeof(Light)

    //---------------------------
  };

  //SUBFUNCTIONS
  //=======================
  bool HasChildren(GameObject obj) {
    //Does current gameobject has exportable childs (congrat !)
    //---------------------------

    foreach (Transform child in obj.transform) {
      GameObject child_obj = child.gameObject;
      if (IsOk(child_obj) || HasChildren(child_obj)) {
        return true;
      }
    }

    //---------------------------
    return false;
  }
  bool IsOk(GameObject obj) {
    //Does current gameobject is in to_keep type list
    //---------------------------

    foreach (var type in filter) {
      if (obj.GetComponent(type) != null) return true;
    }

    //---------------------------
    return false;
  }
  string GetType(GameObject obj) {
    var types = new List<string>();
    //---------------------------

    if (obj.GetComponent<MeshRenderer>() != null) {
      types.Add("mesh");
    }
    if (obj.GetComponent<Light>() != null) {
      types.Add("light");
    }

    //---------------------------
    return types.Count > 0 ? string.Join(", ", types) : "?";
  }
  string GetNode(GameObject obj) {
    var node = "?";
    //---------------------------

    if (HasChildren(obj)) {
      node = "parent";
    }
    else{
      node = "child";
    }

    //---------------------------
    return node;
  }
  SceneObject FillObjectData(GameObject obj){
    //---------------------------

    //Get object ID
    var uid = obj.GetComponent<UID>();
    string ID = uid != null ? uid.ID : "default_id";

    //Get current object data
    var data = new SceneObject {
        id = ID,
        name = obj.name,
        type = GetType(obj),
        node = GetNode(obj),
        position = new vec3(obj.transform.localPosition),
        rotation = new vec3(obj.transform.localEulerAngles),
        scale = new vec3(obj.transform.localScale)
    };

    //---------------------------
    return data;
  }
  void AddChildrenRecursive(GameObject obj, SceneObject data_parent = null) {
    if (!IsOk(obj) && !HasChildren(obj)) return;
    //---------------------------

    SceneObject data = FillObjectData(obj);

    foreach (Transform child in obj.transform) {
      this.AddChildrenRecursive(child.gameObject, data);
    }

    if (data_parent != null) {
      data_parent.children.Add(data);
    } else {
      graph.Add(data);
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

      if (top_level && not_hide && is_active) {
        this.AddChildrenRecursive(obj);
      }
    }

    //---------------------------
  }

}
