using UnityEngine;
using UnityEngine.UIElements;
using NativeWebSocket;

public class WebSocketUIChat : MonoBehaviour {
    WebSocket websocket;
    TextField chatInput;

    async void Start() {
        // Setup WebSocket
        websocket = new WebSocket("ws://localhost:8765");
        await websocket.Connect();

        // Get UIDocument root
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Find the TextField
        chatInput = root.Q<TextField>("chat_input");

        // Register callback
        chatInput.RegisterCallback<KeyDownEvent>(OnChatInputKeyDown);
    }

    async void OnChatInputKeyDown(KeyDownEvent evt) {
        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) {
            string message = chatInput.value.Trim();
            if (!string.IsNullOrEmpty(message)) {
                await websocket.SendText(message);
                chatInput.value = ""; // clear input
            }
        }
    }

    async void OnApplicationQuit() {
        await websocket.Close();
    }

    void Update() {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }
}

