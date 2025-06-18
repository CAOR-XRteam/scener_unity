using System.Collections.Generic;
using Newtonsoft.Json;
using SceneDeserialization;
using UnityEngine;

public class SceneBuilder : MonoBehaviour
{
    private Transform _generatedContentRoot;
    private const string ROOT_NAME = "GENERATED SCENE";

    [ContextMenu("Build Scene From JSON")]
    public void BuildSceneFromJSON(string json)
    {
        try
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
            };

            var scene =
                JsonConvert.DeserializeObject<Scene>(json, settings)
                ?? throw new System.Exception("Deserialization resulted in a null object.");

            ClearScene();
            BuildSkybox(scene.skybox);
            foreach (var node in scene.graph)
            {
                CreateGameObject(node, _generatedContentRoot);
            }

            Debug.Log("Scene graph built successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error building scene from JSON: {e.Message}\n{e.StackTrace}");
        }
    }

    private void CreateGameObject(SceneObject node, Transform parent)
    {
        GameObject newObj = new(node.id);
        newObj.transform.SetParent(parent);

        newObj.transform.SetLocalPositionAndRotation(
            node.position.ToUnityVector3(),
            Quaternion.Euler(node.rotation.ToUnityVector3())
        );
        newObj.transform.localScale = node.scale.ToUnityVector3();

        BuildComponents(newObj, node.components);

        if (node.children != null)
        {
            foreach (var childNode in node.children)
            {
                CreateGameObject(childNode, newObj.transform);
            }
        }
    }

    private void BuildComponents(GameObject target, List<SceneComponent> components)
    {
        if (components == null)
            return;

        foreach (var componentData in components)
        {
            switch (componentData)
            {
                case PrimitiveObject primitive:
                    BuildPrimitive(target, primitive);
                    break;
                case DynamicObject dynamic:
                    BuildDynamicModel(target, dynamic);
                    break;
                case BaseLight light:
                    BuildLight(target, light);
                    break;
            }
        }
    }

    private void BuildPrimitive(GameObject target, PrimitiveObject data)
    {
        PrimitiveType primitiveType = data.shape switch
        {
            ShapeType.Cube => PrimitiveType.Cube,
            ShapeType.Sphere => PrimitiveType.Sphere,
            ShapeType.Cylinder => PrimitiveType.Cylinder,
            ShapeType.Capsule => PrimitiveType.Capsule,
            ShapeType.Plane => PrimitiveType.Plane,
            ShapeType.Quad => PrimitiveType.Quad,
            _ => throw new System.NotImplementedException(),
        };
        GameObject tempPrimitive = GameObject.CreatePrimitive(primitiveType);
        target.AddComponent<MeshFilter>().sharedMesh = tempPrimitive
            .GetComponent<MeshFilter>()
            .sharedMesh;

        target.AddComponent<MeshRenderer>();

        DestroyImmediate(tempPrimitive);

        var renderer = target.GetComponent<MeshRenderer>();

        Material newMaterial = renderer.material;
        newMaterial.color = data.color.ToUnityColor();
    }

    private void BuildDynamicModel(GameObject target, DynamicObject data)
    {
        GameObject modelAsset = Resources.Load<GameObject>(data.id);
        if (modelAsset != null)
        {
            GameObject _ = Instantiate(modelAsset, target.transform);
        }
        else
        {
            Debug.LogWarning($"Could not find model asset '{data.id}' in Resources folder.");
        }
    }

    private void ClearScene()
    {
        GameObject existingRoot = GameObject.Find(ROOT_NAME);
        if (existingRoot != null)
        {
            Destroy(existingRoot);
        }
        _generatedContentRoot = new GameObject(ROOT_NAME).transform;
    }

    private void BuildSkybox(SceneDeserialization.Skybox skyboxData)
    {
        if (skyboxData == null)
            return;

        switch (skyboxData.type)
        {
            case SkyboxType.Sun:
                var sun = skyboxData as SunSkybox;

                RenderSettings.skybox = new Material(Shader.Find("Skybox/Horizon With Sun Skybox"));
                RenderSettings.skybox.SetColor("_SkyColor1", sun.top_color.ToUnityColor());
                RenderSettings.skybox.SetFloat("_SkyExponent1", sun.top_exponent);
                RenderSettings.skybox.SetColor("_SkyColor2", sun.horizon_color.ToUnityColor());
                RenderSettings.skybox.SetColor("_SkyColor3", sun.bottom_color.ToUnityColor());
                RenderSettings.skybox.SetFloat("_SkyExponent2", sun.bottom_exponent);
                RenderSettings.skybox.SetFloat("_SkyIntensity", sun.sky_intensity);
                RenderSettings.skybox.SetColor("_SunColor", sun.sun_color.ToUnityColor());
                RenderSettings.skybox.SetFloat("_SunIntensity", sun.sun_intensity);
                RenderSettings.skybox.SetFloat("_SunAlpha", sun.sun_alpha);
                RenderSettings.skybox.SetFloat("_SunBeta", sun.sun_beta);
                RenderSettings.skybox.SetVector("_SunVector", sun.sun_vector.ToUnityVector4());
                // RenderSettings.sun = null;

                break;

            case SkyboxType.Cubed:
                var cubed = skyboxData as CubedSkybox;

                Cubemap cubemap = Resources.Load<Cubemap>(cubed.cube_map);
                if (cubemap != null)
                {
                    RenderSettings.skybox = new Material(Shader.Find("Skybox/Cubemap"));
                    RenderSettings.skybox.SetTexture("_Tex", cubemap);
                    RenderSettings.skybox.SetColor("_Tint", cubed.tint_color.ToUnityColor());
                    RenderSettings.skybox.SetFloat("_Exposure", cubed.exposure);
                    RenderSettings.skybox.SetFloat("_Rotation", cubed.rotation);
                }
                else
                {
                    Debug.LogWarning($"Cubemap '{cubed.cube_map}' not found in Resources folder.");
                }
                break;

            case SkyboxType.Gradient:
                var grad = skyboxData as GradientSkybox;

                Shader gradientShader = Shader.Find("Skybox/Gradient Skybox");

                RenderSettings.skybox = new Material(gradientShader);

                RenderSettings.skybox.SetColor("_Color1", grad.color1.ToUnityColor());
                RenderSettings.skybox.SetColor("_Color2", grad.color2.ToUnityColor());
                RenderSettings.skybox.SetVector("_UpVector", grad.up_vector.ToUnityVector4());
                RenderSettings.skybox.SetFloat("_Intensity", grad.intensity);
                RenderSettings.skybox.SetFloat("_Exponent", grad.exponent);

                break;
        }
    }

    private void BuildLight(GameObject target, BaseLight lightData)
    {
        var light = target.AddComponent<Light>();

        light.color = lightData.color.ToUnityColor();
        light.intensity = lightData.intensity;
        light.bounceIntensity = lightData.indirect_multiplier;

        switch (lightData.type)
        {
            case SceneDeserialization.LightType.Spot:
                var spot = lightData as SpotLight;
                light.type = UnityEngine.LightType.Spot;
                light.range = spot.range;
                light.spotAngle = spot.spot_angle;
                SetLightModeAndShadows(light, spot.mode, spot.shadow_type);
                break;
            case SceneDeserialization.LightType.Directional:
                var directional = lightData as DirectionalLight;
                light.type = UnityEngine.LightType.Directional;
                SetLightModeAndShadows(light, directional.mode, directional.shadow_type);
                break;
            case SceneDeserialization.LightType.Point:
                var point = lightData as PointLight;
                light.type = UnityEngine.LightType.Point;
                light.range = point.range;
                SetLightModeAndShadows(light, point.mode, point.shadow_type);
                break;
            case SceneDeserialization.LightType.Area:
                var area = lightData as AreaLight;
                light.type = UnityEngine.LightType.Rectangle;

                if (area.shape == SceneDeserialization.LightShape.Rectangle)
                {
                    light.areaSize = new Vector2(area.width ?? 1, area.height ?? 1);
                }
                else if (area.shape == SceneDeserialization.LightShape.Disk)
                {
                    light.type = UnityEngine.LightType.Disc;
                    light.areaSize = new Vector2(area.radius.Value, area.radius.Value);
                }

                light.lightmapBakeType = LightmapBakeType.Baked;
                break;
        }
    }

    private void SetLightModeAndShadows(Light light, LightMode mode, ShadowType shadowType)
    {
        light.lightmapBakeType = mode switch
        {
            LightMode.Baked => LightmapBakeType.Baked,
            LightMode.Mixed => LightmapBakeType.Mixed,
            _ => LightmapBakeType.Realtime,
        };

        light.shadows = shadowType switch
        {
            ShadowType.NoShadows => LightShadows.None,
            ShadowType.HardShadows => LightShadows.Hard,
            _ => LightShadows.Soft,
        };
    }

    [ContextMenu("Test Scene: Sun Skybox with Directional and Point Lights")]
    private void TestWithSampleJson()
    {
        string sampleJson = @"{
            'action': 'scene_generation',
            'message': 'Scene description has been successfully generated.',
            'final_scene_json': {
                'skybox': {
                    'type': 'sun',
                    'top_color': {'r': 0.2, 'g': 0.4, 'b': 0.8, 'a': 1.0},
                    'top_exponent': 1.0,
                    'horizon_color': {'r': 0.9, 'g': 0.8, 'b': 0.6, 'a': 1.0},
                    'bottom_color': {'r': 0.3, 'g': 0.3, 'b': 0.35, 'a': 1.0},
                    'bottom_exponent': 1.0,
                    'sky_intensity': 1.2,
                    'sun_color': {'r': 1.0, 'g': 0.9, 'b': 0.8, 'a': 1.0},
                    'sun_intensity': 1.5,
                    'sun_alpha': 20.0,
                    'sun_beta': 20.0,
                    'sun_vector': {'x': 0.5, 'y': 0.5, 'z': 0.0, 'w': 0.0}
                },
                'lights': [
                    {
                        'id': 'light1', 'type': 'directional', 'position': {'x': 0, 'y': 0, 'z': 0},
                        'rotation': {'x': 50, 'y': -30, 'z': 0}, 'scale': {'x': 1, 'y': 1, 'z': 1},
                        'color': {'r': 1.0, 'g': 0.95, 'b': 0.85, 'a': 1.0},
                        'intensity': 1.1, 'indirect_multiplier': 1.0,
                        'Mode': 'realtime', 'shadow_type': 'soft_shadows'
                    },
                    {
                        'id': 'light2', 'type': 'point', 'position': {'x': 5, 'y': 2, 'z': 3},
                        'rotation': {'x': 0, 'y': 0, 'z': 0}, 'scale': {'x': 1, 'y': 1, 'z': 1},
                        'color': {'r': 1.0, 'g': 0.5, 'b': 0.2, 'a': 1.0},
                        'intensity': 2.5, 'indirect_multiplier': 1.0, 'range': 15.0,
                        'mode': 'mixed', 'shadow_type': 'hard_shadows'
                    }
                ],
                'objects': [
                    {
                        'id': 'obj1', 'name': 'GroundPlane', 'type': 'primitive', 'shape': 'plane',
                        'position': {'x': 0, 'y': 0, 'z': 0}, 'rotation': {'x': 0, 'y': 0, 'z': 0},
                        'scale': {'x': 10, 'y': 1, 'z': 10}
                    },
                    {
                        'id': 'obj2', 'name': 'MainCube', 'type': 'primitive', 'shape': 'cube',
                        'position': {'x': 0, 'y': 1, 'z': 5}, 'rotation': {'x': 0, 'y': 25, 'z': 0},
                        'scale': {'x': 2, 'y': 2, 'z': 2}
                    },
                    {
                    'id': 'theatre',
                    'name': 'theatre',
                    'type': 'dynamic',
                    'position': {'x': 0, 'y': 0, 'z': 10},
                    'rotation': {'x': 0, 'y': 180, 'z': 0},
                    'scale': {'x': 5, 'y': 5, 'z': 5}
                }
                ]
            }
        }".Replace("'", "\"");

        BuildSceneFromJSON(sampleJson);
    }

    [ContextMenu("Test Scene: Gradient Sky with Area Light")]
    private void TestGradientScene()
    {
        string sampleJson = @"{
            'action': 'scene_generation',
            'message': 'Scene description has been successfully generated.',
            'final_scene_json': {
                'skybox': {
                    'type': 'gradient',
                    'color1': {'r': 0.8, 'g': 0.3, 'b': 0.1, 'a': 1.0},
                    'color2': {'r': 0.1, 'g': 0.2, 'b': 0.5, 'a': 1.0},
                    'up_vector': {'x': 0, 'y': 1, 'z': 0, 'w': 0},
                    'intensity': 1.0,
                    'exponent': 1.5
                },
                'lights': [
                    {
                        'id': 'area1', 'type': 'area', 'shape': 'rectangle',
                        'position': {'x': 0, 'y': 4, 'z': 0},
                        'rotation': {'x': 90, 'y': 0, 'z': 0}, 'scale': {'x': 1, 'y': 1, 'z': 1},
                        'color': {'r': 1.0, 'g': 0.9, 'b': 0.9, 'a': 1.0},
                        'intensity': 3.0, 'indirect_multiplier': 1.0,
                        'range': 10.0, 'width': 5.0, 'height': 3.0
                    }
                ],
                'objects': [
                    {
                        'id': 'floor', 'name': 'White Floor', 'type': 'primitive', 'shape': 'plane',
                        'position': {'x': 0, 'y': 0, 'z': 0}, 'rotation': {'x': 0, 'y': 0, 'z': 0},
                        'scale': {'x': 1, 'y': 1, 'z': 1}
                    },
                    {
                        'id': 'wall', 'name': 'Back Wall', 'type': 'primitive', 'shape': 'quad',
                        'position': {'x': 0, 'y': 2.5, 'z': 5}, 'rotation': {'x': 0, 'y': 0, 'z': 0},
                        'scale': {'x': 10, 'y': 5, 'z': 1},
                        'color': {'r': 0.1, 'g': 0.1, 'b': 0.15, 'a': 1.0}
                    },
                    {
                        'id': 'obj', 'name': 'MainCube', 'type': 'primitive', 'shape': 'cube',
                        'position': {'x': 0, 'y': 1, 'z': 5}, 'rotation': {'x': 0, 'y': 25, 'z': 0},
                        'scale': {'x': 2, 'y': 2, 'z': 2}
                    },
                ]
            }
        }".Replace("'", "\"");

        BuildSceneFromJSON(sampleJson);
    }

    [ContextMenu("Test Scene: Single Point Light")]
    private void TestMinimalScene()
    {
        string sampleJson = @"{
            'action': 'scene_generation',
            'message': 'Scene description has been successfully generated.',
            'final_scene_json': {
                'skybox': {
                    'type': 'cubed',
                    'tint_color': {'r': 0, 'g': 0, 'b': 0, 'a': 1.0},
                    'exposure': 0.0,
                    'rotation': 0,
                    'cube_map': ''
                },
                'lights': [
                    {
                        'id': 'point1', 'type': 'point', 'position': {'x': 0, 'y': 2, 'z': 0},
                        'rotation': {'x': 0, 'y': 0, 'z': 0}, 'scale': {'x': 1, 'y': 1, 'z': 1},
                        'color': {'r': 1.0, 'g': 0.8, 'b': 0.4, 'a': 1.0},
                        'intensity': 2.0, 'indirect_multiplier': 1.0,
                        'range': 10.0,
                        'mode': 'realtime', 'shadow_type': 'soft_shadows'
                    }
                ],
                'objects': [
                    {
                        'id': 'pedestal', 'name': 'Pedestal', 'type': 'primitive', 'shape': 'cylinder',
                        'position': {'x': 0, 'y': 0.5, 'z': 0}, 'rotation': {'x': 0, 'y': 0, 'z': 0},
                        'scale': {'x': 1, 'y': 1, 'z': 1}
                    }
                ]
            }
        }".Replace("'", "\"");

        BuildSceneFromJSON(sampleJson);
    }
}
