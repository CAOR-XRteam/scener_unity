using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;


namespace scener.ws {

public class Text : MonoBehaviour
{
    public void process_message(string message){
        //---------------------------

        IncomingMessage parsed = JsonConvert.DeserializeObject<IncomingMessage>(message);
        TerminalLabel terminal = FindFirstObjectByType<TerminalLabel>();

        if (parsed.action == Action.ConvertedSpeech){
            terminal.AddMessageToChat("<b>[You]</b>: " + parsed.message);
            Debug.Log($"Received converted audio message: {parsed.message}");
        }
        else if (parsed.action == Action.ThinkingProcess){
            Debug.Log($"Thinking process: {parsed.message}");
            terminal.AddMessageToChat("<b>[Agent]</b>: " + parsed.message);
        }
        else{
            Debug.Log($"Received message: {parsed.message}");
            terminal.AddMessageToChat("<b>[Agent]</b>: " + parsed.message);
        }

        //---------------------------
    }

    public async System.Threading.Tasks.Task SendTextMessage(string message){
        //---------------------------

        if (ws != null && ws.State == WebSocketState.Open){
            await ws.SendText(message);
            Debug.Log($"Sent message: {message}");
        }
        else{
            Debug.LogWarning("WebSocket is not connected.");
        }

        //---------------------------
    }
}

}
