using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Newtonsoft.Json;
using Scener.Sdk;
using Unity.VisualScripting;
using UnityEngine;

namespace Scener.Importer
{
    public class SceneBuilder : MonoBehaviour
    {
        // TODO: rethink partial deserialization

        public void ModifySceneFromJSON(string json)
        {
            try
            {
                Debug.Log($"Received JSON for scene modification:\n{json}");
                SceneUpdate patch =
                    JsonConvert.DeserializeObject<SceneUpdate>(json)
                    ?? throw new System.Exception(
                        "Deserialization of the scene patch resulted in a null object."
                    );
                if (_generatedContentRoot == null)
                {
                    var marker = FindFirstObjectByType<SceneMarker>();
                    if (marker != null)
                    {
                        _generatedContentRoot = marker.transform;
                    }
                    else
                    {
                        Debug.LogWarning("SceneMarker root not found. Nothing to update");
                        return;
                    }
                }

                if (patch.Skybox != null)
                {
                    BuildSkybox(patch.Skybox);
                }

                ProcessDeletions(patch.ObjectsToDelete);
                ProcessUpdates(patch.ObjectsToUpdate);
                ProcessRegenerations(patch.ObjectsToRegenerate);
                ProcessAdditions(patch.ObjectsToAdd);

                Debug.Log("Scene patch applied successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError(
                    $"Error applying scene patch from JSON: {e.Message}\n{e.StackTrace}"
                );
            }
        }

        private void ProcessDeletions(List<string> objectIdsToDelete)
        {
            if (objectIdsToDelete == null || !objectIdsToDelete.Any())
                return;

            Debug.Log($"Processing {objectIdsToDelete.Count} deletions.");
            foreach (string objectId in objectIdsToDelete)
            {
                GameObject objectToDelete = FindObjectInScene(objectId);
                if (objectToDelete != null)
                {
                    Debug.Log($"Deleting object: {objectId}");
                    Destroy(objectToDelete);
                }
                else
                {
                    Debug.LogWarning($"Could not find object '{objectId}' to delete.");
                }
            }
        }

        private void ProcessUpdates(List<SceneObjectUpdate> updates)
        {
            if (updates == null || !updates.Any())
                return;

            Debug.Log($"Processing {updates.Count} updates.");
            foreach (var updateData in updates)
            {
                GameObject targetObject = FindObjectInScene(updateData.Id);
                if (targetObject != null)
                {
                    ApplyUpdateToGameObject(targetObject, updateData);
                }
                else
                {
                    Debug.LogWarning($"Could not find object '{updateData.Id}' to update.");
                }
            }
        }

        private void ProcessRegenerations(List<RegenerationInfo> regenerations)
        {
            if (regenerations == null || !regenerations.Any())
                return;

            Debug.Log($"Processing {regenerations.Count} regenerations.");
            foreach (var regenInfo in regenerations)
            {
                Debug.Log(
                    $"Object '{regenInfo.Id}' is marked for regeneration with prompt: '{regenInfo.Prompt}'"
                );
                HandleRegeneration(regenInfo);
            }
        }

        private void ProcessAdditions(List<SceneObject> objectsToAdd)
        {
            if (objectsToAdd == null || !objectsToAdd.Any())
                return;

            Debug.Log($"Processing {objectsToAdd.Count} additions.");
            foreach (SceneObject newObjectData in objectsToAdd)
            {
                if (FindObjectInScene(newObjectData.id) != null)
                {
                    Debug.LogWarning(
                        $"Object with id '{newObjectData.id}' already exists. Skipping addition."
                    );
                    continue;
                }

                Transform parentTransform = FindParentTransform(newObjectData.parent_id);
                CreateGameObject(newObjectData, parentTransform);
            }
        }

        private void ApplyUpdateToGameObject(GameObject target, SceneObjectUpdate updateData)
        {
            if (!string.IsNullOrEmpty(updateData.ParentId))
            {
                Transform newParentTransform = FindParentTransform(updateData.ParentId);
                if (target.transform.parent != newParentTransform)
                {
                    Debug.Log(
                        $"Changing parent of '{updateData.Id}' from '{target.transform.parent.name}' to '{newParentTransform.name}'."
                    );
                    target.transform.SetParent(newParentTransform, worldPositionStays: false);
                }
            }

            if (updateData.Position != null)
                target.transform.localPosition = updateData.Position.ToUnityVector3();
            if (updateData.Rotation != null)
                target.transform.localRotation = Quaternion.Euler(
                    updateData.Rotation.ToUnityVector3()
                );
            if (updateData.Scale != null)
                target.transform.localScale = updateData.Scale.ToUnityVector3();

            if (updateData.ComponentsToUpdate != null && updateData.ComponentsToUpdate.Any())
            {
                ApplyComponentUpdates(target, updateData.ComponentsToUpdate);
            }
        }

        private void ApplyComponentUpdates(
            GameObject target,
            List<ComponentUpdate> componentUpdates
        )
        {
            foreach (var update in componentUpdates)
            {
                switch (update.component_type)
                {
                    case SceneObjectType.Primitive:
                        ApplyPrimitiveUpdate(target, update as PrimitiveObjectUpdate);
                        break;
                    case SceneObjectType.Light:
                        ApplyLightUpdate(target, update as BaseLightUpdate);
                        break;
                }
            }
        }

        private void ApplyPrimitiveUpdate(GameObject target, PrimitiveObjectUpdate updateData)
        {
            if (updateData == null)
                return;

            var renderer = target.GetComponentInChildren<MeshRenderer>();
            if (renderer == null)
            {
                Debug.LogWarning(
                    $"No MeshRenderer found on {target.name} or children to apply primitive update."
                );
                return;
            }

            if (updateData.color != null)
            {
                renderer.material.color = updateData.color.ToUnityColor();
            }

            if (updateData.shape.HasValue)
            {
                Debug.Log($"Changing shape of {target.name} to {updateData.shape.Value}.");

                if (renderer.gameObject != target)
                    Destroy(renderer.gameObject);

                var primitive = GameObject.CreatePrimitive(
                    updateData.shape.Value.ToUnityPrimitiveShape()
                );
                primitive.transform.SetParent(target.transform, false);
                primitive.transform.SetLocalPositionAndRotation(
                    UnityEngine.Vector3.zero,
                    Quaternion.identity
                );
                primitive.transform.localScale = UnityEngine.Vector3.one;

                var newRenderer = primitive.GetComponent<MeshRenderer>();
                if (updateData.color != null)
                {
                    newRenderer.material.color = updateData.color.ToUnityColor();
                }
            }
        }

        private void ApplyLightUpdate(GameObject target, BaseLightUpdate updateData)
        {
            if (updateData == null)
                return;

            if (!target.TryGetComponent<Light>(out var light))
            {
                light = target.AddComponent<Light>();
            }

            if (updateData.intensity.HasValue)
                light.intensity = updateData.intensity.Value;
            if (updateData.indirect_multiplier.HasValue)
                light.bounceIntensity = updateData.indirect_multiplier.Value;
            if (updateData.color != null)
                light.color = updateData.color.ToUnityColor();

            switch (updateData)
            {
                case SpotLightUpdate spot:
                    light.type = UnityEngine.LightType.Spot;
                    if (spot.range.HasValue)
                        light.range = spot.range.Value;
                    if (spot.spot_angle.HasValue)
                        light.spotAngle = spot.spot_angle.Value;
                    break;

                case PointLightUpdate point:
                    light.type = UnityEngine.LightType.Point;
                    if (point.range.HasValue)
                        light.range = point.range.Value;
                    break;

                case DirectionalLightUpdate _:
                    light.type = UnityEngine.LightType.Directional;
                    break;

                case AreaLightUpdate area:
                    light.type = UnityEngine.LightType.Rectangle;
                    if (area.width.HasValue && area.height.HasValue)
                        light.areaSize = new Vector2(area.width.Value, area.height.Value);
                    break;
            }
        }

        private void HandleRegeneration(RegenerationInfo regenInfo)
        {
            GameObject target = FindObjectInScene(regenInfo.Id);
            if (target == null)
            {
                Debug.LogWarning($"Could not find object '{regenInfo.Id}' to regenerate.");
                return;
            }

            SceneObjectMetadata metadata = _generatedContentRoot
                .GetComponentsInChildren<SceneObjectMetadata>(true)
                .FirstOrDefault(m => m.id == regenInfo.Id);
            metadata.id = regenInfo.NewId;

            BuildDynamic(target, new DynamicObject { id = regenInfo.NewId });
            Debug.Log(
                $"Triggering regeneration for {regenInfo.NewId} with prompt: '{regenInfo.Prompt}'"
            );
        }

        private Transform FindParentTransform(string parentId)
        {
            if (!string.IsNullOrEmpty(parentId))
            {
                GameObject parentObject = FindObjectInScene(parentId);
                if (parentObject != null)
                {
                    return parentObject.transform;
                }
                Debug.LogWarning(
                    $"Could not find specified parent '{parentId}'. Attaching to scene root instead."
                );
            }
            return _generatedContentRoot;
        }

        private GameObject FindObjectInScene(string objectId)
        {
            if (_generatedContentRoot == null)
                return null;

            SceneObjectMetadata metadata = _generatedContentRoot
                .GetComponentsInChildren<SceneObjectMetadata>(true)
                .FirstOrDefault(m => m.id == objectId);

            Debug.Log($"Metadata found: {metadata}");

            return metadata != null ? metadata.gameObject : null;
        }

        private Transform _generatedContentRoot;

        public void BuildSceneFromJSON(string json)
        {
            try
            {
                JsonSerializerSettings settings = new()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore,
                };

                Scene scene =
                    JsonConvert.DeserializeObject<Scene>(json, settings)
                    ?? throw new System.Exception("Deserialization resulted in a null object.");

                ClearScene(scene.name);
                BuildSkybox(scene.skybox);
                foreach (SceneObject node in scene.graph)
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

        private void ClearScene(string scene_name)
        {
            var existingRoot = FindAnyObjectByType<SceneMarker>();
            if (existingRoot != null)
            {
                Destroy(existingRoot.gameObject);
            }
            _generatedContentRoot = new GameObject(scene_name).transform;
            var marker = _generatedContentRoot.AddComponent<SceneMarker>();
            marker.sceneName = scene_name;
        }

        private void CreateGameObject(SceneObject node, Transform parent)
        {
            GameObject newObj = new(node.name);
            newObj.AddComponent<SceneObjectMetadata>().id = node.id;
            newObj.transform.SetParent(parent, worldPositionStays: false);

            // newObj.transform.SetLocalPositionAndRotation(
            //     node.position.ToUnityVector3(),
            //     Quaternion.Euler(node.rotation.ToUnityVector3())
            // );

            newObj.transform.position = node.position.ToUnityVector3();
            newObj.transform.localRotation = Quaternion.Euler(node.rotation.ToUnityVector3());

            UnityEngine.Vector3 parentWorldScale = parent.lossyScale;

            UnityEngine.Vector3 requiredLocalScale = new(
                node.scale.x / parentWorldScale.x,
                node.scale.y / parentWorldScale.y,
                node.scale.z / parentWorldScale.z
            );

            newObj.transform.localScale = requiredLocalScale;

            BuildComponents(newObj, node.components);

            if (node.children != null)
            {
                foreach (SceneObject childNode in node.children)
                {
                    CreateGameObject(childNode, newObj.transform);
                }
            }
        }

        private void BuildComponents(GameObject target, List<SceneComponent> components)
        {
            if (components == null)
            {
                return;
            }

            foreach (SceneComponent componentData in components)
            {
                switch (componentData)
                {
                    case PrimitiveObject primitive:
                        BuildPrimitive(target, primitive);
                        break;
                    case DynamicObject dynamic:
                        BuildDynamic(target, dynamic);
                        break;
                    case BaseLight light:
                        BuildLight(target, light);
                        break;
                    default:
                        break;
                }
            }
        }

        private void BuildPrimitive(GameObject target, PrimitiveObject data)
        {
            GameObject tempPrimitive = GameObject.CreatePrimitive(
                data.shape.ToUnityPrimitiveShape()
            );
            target.AddComponent<MeshFilter>().sharedMesh = tempPrimitive
                .GetComponent<MeshFilter>()
                .sharedMesh;
            _ = target.AddComponent<MeshRenderer>();

            DestroyImmediate(tempPrimitive);

            MeshRenderer renderer = target.GetComponent<MeshRenderer>();

            Material newMaterial = renderer.material;
            newMaterial.color = data.color.ToUnityColor();
        }

        private void BuildDynamic(GameObject target, DynamicObject data)
        {
            GameObject modelAsset = Resources.Load<GameObject>(data.id);
            if (modelAsset != null)
            {
                // With the commented code below, it keeps the original hierarchy of unity when loading a 3D object (theathe gameobject that contains geometry gameobject with mesh and materials)

                // GameObject tmp = Instantiate(modelAsset, target.transform);
                // for (int i = tmp.transform.childCount - 1; i >= 0; i--)
                // {
                //     Transform child = tmp.transform.GetChild(i);
                //     child.SetParent(target.transform, worldPositionStays: false);
                // }

                // Destroy(tmp);

                // With this code, it flattens the hierarchy and only keeps the mesh and materials of the model asset

                MeshFilter sourceMeshFilter = modelAsset.GetComponentInChildren<MeshFilter>();
                MeshRenderer sourceMeshRenderer = modelAsset.GetComponentInChildren<MeshRenderer>();

                if (sourceMeshFilter != null && sourceMeshRenderer != null)
                {
                    if (!target.TryGetComponent<MeshFilter>(out var targetMeshFilter))
                    {
                        targetMeshFilter = target.AddComponent<MeshFilter>();
                    }
                    targetMeshFilter.sharedMesh = sourceMeshFilter.sharedMesh;

                    if (!target.TryGetComponent<MeshRenderer>(out var targetMeshRenderer))
                    {
                        targetMeshRenderer = target.AddComponent<MeshRenderer>();
                    }

                    targetMeshRenderer.sharedMaterials = sourceMeshRenderer.sharedMaterials;
                }
                else
                {
                    Debug.LogWarning(
                        $"Could not find model asset '{data.id}' in Resources folder."
                    );
                }
            }
        }

        private void BuildSkybox(Scener.Sdk.Skybox skyboxData)
        {
            if (skyboxData == null)
            {
                return;
            }

            switch (skyboxData.type)
            {
                case SkyboxType.Sun:
                    SunSkybox sun = skyboxData as SunSkybox;

                    RenderSettings.skybox = new Material(
                        Shader.Find("Skybox/Horizon With Sun Skybox")
                    );
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

                    break;

                case SkyboxType.Cubed:
                    CubedSkybox cubed = skyboxData as CubedSkybox;

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
                        Debug.LogWarning(
                            $"Cubemap '{cubed.cube_map}' not found in Resources folder."
                        );
                    }
                    break;

                case SkyboxType.Gradient:
                    GradientSkybox grad = skyboxData as GradientSkybox;

                    Shader gradientShader = Shader.Find("Skybox/Gradient Skybox");

                    RenderSettings.skybox = new Material(gradientShader);

                    RenderSettings.skybox.SetColor("_Color1", grad.color1.ToUnityColor());
                    RenderSettings.skybox.SetColor("_Color2", grad.color2.ToUnityColor());
                    RenderSettings.skybox.SetVector("_UpVector", grad.up_vector.ToUnityVector4());
                    RenderSettings.skybox.SetFloat("_Intensity", grad.intensity);
                    RenderSettings.skybox.SetFloat("_Exponent", grad.exponent);

                    break;

                default:
                    break;
            }
        }

        private void BuildLight(GameObject target, BaseLight lightData)
        {
            Light light = target.AddComponent<Light>();

            light.color = lightData.color.ToUnityColor();
            light.intensity = lightData.intensity;
            light.bounceIntensity = lightData.indirect_multiplier;

            switch (lightData.type)
            {
                case Scener.Sdk.LightType.Spot:
                    SpotLight spot = lightData as SpotLight;
                    light.type = UnityEngine.LightType.Spot;
                    light.range = spot.range;
                    light.spotAngle = spot.spot_angle;
                    SetLightModeAndShadows(light, spot.mode, spot.shadow_type);
                    break;
                case Scener.Sdk.LightType.Directional:
                    DirectionalLight directional = lightData as DirectionalLight;
                    light.type = UnityEngine.LightType.Directional;
                    SetLightModeAndShadows(light, directional.mode, directional.shadow_type);
                    break;
                case Scener.Sdk.LightType.Point:
                    PointLight point = lightData as PointLight;
                    light.type = UnityEngine.LightType.Point;
                    light.range = point.range;
                    SetLightModeAndShadows(light, point.mode, point.shadow_type);
                    break;
                case Scener.Sdk.LightType.Area:
                    AreaLight area = lightData as AreaLight;
                    light.type = UnityEngine.LightType.Rectangle;

                    if (area.shape == Scener.Sdk.LightShape.Rectangle)
                    {
                        light.areaSize = new Vector2(area.width ?? 1, area.height ?? 1);
                    }
                    else if (area.shape == Scener.Sdk.LightShape.Disk)
                    {
                        light.type = UnityEngine.LightType.Disc;
                        light.areaSize = new Vector2(area.radius.Value, area.radius.Value);
                    }
                    light.lightmapBakeType = LightmapBakeType.Baked;
                    break;
                default:
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

        [ContextMenu("Test Scene: Sun Skybox with Primitives and Dynamic Model")]
        private void TestScene_Sun()
        {
            string json = @"{
            'name': 'Test Sun Scene',
            'timestamp': '2023-10-27T10:00:00Z',
            'skybox': {
                'type': 'sun',
                'top_color': {'r': 0.2, 'g': 0.4, 'b': 0.8}, 'top_exponent': 1.0,
                'horizon_color': {'r': 0.9, 'g': 0.8, 'b': 0.6}, 'bottom_color': {'r': 0.3, 'g': 0.3, 'b': 0.35},
                'bottom_exponent': 1.0, 'sky_intensity': 1.2, 'sun_color': {'r': 1.0, 'g': 0.9, 'b': 0.8},
                'sun_intensity': 1.5, 'sun_alpha': 20.0, 'sun_beta': 20.0,
                'sun_vector': {'x': 0.5, 'y': 0.5, 'z': 0.0, 'w': 0.0}
            },
            'graph': [
                {
                    'id': 'light1', 'name': 'Directional Light',
                    'position': {'x': 0, 'y': 0, 'z': 0}, 'rotation': {'x': 50, 'y': -30, 'z': 0}, 'scale': {'x': 1, 'y': 1, 'z': 1},
                    'components': [
                        {
                            'componentType': 'light',
                            'type': 'directional', 'color': {'r': 1.0, 'g': 0.95, 'b': 0.85}, 'intensity': 1.1,
                            'indirect_multiplier': 1.0, 'mode': 'realtime', 'shadow_type': 'soft_shadows'
                        }
                    ]
                },
                {
                    'id': 'light2', 'name': 'Point Light Source',
                    'position': {'x': 5, 'y': 2, 'z': 3}, 'rotation': {'x': 0, 'y': 0, 'z': 0}, 'scale': {'x': 1, 'y': 1, 'z': 1},
                    'components': [
                        {
                            'componentType': 'light',
                            'type': 'point', 'color': {'r': 1.0, 'g': 0.5, 'b': 0.2}, 'intensity': 2.5,
                            'indirect_multiplier': 1.0, 'range': 15.0, 'mode': 'mixed', 'shadow_type': 'hard_shadows'
                        }
                    ]
                },
                {
                    'id': 'obj1', 'name': 'GroundPlane',
                    'position': {'x': 0, 'y': 0, 'z': 0}, 'rotation': {'x': 0, 'y': 0, 'z': 0}, 'scale': {'x': 10, 'y': 1, 'z': 10},
                    'components': [
                        {
                            'componentType': 'Primitive',
                            'shape': 'plane', 'color': {'r': 0.5, 'g': 0.5, 'b': 0.5}
                        }
                    ]
                },
                {
                    'id': 'obj2', 'name': 'MainCube',
                    'position': {'x': 0, 'y': 1, 'z': 5}, 'rotation': {'x': 0, 'y': 25, 'z': 0}, 'scale': {'x': 2, 'y': 2, 'z': 2},
                    'components': [
                        {
                            'componentType': 'primitive',
                            'shape': 'cube', 'color': {'r': 0.8, 'g': 0.1, 'b': 0.1}
                        }
                    ]
                },
                {
                    'id': 'theatre', 'name': 'TheatreContainer',
                    'position': {'x': -5, 'y': 0, 'z': 10}, 'rotation': {'x': 0, 'y': 180, 'z': 0}, 'scale': {'x': 5, 'y': 5, 'z': 5},
                    'components': [
                        {
                            'componentType': 'dynamic',
                            'id': 'theatre'
                        }
                    ]
                }
            ]
        }".Replace("'", "\"");

            BuildSceneFromJSON(json);
        }

        [ContextMenu("Test Scene: Gradient Sky with Area Light")]
        private void TestScene_Gradient()
        {
            string json = @"{
            'name': 'Test Gradient Scene',
            'timestamp': '2023-10-27T11:00:00Z',
            'skybox': {
                'type': 'gradient', 'color1': {'r': 0.8, 'g': 0.3, 'b': 0.1},
                'color2': {'r': 0.1, 'g': 0.2, 'b': 0.5}, 'up_vector': {'x': 0, 'y': 1, 'z': 0, 'w': 0},
                'intensity': 1.0, 'exponent': 1.5
            },
            'graph': [
                {
                    'id': 'area1', 'name': 'Ceiling Area Light',
                    'position': {'x': 0, 'y': 4, 'z': 0}, 'rotation': {'x': 90, 'y': 0, 'z': 0}, 'scale': {'x': 1, 'y': 1, 'z': 1},
                    'components': [
                        {
                            'type': 'area', 'shape': 'rectangle', 'color': {'r': 1.0, 'g': 0.9, 'b': 0.9},
                            'intensity': 3.0, 'indirect_multiplier': 1.0, 'range': 10.0, 'width': 5.0, 'height': 3.0,'componentType': 'light',
                        }
                    ]
                },
                {
                    'id': 'floor', 'name': 'White Floor',
                    'position': {'x': 0, 'y': 0, 'z': 0}, 'rotation': {'x': 0, 'y': 0, 'z': 0}, 'scale': {'x': 1, 'y': 1, 'z': 1},
                    'components': [
                        {
                            'shape': 'plane', 'color': {'r': 0.9, 'g': 0.9, 'b': 0.9}, 'componentType': 'primitive',
                        }
                    ]
                },
                {
                    'id': 'wall', 'name': 'Back Wall',
                    'position': {'x': 0, 'y': 2.5, 'z': 5}, 'rotation': {'x': 0, 'y': 0, 'z': 0}, 'scale': {'x': 10, 'y': 5, 'z': 1},
                    'components': [
                        {
                            'shape': 'quad', 'color': {'r': 0.1, 'g': 0.1, 'b': 0.15}, 'componentType': 'primitive',
                        }
                    ]
                }
            ]
        }".Replace("'", "\"");

            BuildSceneFromJSON(json);
        }

        [ContextMenu("Test Scene: Minimal Dark Room")]
        private void TestScene_Cube()
        {
            string json = @"{
            'name': 'Test Cube Skybox',
            'timestamp': '2023-10-27T12:00:00Z',
            'skybox': {
                'type': 'cubed', 'tint_color': {'r': 0, 'g': 0, 'b': 0},
                'exposure': 0.0, 'rotation': 0, 'cube_map': ''
            },
            'graph': [
                {
                    'id': 'point1', 'name': 'Single Bulb',
                    'position': {'x': 0, 'y': 2, 'z': 0}, 'rotation': {'x': 0, 'y': 0, 'z': 0}, 'scale': {'x': 1, 'y': 1, 'z': 1},
                    'components': [
                        {
                            'type': 'point', 'color': {'r': 1.0, 'g': 0.8, 'b': 0.4}, 'intensity': 2.0,
                            'indirect_multiplier': 1.0, 'range': 10.0, 'mode': 'realtime', 'shadow_type': 'soft_shadows', 'componentType': 'light',
                        }
                    ]
                },
                {
                    'id': 'pedestal', 'name': 'Pedestal',
                    'position': {'x': 0, 'y': 0.5, 'z': 0}, 'rotation': {'x': 0, 'y': 0, 'z': 0}, 'scale': {'x': 1, 'y': 1, 'z': 1},
                    'components': [
                        {
                            'shape': 'cylinder', 'color': {'r': 0.6, 'g': 0.6, 'b': 0.6}, 'componentType': 'primitive',
                        }
                    ]
                }
            ]
        }".Replace("'", "\"");

            BuildSceneFromJSON(json);
        }
    }
}
