using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ui.Terminal
{
    public class TerminalImage : MonoBehaviour
    {
        public VisualElement imageContainer;
        private readonly List<Texture2D> receivedImages = new();

        void Start()
        {
            // Search UI elements
            //---------------------------

            imageContainer = FindFirstObjectByType<UIDocument>()
                .rootVisualElement.Q<VisualElement>("image_container");

            //---------------------------
        }

        public void LoadAndDisplayImages(List<byte[]> imageBytesList)
        {
            // Load images from bytes and display them
            //---------------------------

            foreach (var bytes in imageBytesList)
            {
                LoadImageFromBytes(bytes);
            }

            DisplayImages(receivedImages);

            Debug.Log($"Loaded {receivedImages.Count} images from {imageBytesList.Count}.");

            //---------------------------
        }

        void LoadImageFromBytes(byte[] bytes)
        {
            Texture2D tex = new(2, 2);
            if (tex.LoadImage(bytes))
            {
                receivedImages.Add(tex);
            }
            else
            {
                Debug.LogWarning("Failed to load image");
            }
        }

        void DisplayImages(List<Texture2D> images)
        {
            //---------------------------

            foreach (var tex in images)
            {
                var image = new Image
                {
                    image = tex,
                    scaleMode = ScaleMode.ScaleToFit,
                    style =
                    {
                        width = 64,
                        height = 64,
                        marginTop = 5,
                        marginBottom = 5,
                    },
                };

                imageContainer.Add(image);
            }

            //---------------------------
        }
    }
}
