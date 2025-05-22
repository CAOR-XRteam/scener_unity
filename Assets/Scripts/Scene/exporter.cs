// exporter.cs
using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class SceneExporter : MonoBehaviour {
  public void ExportSceneToJson(string path) {
    //---------------------------

    SceneGraph graph = new SceneGraph();
    graph.GraphCompletion();

    var exportData = new {
      sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
      timestamp = DateTime.Now.ToString("o"),  // ISO 8601 format
      graph = graph.graph
    };

    string json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
    File.WriteAllText(path, json);
    Debug.Log($"Scene exported to: {path}");

    //---------------------------
  }

  void Start() {
    //---------------------------

    string path = Path.Combine(Application.dataPath, "scene_export.json");
    this.ExportSceneToJson(path);

    //---------------------------
  }
}
