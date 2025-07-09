using System.Collections.Generic;
using Newtonsoft.Json;
using Scener.Sdk;
using UnityEngine;

namespace Scener.Exporter
{
    public class SceneSerializer : MonoBehaviour
    {
        public string generatedJson;

        public string SerializeScene()
        {
            List<SceneObject> graph = new();
            List<GameObject> topLevelObjects = new();

            SceneMarker sceneRoot = FindFirstObjectByType<SceneMarker>();

            if (sceneRoot == null)
            {
                Debug.LogError("Could not find SceneMarker component. Cannot serialize.");
                throw new System.Exception(
                    "Could not find SceneMarker component. Cannot serialize."
                );
            }

            GameObject rootObject = sceneRoot.gameObject;
            Debug.Log($"Serializing scene from root object: '{rootObject.name}'");

            foreach (Transform child in rootObject.transform)
            {
                topLevelObjects.Add(child.gameObject);
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
                name = rootObject.name,
                skybox = MapSkybox(),
                graph = graph,
            };

            JsonSerializerSettings settings = new()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            generatedJson = JsonConvert.SerializeObject(scene, settings);

            // File.WriteAllText($"Assets/Resources/{rootObject.name}.json", generatedJson);

            Debug.Log("Scene serialized successfully!");

            return generatedJson;
        }

        private SceneObject BuildSceneNode(GameObject obj)
        {
            SceneObject node = new()
            {
                id = obj.name,
                parent_id = obj.transform.parent.name,
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
            else if (
                obj.GetComponent<MeshFilter>() != null
                && !ObjectConverter.IsPrimitive(obj, out _)
            )
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
}
