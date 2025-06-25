using System;
using System.Collections.Generic;
using Scener.Sdk;
using ui.terminal;
using UnityEngine;

namespace Scener.Ws
{
    public class WsMessage : MonoBehaviour
    {
        public void ProcessMessage(byte[] bytes)
        {
            //---------------------------

            var msg = Content.Parser.ParseFrom(bytes);
            Debug.Log($"Response: {msg.Text}\n");

            TerminalLabel terminal = FindFirstObjectByType<TerminalLabel>();
            if (terminal == null)
            {
                Debug.LogWarning("TerminalLabel not found in scene.");
            }
            switch (Enum.Parse<IncomingMessageType>(msg.Type))
            {
                case IncomingMessageType.UnrelatedResponse:
                    terminal.AddMessageToChat("<b>[Agent]</b>: " + msg.Text);
                    break;

                case IncomingMessageType.GenerateImage:
                    terminal.AddMessageToChat("<b>[Agent]</b>: " + msg.Text);
                    TerminalImage terminalImage = FindFirstObjectByType<TerminalImage>();
                    if (terminalImage != null)
                    {
                        terminalImage.LoadAndDisplayImages(
                            new List<byte[]> { msg.Data.ToByteArray() }
                        );
                    }
                    else
                    {
                        Debug.LogWarning("TerminalImage not found in scene.");
                    }
                    break;

                case IncomingMessageType.ConverteSpeech:
                    terminal.AddMessageToChat("<b>[You]</b>: " + msg.Text);
                    break;

                case IncomingMessageType.Generate3DObject:
                    terminal.AddMessageToChat("<b>[Agent]</b>: " + msg.Text);
                    // Handle 3D object generation
                    break;
                case IncomingMessageType.Generate3DScene:
                    terminal.AddMessageToChat("<b>[Agent]</b>: " + msg.Text);
                    // Handle 3D scene generation
                    break;
                default:
                    Debug.LogWarning($"Unknown message type: {msg.Type}");
                    break;
            }

            //---------------------------
        }
    }
}
