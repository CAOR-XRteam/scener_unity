using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Newtonsoft.Json;
using ui.terminal;


namespace scener.ws {

public class WsMessage : MonoBehaviour
{
    public void process_message(byte[] bytes){
        //---------------------------

        var msg = Content.Parser.ParseFrom(bytes);
        Debug.Log($"Response: {msg.Text}\n");


        TerminalLabel terminal = FindFirstObjectByType<TerminalLabel>();
        if (terminal == null) { Debug.LogWarning("TerminalLabel not found in scene."); }
        terminal.AddMessageToChat("<b>[Agent]</b>: " + msg.Text);

        //---------------------------
    }
}

}
