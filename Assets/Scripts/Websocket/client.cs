using System.Threading.Tasks;
using Google.Protobuf;
using NativeWebSocket;
using Scener.Sdk;
using UnityEngine;

namespace Scener.Ws
{
    public class WsClient : MonoBehaviour
    {
        //Parameters
        public static WsClient instance { get; private set; } // Singleton instance
        private WebSocket ws;
        public string clientId { get; set; }

        // Automatic functions
        void Awake()
        {
            // Permet le singleton
            //---------------------------

            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            //---------------------------
        }

        void Start()
        {
            // Start websocket services without waiting
            //---------------------------

            _ = InitAsync();

            //---------------------------
        }

        void Update()
        {
            //Update stuff essential for websocket not on webgl
            //---------------------------

#if !UNITY_WEBGL || UNITY_EDITOR
            ws?.DispatchMessageQueue();
#endif

            //---------------------------
        }

        void OnApplicationQuit()
        {
            //---------------------------

            ws.Close();

            //---------------------------
        }

        // Subfunctions
        private async Task InitAsync()
        {
            //---------------------------

            WsMessage ws_message = FindFirstObjectByType<WsMessage>();
            if (ws_message == null)
            {
                Debug.LogWarning("WsMessage not found in scene.");
            }

            ws = new WebSocket("ws://localhost:8765");
            ws.OnOpen += () => Debug.Log("Connection opened!");
            ws.OnError += e => Debug.Log("Error: " + e);
            ws.OnClose += e => Debug.Log("Connection closed!");
            ws.OnMessage += async (bytes) => await ws_message.ProcessMessage(bytes);

            await ws.Connect();

            //---------------------------
        }

        public async Task SendMessage(IOutgoingMessage outgoingMessage)
        {
            //---------------------------

            if (ws != null && ws.State == WebSocketState.Open)
            {
                var protoMessage = outgoingMessage.ToProto();
                //Binarize and send it
                byte[] data = protoMessage.ToByteArray();
                await ws.Send(data);
                Debug.Log($"Sent message: {protoMessage.Text}");
            }
            else
            {
                Debug.LogWarning("WebSocket is not connected.");
            }

            //---------------------------
        }
    }
}
