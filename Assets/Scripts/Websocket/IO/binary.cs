using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace scener.ws {

public class Binary : MonoBehaviour
{
    private int expectedImages = 0;
    private bool awaitingImages = false;
    private readonly List<Texture2D> receivedImages = new();

    void process_message(byte[] bytes){
        //---------------------------

        string message = System.Text.Encoding.UTF8.GetString(bytes);

        try{
            if (awaitingImages && int.TryParse(message, out int imageCount)){
                expectedImages = imageCount;
                Debug.Log($"Expecting {expectedImages} image(s)");
                return;
            }

            IncomingMessage parsed = JsonConvert.DeserializeObject<IncomingMessage>(message);

            if (parsed.action == Action.GenerateImage){
                awaitingImages = true;
                receivedImages.Clear();
                Debug.Log("Awaiting images...");
                //terminal.AddMessageToChat("<b>[Agent]</b>: " + parsed.message);
            }
        }
        catch{
            if (awaitingImages){
                Texture2D tex = new(2, 2);
                if (tex.LoadImage(bytes)){
                    receivedImages.Add(tex);
                    Debug.Log($"Received image {receivedImages.Count} from {expectedImages}");

                    if (receivedImages.Count == expectedImages){
                        awaitingImages = false;
                        expectedImages = 0;
                        //DisplayImages(receivedImages);
                    }
                }
                else{
                    Debug.LogWarning("Failed to load image");
                }
            }
            else{
                Debug.LogWarning("Received unhandled binary message");
            }
        }

        //---------------------------
    }

    public async System.Threading.Tasks.Task SendBytesMessage(byte[] message){
        //---------------------------
/*
        if (ws != null && ws.State == WebSocketState.Open){
            await ws.Send(message);
            Debug.Log("Sent binary message");
        }
        else{
            Debug.LogWarning("WebSocket is not connected.");
        }
*/
        //---------------------------
    }
}

}
