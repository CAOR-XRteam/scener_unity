using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Scener.Sdk;
using UnityEngine;

public class SceneSerializer : MonoBehaviour
{
    public string generatedJson;

    private void SerializeScene(string scene_name = null)
    {
        List<SceneObject> graph = new();
        List<GameObject> topLevelObjects = new();

        if (!string.IsNullOrEmpty(scene_name))
        {
            GameObject rootObject = GameObject.Find(scene_name);

            if (rootObject == null)
            {
                Debug.LogError(
                    $"Could not find a GameObject named '{scene_name}' to use as a serialization root."
                );
                return;
            }

            Debug.Log($"Serializing GameObject: '{scene_name}'.");

            foreach (Transform child in rootObject.transform)
            {
                topLevelObjects.Add(child.gameObject);
            }
        }
        else
        {
            Debug.Log("No specific root object provided. Serializing entire scene.");

            foreach (GameObject obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (obj.transform.parent == null)
                {
                    topLevelObjects.Add(obj);
                }
            }
        }

        foreach (GameObject rootObj in topLevelObjects)
        {
            if (rootObj.activeInHierarchy && rootObj.hideFlags == HideFlags.None)
            {
                graph.Add(BuildSceneNode(rootObj));
            }
        }

        Scene scene = new()
        {
            name = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            skybox = MapSkybox(),
            graph = graph,
        };

        JsonSerializerSettings settings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        generatedJson = JsonConvert.SerializeObject(scene, settings);

        if (scene_name is null or "")
        {
            scene_name = "scene";
        }

        File.WriteAllText($"Assets/Resources/{scene_name}.json", generatedJson);

        Debug.Log("Scene serialized successfully!");
    }

    [ContextMenu("Serialize Scene from JSON")]
    private void SerializeInEditor()
    {
        SerializeScene("Test Sun Scene");
    }

    private SceneObject BuildSceneNode(GameObject obj)
    {
        SceneObject node = new()
        {
            id = obj.name,
            position = obj.transform.localPosition.ToVector3(),
            rotation = obj.transform.localEulerAngles.ToVector3(),
            scale = obj.transform.localScale.ToVector3(),
            components = MapComponents(obj),
        };

        foreach (Transform child in obj.transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                node.children.Add(BuildSceneNode(child.gameObject));
            }
        }
        return node;
    }

    private List<SceneComponent> MapComponents(GameObject obj)
    {
        List<SceneComponent> components = new();

        if (obj.TryGetComponent(out Light light))
        {
            components.Add(MapLight(light));
        }

        if (ObjectConverter.IsPrimitive(obj, out ShapeType? shape))
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            components.Add(
                new PrimitiveObject
                {
                    shape = shape.Value,
                    color = renderer.material.color.ToColorRGBA(),
                }
            );
        }
        else if (obj.GetComponent<MeshFilter>() != null && !ObjectConverter.IsPrimitive(obj, out _))
        {
            components.Add(new DynamicObject { id = obj.name });
        }

        return components;
    }

    private Scener.Sdk.Skybox MapSkybox()
    {
        Material skyboxMat = RenderSettings.skybox;
        return skyboxMat == null
            ? null
            : skyboxMat.shader.name switch
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

    private BaseLight MapLight(Light light)
    {
        BaseLight lightData = null;
        switch (light.type)
        {
            case UnityEngine.LightType.Directional:
                DirectionalLight directionalData = new()
                {
                    type = Scener.Sdk.LightType.Directional,
                };
                LightConverter.MapLightModeAndShadows(directionalData, light);
                lightData = directionalData;
                break;
            case UnityEngine.LightType.Point:
                PointLight pointData = new()
                {
                    type = Scener.Sdk.LightType.Point,
                    range = light.range,
                };
                LightConverter.MapLightModeAndShadows(pointData, light);
                lightData = pointData;
                break;
            case UnityEngine.LightType.Spot:
                SpotLight spotData = new()
                {
                    type = Scener.Sdk.LightType.Spot,
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
                    type = Scener.Sdk.LightType.Area,
                    shape =
                        (light.type == UnityEngine.LightType.Rectangle)
                            ? Scener.Sdk.LightShape.Rectangle
                            : Scener.Sdk.LightShape.Disk,
                    range = light.range,
                    width = light.areaSize.x,
                    height = light.areaSize.y,
                };
                break;
            default:
                break;
        }

        if (lightData != null)
        {
            LightConverter.MapBaseLightProperties(lightData, light);
        }

        return lightData;
    }
}
