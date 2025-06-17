// exporter.cs
using System;
using System.IO;
using UnityEngine;

namespace scener.scene
{
    public class SceneExporter : MonoBehaviour
    {
        public void ExportSceneToJson(string path)
        {
            //---------------------------

            //Create and fill current scene graph
            SceneGraph graph = new SceneGraph();
            graph.GraphCompletion();

            //Add scene name and timestamp at top of the json
            var exportData = new
            {
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                timestamp = DateTime.Now.ToString("o"), // ISO 8601 format
                graph = graph.graph,
            };

            //Serialize and export to json
            Json.write_on_file(exportData, path);
            Debug.Log($"Scene exported to: {path}");

            //---------------------------
        }

        void Start()
        {
            //---------------------------

            //For now just save in Assets folder
            string path = Path.Combine(Application.dataPath, "scene_export.json");
            this.ExportSceneToJson(path);

            //---------------------------
        }
    }
}
