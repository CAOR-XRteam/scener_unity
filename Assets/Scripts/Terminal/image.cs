using System.Collections.Generic;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;
using scener.ui;


namespace scener.ws {

public class WebSocketClient : MonoBehaviour
{
    public VisualElement imageContainer;
  
    void DisplayImages(List<Texture2D> images){
        //---------------------------

        foreach (var tex in images){
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
