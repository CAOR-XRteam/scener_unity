using System.Collections.Generic;
using System.Runtime.Serialization;
using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.UIElements;

public class WebSocketClient : MonoBehaviour
{
    public enum Action
    {
        [EnumMember(Value = "thinking_process")]
        ThinkingProcess,

        [EnumMember(Value = "agent_response")]
        AgentResponse,

        [EnumMember(Value = "image_generation")]
        GenerateImage,
    }

    public enum Status
    {
        stream,
        error,
    };

    public class IncomingMessage
    {
        public Status status;

        public int code;

        [JsonConverter(typeof(StringEnumConverter))]
        public Action action;

        public string message;
    }

    public static WebSocketClient Instance { get; private set; }

    WebSocket websocket;

    ScrollView chatScrollView;
    List<string> messageHistory = new List<string>();

    private int expectedImages = 0;
    private bool awaitingImages = false;
    private List<Texture2D> receivedImages = new();
    public VisualElement imageContainer;

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
            imageContainer = root.Q<VisualElement>("image_container");
        }

        websocket.OnOpen += () => Debug.Log("Connection opened!");
        websocket.OnError += e => Debug.Log("Error: " + e);
        websocket.OnClose += e => Debug.Log("Connection closed!");
        websocket.OnMessage += (bytes) =>
        {
            string message = null;
            try
            {
                message = System.Text.Encoding.UTF8.GetString(bytes);

                if (awaitingImages && int.TryParse(message, out int imageCount))
                {
                    expectedImages = imageCount;
                    Debug.Log($"Expecting {expectedImages} image(s)");
                    return;
                }

                IncomingMessage parsed = JsonConvert.DeserializeObject<IncomingMessage>(message);

                if (parsed.action == Action.GenerateImage)
                {
                    awaitingImages = true;
                    receivedImages.Clear();
                    Debug.Log("Awaiting images...");
                    AddMessageToChat("[Agent]: " + parsed.message);
                }
                else
                {
                    Debug.Log($"Received message: {parsed.message}");
                    AddMessageToChat("[Agent]: " + parsed.message);
                }
            }
            catch
            {
                if (awaitingImages)
                {
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(bytes))
                    {
                        receivedImages.Add(tex);
                        Debug.Log($"Received image {receivedImages.Count} from {expectedImages}");

                        if (receivedImages.Count == expectedImages)
                        {
                            awaitingImages = false;
                            expectedImages = 0;
                            DisplayImages(receivedImages);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Failed to load image");
                    }
                }
                else
                {
                    Debug.LogWarning("Received unhandled binary message");
                }
            }
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

    void DisplayImages(List<Texture2D> images)
    {
        foreach (var tex in images)
        {
            var image = new Image
            {
                image = tex,
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = Length.Percent(100),
                    height = tex.height,
                    marginTop = 5,
                    marginBottom = 5,
                },
            };

            imageContainer.Add(image);
        }
    }
}
