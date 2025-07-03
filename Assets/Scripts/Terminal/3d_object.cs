using System.Threading.Tasks;
using GLTFast;
using Scener.Sdk;
using UnityEngine;

namespace Ui.Terminal
{
    public class ModelInstantiator : MonoBehaviour
    {
        private GameObject currentModel;

        public async void LoadAndPlaceModel(AppMediaAsset modelAsset)
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

            Debug.Log("Attempting to load GLB model from byte data...");

            var gltf = new GltfImport();
            try
            {
                bool success = await gltf.Load(modelAsset.Data);

                if (success)
                {
                    success = await gltf.InstantiateMainSceneAsync(transform);

                    if (success)
                    {
                        currentModel = transform.GetChild(transform.childCount - 1).gameObject;
                        currentModel.name = $"{modelAsset.Id}";
                        PlaceModelInCenter(currentModel);

                        Debug.Log($"Successfully loaded and placed model: {currentModel.name}");
                    }
                    else
                    {
                        Debug.LogError("glTFast failed to instantiate the gameobject.");
                    }
                }
                else
                {
                    Debug.LogError("glTFast failed to load the GLB data.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(
                    $"An error occurred during GLB loading: {e.Message}\n{e.StackTrace}"
                );
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
