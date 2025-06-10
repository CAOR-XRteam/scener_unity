using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;


namespace ui.terminal {

public class TerminalInput : MonoBehaviour
{
    private TextField field_text;
    private Button button_send;
    private Button button_mic;

    private AudioClip recordedClip;
    private const int maxRecordDuration = 60;
    private const int standardFreq = 44100;
    public string mic_device;
    //private bool is_recording = false;

    void Start(){
        //---------------------------

        //Search UI elements
        var uiDoc = FindFirstObjectByType<UIDocument>();
        if (uiDoc == null) {
            Debug.LogError("UIDocument not found!");
            return;
        }
        var root = uiDoc.rootVisualElement;

        field_text = root.Q("terminal").Q<VisualElement>("box_input").Q<TextField>("field_text");
        button_send = root.Q("terminal").Q<VisualElement>("box_input").Q<Button>("button_send");
        button_mic = root.Q("terminal").Q<VisualElement>("box_input").Q<Button>("button_mic");


        // Register callback
        field_text.RegisterCallback<KeyUpEvent>(callback_field_text);
        button_send.clicked += callback_button_send;

        mic_device = Microphone.devices[0];
        button_mic.clicked += callback_button_mic;

        //---------------------------
    }

    void callback_field_text(KeyUpEvent evt){
        // Called when text field user input
        //---------------------------

        Logger.say("callback");

        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter){
            string message = field_text.value.Trim();
            if (string.IsNullOrEmpty(message)) return;

            TerminalLabel terminal = FindObjectOfType<TerminalLabel>();
            terminal.AddMessageToChat("<b>[You]</b>: " + message);

            // Clear input in next frame to avoid caret issues
            evt.PreventDefault(); // Prevent default Return behavior
            field_text.schedule.Execute(() => {
                field_text.value = "";
            }).ExecuteLater(1);
        }
        
/*
        if (evt.keyCode == KeyCode.KeypadEnter){
            string message = field_text.value.Trim();
            if (string.IsNullOrEmpty(message)) return;

            terminal.AddMessageToChat("<b>[You]</b>: " + message);
            field_text.value = ""; // clear input
        }
*/
        //---------------------------
    }

    void callback_button_send(){
        //---------------------------

        string message = field_text.value.Trim();
/*
        if (!string.IsNullOrEmpty(message)){
            await WebSocketClient.instance.SendTextMessage(
                JsonConvert.SerializeObject(
                    new OutgoingMessageMeta { command = Command.Chat, type = OutputType.Text }
                )
            );
            await WebSocketClient.instance.SendTextMessage(message);
            //terminal.AddMessageToChat("<b>[You]</b>: " + message);
            field_text.value = "";
        }
*/
        //---------------------------
    }

    void callback_button_mic(){
        //---------------------------
/*
        if (!is_recording){
            Debug.Log("Started recording...");
            Debug.Log($"Mic: {mic_device}");

            recordedClip = Microphone.Start(
                mic_device,
                false,
                maxRecordDuration,
                standardFreq
            );

            is_recording = true;
        }
        else{
            Debug.Log("Stopped recording...");
            int pos = Microphone.GetPosition(mic_device);
            Microphone.End(mic_device);
            is_recording = false;

            byte[] audioData = VoiceInput.ConvertToWav(pos, recordedClip);

            await WebSocketClient.instance.SendTextMessage(
                JsonConvert.SerializeObject(
                    new OutgoingMessageMeta { command = Command.Chat, type = OutputType.Audio }
                )
            );
            await WebSocketClient.instance.SendBytesMessage(audioData);
        }
*/
        //---------------------------
    }
}

}
