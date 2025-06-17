using System.Collections.Generic;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

namespace scener.ws
{
    public class WebSocketClient : MonoBehaviour
    {
        public static WebSocketClient Instance { get; private set; }

        WebSocket websocket;

        ScrollView chatScrollView;
        private Label chatTextLabel;
        readonly List<string> messageHistory = new();

        private int expectedImages = 0;
        private bool awaitingImages = false;
        private readonly List<Texture2D> receivedImages = new();
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

                chatTextLabel = new Label();
                chatTextLabel.style.whiteSpace = WhiteSpace.Normal;
                chatTextLabel.style.flexGrow = 1;
                chatTextLabel.selection.isSelectable = true;
                chatTextLabel.pickingMode = PickingMode.Position;
                chatTextLabel.enableRichText = true;
                Font myFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                chatTextLabel.style.unityFont = myFont;
                chatTextLabel.AddToClassList("chat-message");

                chatScrollView.Add(chatTextLabel);
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

                    IncomingMessage parsed = JsonConvert.DeserializeObject<IncomingMessage>(
                        message
                    );

                    if (parsed.action == Action.GenerateImage)
                    {
                        awaitingImages = true;
                        receivedImages.Clear();
                        Debug.Log("Awaiting images...");
                        AddMessageToChat("<b>[Agent]</b>: " + parsed.message);
                    }
                    else if (parsed.action == Action.ConvertedSpeech)
                    {
                        AddMessageToChat("<b>[You]</b>: " + parsed.message);
                        Debug.Log($"Received converted audio message: {parsed.message}");
                    }
                    else if (parsed.action == Action.ThinkingProcess)
                    {
                        Debug.Log($"Thinking process: {parsed.message}");
                        AddMessageToChat("<b>[Agent]</b>: " + parsed.message);
                    }
                    else
                    {
                        Debug.Log($"Received message: {parsed.message}");
                        AddMessageToChat("<b>[Agent]</b>: " + parsed.message);
                    }
                }
                catch
                {
                    if (awaitingImages)
                    {
                        Texture2D tex = new(2, 2);
                        if (tex.LoadImage(bytes))
                        {
                            receivedImages.Add(tex);
                            Debug.Log(
                                $"Received image {receivedImages.Count} from {expectedImages}"
                            );

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

        public async System.Threading.Tasks.Task SendTextMessage(string message)
        {
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                await websocket.SendText(message);
                Debug.Log($"Sent message: {message}");
            }
            else
            {
                Debug.LogWarning("WebSocket is not connected.");
            }
        }

        public async System.Threading.Tasks.Task SendBytesMessage(byte[] message)
        {
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                await websocket.Send(message);
                Debug.Log("Sent binary message");
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

            chatTextLabel.text += msg + "\n\n";
            chatScrollView.Add(chatTextLabel);
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
                        width = 64,
                        height = 64,
                        marginTop = 5,
                        marginBottom = 5,
                    },
                };

                imageContainer.Add(image);
            }
        }
    }
}
