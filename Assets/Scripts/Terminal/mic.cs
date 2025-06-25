using scener.input;
using scener.ws;
using UnityEngine;
using UnityEngine.UIElements;
using static scener.input.VoiceInput;

namespace ui.terminal
{
    public class TerminalMic : MonoBehaviour
    {
        private Button button_mic;
        private AudioClip record_clip;
        private const int max_record_duration = 60;
        private const int standard_freq = 44100;
        private bool is_recording = false;
        public string mic_device;

        void Start()
        {
            //---------------------------

            //Search UI elements
            var root = FindFirstObjectByType<UIDocument>().rootVisualElement;
            button_mic = root.Q<Button>("button_mic");
            mic_device = Microphone.devices[0];
            button_mic.clicked += CallbackButton;

            //---------------------------
        }

        private void CallbackButton()
        {
            //---------------------------

            if (!is_recording)
            {
                RunRecording();
            }
            else
            {
                StopRecording();
            }

            //---------------------------
        }

        private void RunRecording()
        {
            //---------------------------

            Debug.Log("Started recording...");
            Debug.Log($"Mic: {mic_device}");

            record_clip = Microphone.Start(mic_device, false, max_record_duration, standard_freq);

            is_recording = true;

            //---------------------------
        }

        private async void StopRecording()
        {
            //---------------------------

            Debug.Log("Stopped recording...");
            int pos = Microphone.GetPosition(mic_device);
            Microphone.End(mic_device);
            is_recording = false;

            byte[] data = ConvertToWav(pos, record_clip);

            await WsClient.instance.SendMessage(type: OutputType.Audio, text: "", bytes: data);

            //---------------------------
        }
    }
}
