using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

namespace scener.ws
{
    public class WsClient : MonoBehaviour
    {
        //Parameters
        public static WsClient instance { get; private set; } //Singleton instance
        private NativeWebSocket.WebSocket ws;

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

            ws = new NativeWebSocket.WebSocket("ws://localhost:8765");
            ws.OnOpen += () => Debug.Log("Connection opened!");
            ws.OnError += e => Debug.Log("Error: " + e);
            ws.OnClose += e => Debug.Log("Connection closed!");
            ws.OnMessage += (bytes) => ws_message.process_message(bytes);

            await ws.Connect();

            //---------------------------
        }

        void onMessage(byte[] bytes)
        {
            //---------------------------

            var msg = Content.Parser.ParseFrom(bytes);
            Debug.Log($"Response: {msg.Text}\n");

            //---------------------------
        }

        public async System.Threading.Tasks.Task SendMessage(
            string type = "chat",
            string text = "",
            byte[] bytes = null
        )
        {
            //---------------------------

            if (ws != null && ws.State == WebSocketState.Open)
            {
                // Fill protobuf message
                var msg = new Content();
                msg.Type = type;
                msg.Text = text;
                msg.Data = ByteString.CopyFrom(bytes ?? new byte[0]);
                msg.Status = 200;

                //Binarize and send it
                byte[] data = msg.ToByteArray();
                await ws.Send(data);
                Debug.Log($"Sent message: {msg.Text}");
            }
            else
            {
                Debug.LogWarning("WebSocket is not connected.");
            }

            //---------------------------
        }
    }
}
