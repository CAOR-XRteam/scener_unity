using UnityEngine;
using NativeWebSocket;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class WebSocketClient : MonoBehaviour
{
    public static WebSocketClient Instance { get; private set; }

    WebSocket websocket;
    List<string> messageHistory = new List<string>();

    // Reference to UI TextField to update
    TextField chatHistoryField;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        websocket = new WebSocket("ws://localhost:8765");

        // Find UIDocument and TextField in scene (assuming one exists)
        var uiDoc = FindObjectOfType<UIDocument>();
        if (uiDoc != null)
        {
            chatHistoryField = uiDoc.rootVisualElement.Q<TextField>("chat_history");
            if (chatHistoryField != null)
            {
                chatHistoryField.isReadOnly = true;
                chatHistoryField.multiline = true;
            }
        }

        websocket.OnOpen += async () =>
        {
            Debug.Log("Connection opened!");
            // await websocket.SendText("Hello from Unity client!");
        };
        websocket.OnError += e => Debug.Log("Error: " + e);
        websocket.OnClose += e => Debug.Log("Connection closed!");
        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Received: " + message);
            messageHistory.Add(message);
            UpdateChatHistoryUI();
        };

        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    async void OnApplicationQuit()
    {
        await websocket.Close();
    }

    void UpdateChatHistoryUI()
    {
        if (chatHistoryField != null)
        {
            // Join messages with new lines
            chatHistoryField.value = string.Join("\n", messageHistory);
        }
    }

    public List<string> GetMessageHistory()
    {
        return new List<string>(messageHistory);
    }
    
    public async System.Threading.Tasks.Task SendMessage(string message) {
    if (websocket != null && websocket.State == WebSocketState.Open) {
        await websocket.SendText(message);
    } else {
        Debug.LogWarning("WebSocket is not connected.");
    }
}
}

