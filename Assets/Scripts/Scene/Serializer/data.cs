// data.cs
using System;
using System.Collections.Generic;
using UnityEngine;


public static class SceneData {
  public static string GetType(GameObject obj) {
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
  public static string GetNode(GameObject obj) {
    var node = "?";
    //---------------------------

    if (SceneUtils.HasChildren(obj)) {
      node = "parent";
    }
    else{
      node = "child";
    }

    //---------------------------
    return node;
  }
  public static void SimplifyScene(SceneObject node) {
    //---------------------------

    if (node.children.Count == 1 && node.type == "?" && node.children[0].type != "?") {
      var child = node.children[0];
      node.type = child.type;
      node.node = child.node;
      node.children = child.children;
    }

    foreach (var child in node.children) {
      SimplifyScene(child);
    }

    //---------------------------
  }
  public static SceneObject FillObjectData(GameObject obj){
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
}
