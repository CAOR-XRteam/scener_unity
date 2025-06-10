using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;


namespace ui.terminal {

public class TerminalImage : MonoBehaviour
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
