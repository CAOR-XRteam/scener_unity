using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;


namespace scener.scene {
public class SceneUpdater : MonoBehaviour {
    public Transform rootParent; // Assign root parent in Inspector (optional)
    private Dictionary<string, GameObject> sceneObjects = new();

    void Start() {
      //---------------------------

      string path = Path.Combine(Application.dataPath, "scene_export.json");
      string json = File.ReadAllText(path);
      JObject scene = JObject.Parse(json);
      JArray graph = (JArray)scene["graph"];

      // Build current scene object map
      foreach (var obj in GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None)){
        sceneObjects[obj.name] = obj.gameObject;
      }

      // Process graph
      HashSet<string> updatedNames = new();
      foreach (var node in graph) {
        ProcessNode(node, rootParent, updatedNames);
      }

      // Remove GameObjects not in JSON
      foreach (var objName in sceneObjects.Keys) {
        if (!updatedNames.Contains(objName)) {
          Destroy(sceneObjects[objName]);
        }
      }

      //---------------------------
    }

    void ProcessNode(JToken node, Transform parent, HashSet<string> updatedNames) {
        string name = node["name"].ToString();
        updatedNames.Add(name);

        GameObject obj;
        if (!sceneObjects.ContainsKey(name)) {
            obj = new GameObject(name);
            obj.transform.parent = parent;
            // Load mesh/asset if needed based on node["type"] etc.
        } else {
            obj = sceneObjects[name];
            obj.transform.parent = parent;
        }

        // Update transform
        obj.transform.localPosition = ToVector3(node["position"]);
        obj.transform.localRotation = Quaternion.Euler(ToVector3(node["rotation"]));
        obj.transform.localScale = ToVector3(node["scale"]);

        // Recursive children
        foreach (var child in node["children"]) {
            ProcessNode(child, obj.transform, updatedNames);
        }
    }

    Vector3 ToVector3(JToken token) {
        return new Vector3(
            token["x"].Value<float>(),
            token["y"].Value<float>(),
            token["z"].Value<float>()
        );
    }
}
}
