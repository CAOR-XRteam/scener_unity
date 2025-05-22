using NativeWebSocket;
using UnityEngine;
using UnityEngine.UIElements;

public class WebSocketUIChat : MonoBehaviour
{
    WebSocket websocket;
    TextField chatInput;
    Button chatSendButton;

    void Start()
    {
        // Get UIDocument root
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Find the TextField
        chatInput = root.Q<TextField>("chat_input");
        chatSendButton = root.Q<Button>("chat_send_button");

        // Register callback
        chatInput.RegisterCallback<KeyDownEvent>(OnChatInputKeyDown);
        chatSendButton.clicked += OnSendClicked;
    }

    async void OnChatInputKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
        {
            string message = chatInput.value.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                await WebSocketClient.Instance.SendUserMessage(message);
                WebSocketClient.Instance.AddMessageToChat("[You]: " + message);
                chatInput.value = ""; // clear input
            }
        }
    }

    async void OnSendClicked()
    {
        string message = chatInput.value.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            await WebSocketClient.Instance.SendUserMessage(message);
            WebSocketClient.Instance.AddMessageToChat("[You]: " + message);
            chatInput.value = "";
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
