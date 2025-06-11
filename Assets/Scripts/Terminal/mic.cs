using UnityEngine;
using UnityEngine.UIElements;


namespace ui.terminal {

public class TerminalMic : MonoBehaviour
{
    private Button button_mic;
    private AudioClip record_clip;
    private const int max_record_duration = 60;
    private const int standard_freq = 44100;
    private bool is_recording = false;
    public string mic_device;

    void Start(){
        //---------------------------

        //Search UI elements
        var root = GetComponent<UIDocument>().rootVisualElement;
        button_mic = root.Q("terminal").Q<VisualElement>("box_input").Q<Button>("button_mic");
        mic_device = Microphone.devices[0];
        button_mic.clicked += callback_button;

        //---------------------------
    }

    void callback_button(){
        //---------------------------

        if (!is_recording){
            run_recording();
        }
        else{
            stop_recording();
        }

        //---------------------------
    }

    void run_recording(){
        //---------------------------

        Debug.Log("Started recording...");
        Debug.Log($"Mic: {mic_device}");

        record_clip = Microphone.Start(
            mic_device,
            false,
            max_record_duration,
            standard_freq
        );

        is_recording = true;

        //---------------------------
    }
    void stop_recording(){
        //---------------------------

        Debug.Log("Stopped recording...");
        int pos = Microphone.GetPosition(mic_device);
        Microphone.End(mic_device);
        is_recording = false;

        //byte[] data = VoiceInput.ConvertToWav(pos, record_clip);

        /*await WebSocketClient.instance.SendTextMessage(
            JsonConvert.SerializeObject(
                new OutgoingMessageMeta { command = Command.Chat, type = OutputType.Audio }
            )
        );
        await WebSocketClient.instance.SendBytesMessage(data);*/

        //---------------------------
    }
}

}
