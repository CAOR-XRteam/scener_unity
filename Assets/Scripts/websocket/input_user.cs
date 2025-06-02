using NativeWebSocket;
using Newtonsoft.Json;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class WebSocketUIChat : MonoBehaviour
{
    readonly WebSocket websocket;
    TextField chatInput;
    Button chatSendButton;
    Button micButton;
    private AudioClip recordedClip;
    private const int maxRecordDuration = 60;
    private const int standardFreq = 44100;
    private string microphoneDevice;
    private bool isRecording = false;

    void Start()
    {
        // Get UIDocument root
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Find the TextField
        chatInput = root.Q<TextField>("chat_input");
        chatSendButton = root.Q<Button>("chat_send_button");
        micButton = root.Q<Button>("chat_dictate_button");

        // Register callback
        chatInput.RegisterCallback<KeyDownEvent>(OnChatInputKeyDown);
        chatSendButton.clicked += OnSendClicked;

        microphoneDevice = Microphone.devices[0];
        micButton.clicked += ToggleRecording;
    }

    async void OnChatInputKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
        {
            string message = chatInput.value.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                await WebSocketClient.Instance.SendTextMessage(
                    JsonConvert.SerializeObject(
                        new OutgoingMessageMeta { command = Command.Chat, type = OutputType.Text }
                    )
                );
                await WebSocketClient.Instance.SendTextMessage(message);
                WebSocketClient.Instance.AddMessageToChat("<b>[You]</b>: " + message);
                chatInput.value = ""; // clear input
            }
        }
    }

    async void OnSendClicked()
    {
        string message = chatInput.value.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            await WebSocketClient.Instance.SendTextMessage(
                JsonConvert.SerializeObject(
                    new OutgoingMessageMeta { command = Command.Chat, type = OutputType.Text }
                )
            );
            await WebSocketClient.Instance.SendTextMessage(message);
            WebSocketClient.Instance.AddMessageToChat("<b>[You]</b>: " + message);
            chatInput.value = "";
        }
    }

    async void ToggleRecording()
    {
        if (!isRecording)
        {
            Debug.Log("Started recording...");
            Debug.Log($"Mic: {microphoneDevice}");
            recordedClip = Microphone.Start(
                microphoneDevice,
                false,
                maxRecordDuration,
                standardFreq
            );
            isRecording = true;
        }
        else
        {
            Debug.Log("Stopped recording...");
            Microphone.End(microphoneDevice);
            isRecording = false;

            byte[] audioData = VoiceInput.ConvertToWav(recordedClip);

            await WebSocketClient.Instance.SendTextMessage(
                JsonConvert.SerializeObject(
                    new OutgoingMessageMeta { command = Command.Chat, type = OutputType.Audio }
                )
            );
            await WebSocketClient.Instance.SendBytesMessage(audioData);
        }
    }

    void OnApplicationQuit()
    {
        WebSocketClient.Instance.OnApplicationQuit();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }
}
