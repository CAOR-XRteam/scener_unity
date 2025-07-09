using System.Threading.Tasks;
using GLTFast;
using Scener.Sdk;
using UnityEngine;

namespace Scener.ModelInstantiator
{
    public class ObjectBuilder : MonoBehaviour
    {
        private GameObject currentModel;

        public void LoadAndPlaceModel(AppMediaAsset modelAsset)
        {
            if (currentModel != null)
            {
                Destroy(currentModel);
                currentModel = null;
            }

            if (modelAsset == null || modelAsset.Data == null || modelAsset.Data.Length == 0)
            {
                Debug.LogError("Model asset is null or contains no data.");
                return;
            }

            LoadFromGLBBytes(modelAsset);
        }

        private async void LoadFromGLBBytes(AppMediaAsset modelAsset)
        {
            var gltf = new GltfImport();

            Debug.Log("Attempting to load GLB model from memory...");

            try
            {
                bool success = await gltf.Load(modelAsset.Data);
                if (success)
                {
                    currentModel = new($"{modelAsset.Id}");
                    success = await gltf.InstantiateMainSceneAsync(currentModel.transform);

                    if (success)
                    {
                        PlaceModelInCenter(currentModel);
                        Debug.Log($"Successfully loaded and placed model: {currentModel.name}");
                    }
                    else
                    {
                        Debug.LogError("Failed to instantiate GLB scene.");
                        Destroy(currentModel);
                    }
                }
                else
                {
                    Debug.LogError("Failed to load GLB bytes.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception during GLB load: {e.Message}\n{e.StackTrace}");
            }
        }

        private void PlaceModelInCenter(GameObject modelInstance)
        {
            if (modelInstance != null)
            {
                transform.position = UnityEngine.Vector3.zero;
                transform.rotation = Quaternion.identity;

                modelInstance.transform.localPosition = UnityEngine.Vector3.zero;
                modelInstance.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
