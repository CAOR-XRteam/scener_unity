// exporter.cs
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SceneDeserialization;
using UnityEngine;

// namespace scener.scene
// {
//     public class SceneExporter : MonoBehaviour
//     {
//         public void ExportSceneToJson(string path)
//         {
//             //---------------------------

//             //Create and fill current scene graph
//             SceneGraph graph = new SceneGraph();
//             graph.GraphCompletion();

//             //Add scene name and timestamp at top of the json
//             var exportData = new
//             {
//                 sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
//                 timestamp = DateTime.Now.ToString("o"), // ISO 8601 format
//                 graph = graph.graph,
//             };

//             //Serialize and export to json
//             Json.write_on_file(exportData, path);
//             Debug.Log($"Scene exported to: {path}");

//             //---------------------------
//         }

//         void Start()
//         {
//             //---------------------------

//             //For now just save in Assets folder
//             string path = Path.Combine(Application.dataPath, "scene_export.json");
//             this.ExportSceneToJson(path);

//             //---------------------------
//         }
//     }
// }

public class SceneSerializer : MonoBehaviour
{
    [Tooltip("If set, only objects under this transform will be serialized.")]
    public Transform sceneRootToSerialize;

    [Header("JSON Output")]
    [TextArea(10, 20)]
    public string generatedJson;

    [ContextMenu("Serialize Scene to JSON")]
    private void SerializeScene()
    {
        Scene sceneData = new()
        {
            skybox = MapSkybox(),
            lights = MapLights(),
            objects = MapObjects(),
        };

        JsonSerializerSettings settings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        generatedJson = JsonConvert.SerializeObject(sceneData, settings);
        File.WriteAllText("Assets/Resources/scene.json", generatedJson);

        Debug.Log("Scene serialized successfully!");
    }

    private SceneDeserialization.Skybox MapSkybox()
    {
        Material skyboxMat = RenderSettings.skybox;
        if (skyboxMat == null)
            return null;

        return skyboxMat.shader.name switch
        {
            "Skybox/Horizon With Sun Skybox" => new SunSkybox
            {
                type = SkyboxType.Sun,
                top_color = skyboxMat.GetColor("_SkyColor1").ToColorRGBA(),
                top_exponent = skyboxMat.GetFloat("_SkyExponent1"),
                horizon_color = skyboxMat.GetColor("_SkyColor2").ToColorRGBA(),
                bottom_color = skyboxMat.GetColor("_SkyColor3").ToColorRGBA(),
                bottom_exponent = skyboxMat.GetFloat("_SkyExponent2"),
                sky_intensity = skyboxMat.GetFloat("_SkyIntensity"),
                sun_color = skyboxMat.GetColor("_SunColor").ToColorRGBA(),
                sun_intensity = skyboxMat.GetFloat("_SunIntensity"),
                sun_alpha = skyboxMat.GetFloat("_SunAlpha"),
                sun_beta = skyboxMat.GetFloat("_SunBeta"),
                sun_vector = skyboxMat.GetVector("_SunVector").ToVector4(),
            },
            "Skybox/Cubemap" => new CubedSkybox
            {
                type = SkyboxType.Cubed,
                tint_color = skyboxMat.GetColor("_Tint").ToColorRGBA(),
                exposure = skyboxMat.GetFloat("_Exposure"),
                rotation = skyboxMat.GetFloat("_Rotation"),
                cube_map = skyboxMat.GetTexture("_Tex").name,
            },
            "Skybox/Gradient Skybox" => new GradientSkybox
            {
                type = SkyboxType.Gradient,
                color1 = skyboxMat.GetColor("_Color1").ToColorRGBA(),
                color2 = skyboxMat.GetColor("_Color2").ToColorRGBA(),
                up_vector = skyboxMat.GetVector("_UpVector").ToVector4(),
                intensity = skyboxMat.GetFloat("_Intensity"),
                exponent = skyboxMat.GetFloat("_Exponent"),
            },
            _ => null,
        };
    }

    private List<BaseLight> MapLights()
    {
        var lightsList = new List<BaseLight>();
        Light[] sceneLights;

        if (sceneRootToSerialize != null)
            sceneLights = sceneRootToSerialize.GetComponentsInChildren<Light>();
        else
            sceneLights = FindObjectsByType<Light>(FindObjectsSortMode.None);

        foreach (var light in sceneLights)
        {
            BaseLight lightData = null;
            switch (light.type)
            {
                case UnityEngine.LightType.Directional:
                    var directionalData = new DirectionalLight
                    {
                        type = SceneDeserialization.LightType.Directional,
                    };
                    LightConverter.MapLightModeAndShadows(directionalData, light);
                    lightData = directionalData;
                    break;
                case UnityEngine.LightType.Point:
                    var pointData = new PointLight
                    {
                        type = SceneDeserialization.LightType.Point,
                        range = light.range,
                    };
                    LightConverter.MapLightModeAndShadows(pointData, light);
                    lightData = pointData;
                    break;
                case UnityEngine.LightType.Spot:
                    var spotData = new SpotLight
                    {
                        type = SceneDeserialization.LightType.Spot,
                        range = light.range,
                        spot_angle = light.spotAngle,
                    };
                    LightConverter.MapLightModeAndShadows(spotData, light);
                    lightData = spotData;
                    break;
                case UnityEngine.LightType.Rectangle:
                case UnityEngine.LightType.Disc:
                    lightData = new AreaLight
                    {
                        type = SceneDeserialization.LightType.Area,
                        shape =
                            (light.type == UnityEngine.LightType.Rectangle)
                                ? SceneDeserialization.LightShape.Rectangle
                                : SceneDeserialization.LightShape.Disk,
                        range = light.range,
                        width = light.areaSize.x,
                        height = light.areaSize.y,
                    };
                    break;
            }

            if (lightData != null)
            {
                LightConverter.MapBaseLightProperties(lightData, light);
                lightsList.Add(lightData);
            }
        }
        return lightsList;
    }

    private List<SceneObject> MapObjects()
    {
        var objectsList = new List<SceneObject>();
        MeshRenderer[] renderers;

        if (sceneRootToSerialize != null)
            renderers = sceneRootToSerialize.GetComponentsInChildren<MeshRenderer>();
        else
            renderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);

        foreach (var renderer in renderers)
        {
            if (renderer.GetComponent<Light>() != null)
                continue;

            bool isPrimitive = ObjectConverter.IsPrimitive(renderer.gameObject);

            var objData = new SceneObject
            {
                id = renderer.gameObject.GetInstanceID().ToString(),
                name = renderer.gameObject.name,
                type = isPrimitive ? SceneObjectType.Primitive : SceneObjectType.Dynamic,
                position = renderer.transform.position.ToVector3(),
                rotation = renderer.transform.eulerAngles.ToVector3(),
                scale = renderer.transform.localScale.ToVector3(),
                color = renderer.material.color.ToColorRGBA(),
            };

            if (isPrimitive)
            {
                objData.shape = ObjectConverter.GetPrimitiveShape(renderer.gameObject);
            }

            if (!isPrimitive)
            {
                objData.name = renderer.gameObject.name;
            }

            objectsList.Add(objData);
        }
        return objectsList;
    }
}
