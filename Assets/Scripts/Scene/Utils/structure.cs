// data.cs
using System;
using System.Collections.Generic;
using UnityEngine;


namespace scener.scene {
//Define scene object ID
public class UID : MonoBehaviour {
  //---------------------------

  public string ID = Guid.NewGuid().ToString();

  //---------------------------
}

//Convert Unity vector3 to json readable vec3
[Serializable]
public struct vec3 {
  //---------------------------

  public float x, y, z;
  public vec3(Vector3 v) {x = v.x; y = v.y; z = v.z;}

  //---------------------------
}

//Main structure to store each scene elements
[Serializable]
public class SceneObject {
  //---------------------------

  public string id;
  public string name;
  public string type;
  public string node;
  public vec3 position;
  public vec3 rotation;
  public vec3 scale;
  public List<SceneObject> children = new List<SceneObject>();

  //---------------------------
}
}
