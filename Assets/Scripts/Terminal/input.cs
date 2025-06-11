using UnityEngine;
using UnityEngine.UIElements;


namespace ui.terminal {

public class TerminalInput : MonoBehaviour
{
    private TextField field_text;
    private Button button_send;

    void Start(){
        //---------------------------

        //Search UI elements
        var root = GetComponent<UIDocument>().rootVisualElement;
        field_text = root.Q("terminal").Q<VisualElement>("box_input").Q<TextField>("field_text");
        button_send = root.Q("terminal").Q<VisualElement>("box_input").Q<Button>("button_send");

        // Register callback
        field_text.RegisterCallback<KeyUpEvent>(callback_field_text);
        button_send.clicked += callback_button_send;

        //---------------------------
    }

    void callback_field_text(KeyUpEvent evt){
        // Called when text field user input
        //---------------------------

        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter){
            string message = field_text.value.Trim();
            if (string.IsNullOrEmpty(message)) return;

            TerminalLabel terminal = FindFirstObjectByType<TerminalLabel>();
            terminal.AddMessageToChat("<b>[You]</b>: " + message);

            evt.StopPropagation();
            clear_text_field();
        }

        //---------------------------
    }

    void callback_button_send(){
        string message = field_text.value.Trim();
        //---------------------------

        if (!string.IsNullOrEmpty(message)){
            /*await WebSocketClient.instance.SendTextMessage(
                JsonConvert.SerializeObject(
                    new OutgoingMessageMeta { command = Command.Chat, type = OutputType.Text }
                )
            );
            await WebSocketClient.instance.SendTextMessage(message);*/

            TerminalLabel terminal = FindFirstObjectByType<TerminalLabel>();
            terminal.AddMessageToChat("<b>[You]</b>: " + message);
            clear_text_field();
        }

        //---------------------------
    }

    public void clear_text_field(){
        //---------------------------

        // Clear input in next frame to avoid caret issues
        field_text.schedule.Execute(() => {
            field_text.value = "";
        }).ExecuteLater(1);

        //---------------------------
    }
}

}
