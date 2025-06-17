using System.Collections.Generic;
using Newtonsoft.Json;
using SceneDeserialization;
using UnityEngine;

public class SceneBuilder : MonoBehaviour
{
    private Transform _generatedContentRoot;
    private const string ROOT_NAME = "GENERATED SCENE";

    public void BuildSceneFromJSON(string json)
    {
        Debug.Log("Received JSON, generating scene...");
        try
        {
            var output = JsonConvert.DeserializeObject<FinalDecompositionOutput>(json);

            if (output == null || output.final_scene_json == null)
            {
                Debug.LogError("Deserialization failed or scene data is null.");
                return;
            }

            Scene sceneData = output.final_scene_json;

            ClearScene();

            BuildSkybox(sceneData.skybox);
            BuildLights(sceneData.lights);
            BuildObjects(sceneData.objects);

            Debug.Log("Scene generated successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error building scene from JSON: {e}");
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

                RenderSettings.skybox = new Material(Shader.Find("Skybox/Procedural"));
                RenderSettings.skybox.SetColor("_SkyTint", sun.top_color.ToUnityColor());
                RenderSettings.skybox.SetColor("_GroundColor", sun.bottom_color.ToUnityColor());
                RenderSettings.sun = null;
                RenderSettings.ambientIntensity = sun.sky_intensity;

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

    private void BuildLights(List<BaseLight> lightsData)
    {
        if (lightsData == null)
            return;

        foreach (var lightData in lightsData)
        {
            GameObject lightGo = new($"Light_{lightData.id}");
            lightGo.transform.SetParent(_generatedContentRoot);
            lightGo.transform.SetPositionAndRotation(
                lightData.position.ToUnityVector3(),
                Quaternion.Euler(lightData.rotation.ToUnityVector3())
            );

            Light lightComponent = lightGo.AddComponent<Light>();
            lightComponent.color = lightData.color.ToUnityColor();
            lightComponent.intensity = lightData.intensity;
            lightComponent.bounceIntensity = lightData.indirect_multiplier;

            switch (lightData.type)
            {
                case SceneDeserialization.LightType.Spot:
                    var spot = lightData as SpotLight;
                    lightComponent.type = UnityEngine.LightType.Spot;
                    lightComponent.range = spot.range;
                    lightComponent.spotAngle = spot.spot_angle;
                    SetLightModeAndShadows(lightComponent, spot.mode, spot.shadow_type);
                    break;
                case SceneDeserialization.LightType.Directional:
                    var directional = lightData as DirectionalLight;
                    lightComponent.type = UnityEngine.LightType.Directional;
                    SetLightModeAndShadows(
                        lightComponent,
                        directional.mode,
                        directional.shadow_type
                    );
                    break;
                case SceneDeserialization.LightType.Point:
                    var point = lightData as PointLight;
                    lightComponent.type = UnityEngine.LightType.Point;
                    lightComponent.range = point.range;
                    SetLightModeAndShadows(lightComponent, point.mode, point.shadow_type);
                    break;
                case SceneDeserialization.LightType.Area:
                    var area = lightData as AreaLight;
                    lightComponent.type = UnityEngine.LightType.Rectangle;

                    if (area.shape == SceneDeserialization.LightShape.Rectangle)
                    {
                        lightComponent.areaSize = new Vector2(area.width ?? 1, area.height ?? 1);
                    }
                    else if (area.shape == SceneDeserialization.LightShape.Disk)
                    {
                        lightComponent.type = UnityEngine.LightType.Disc;
                        lightComponent.areaSize = new Vector2(area.radius.Value, area.radius.Value);
                    }

                    lightComponent.lightmapBakeType = LightmapBakeType.Baked;
                    break;
            }
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

    private void BuildObjects(List<SceneObject> objectsData)
    {
        if (objectsData == null)
            return;

        foreach (var objData in objectsData)
        {
            GameObject newObj = null;

            if (objData.type == SceneObjectType.Primitive)
            {
                if (objData.shape.HasValue)
                {
                    PrimitiveType primitiveType = objData.shape.Value switch
                    {
                        ShapeType.Cube => PrimitiveType.Cube,
                        ShapeType.Sphere => PrimitiveType.Sphere,
                        ShapeType.Cylinder => PrimitiveType.Cylinder,
                        ShapeType.Capsule => PrimitiveType.Capsule,
                        ShapeType.Plane => PrimitiveType.Plane,
                        ShapeType.Quad => PrimitiveType.Quad,
                        _ => throw new System.NotImplementedException(),
                    };
                    newObj = GameObject.CreatePrimitive(primitiveType);
                    newObj.name = objData.name;
                }
                else
                {
                    Debug.LogWarning(
                        $"Primitive object '{objData.name}' has no shape defined. Creating a placeholder cube."
                    );
                    newObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    newObj.name = $"{objData.name} (Placeholder)";
                }
            }
            else if (objData.type == SceneObjectType.Dynamic)
            {
                string prefabPath = $"Prefabs/{objData.id}.glb";
                GameObject prefab = Resources.Load<GameObject>(prefabPath);
                if (prefab != null)
                {
                    newObj = Instantiate(prefab);
                    newObj.name = objData.name;
                }
                else
                {
                    Debug.LogWarning(
                        $"Prefab not found at 'Resources/{prefabPath}'. Creating a placeholder cube."
                    );
                    newObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    newObj.name = $"{objData.name} (Placeholder)";
                }
            }

            if (newObj != null)
            {
                newObj.transform.SetParent(_generatedContentRoot);
                newObj.transform.position = objData.position.ToUnityVector3();
                newObj.transform.rotation = Quaternion.Euler(objData.rotation.ToUnityVector3());
                newObj.transform.localScale = objData.scale.ToUnityVector3();
            }
        }
    }

    [ContextMenu("Test Scene Build")]
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
                ]
            }
        }".Replace("'", "\"");

        BuildSceneFromJSON(sampleJson);
    }
}
