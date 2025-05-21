using System.Collections.Generic;
using NativeWebSocket;
using UnityEngine;
using UnityEngine.UIElements;

public class WebSocketClient : MonoBehaviour
{
    public static WebSocketClient Instance { get; private set; }

    WebSocket websocket;

    ScrollView chatScrollView;
    List<string> messageHistory = new List<string>();

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
        var uiDoc = FindFirstObjectByType<UIDocument>();
        if (uiDoc != null)
        {
            var root = uiDoc.rootVisualElement;
            chatScrollView = root.Q<ScrollView>("chat_scrollview");
        }

        websocket.OnOpen += () => Debug.Log("Connection opened!");
        websocket.OnError += e => Debug.Log("Error: " + e);
        websocket.OnClose += e => Debug.Log("Connection closed!");
        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Received: " + message);
            messageHistory.Add(message);
            AddMessageToChat("[Agent]: " + message);
        };

        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    public async void OnApplicationQuit()
    {
        await websocket.Close();
    }

    public List<string> GetMessageHistory()
    {
        return new List<string>(messageHistory);
    }

    public async System.Threading.Tasks.Task SendUserMessage(string message)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(message);
        }
        else
        {
            Debug.LogWarning("WebSocket is not connected.");
        }
    }

    public void AddMessageToChat(string msg)
    {
        if (chatScrollView == null)
            return;

        var label = new Label(msg);
        label.AddToClassList("chat-message");
        chatScrollView.Add(label);
    }
}
