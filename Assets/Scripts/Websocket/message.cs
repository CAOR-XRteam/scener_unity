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

            var protoContent = Content.Parser.ParseFrom(bytes);
            IIncomingMessage message = IncomingMessageFactory.FromProto(protoContent);

            TerminalLabel terminal = FindFirstObjectByType<TerminalLabel>();
            if (terminal == null)
            {
                Debug.LogWarning("TerminalLabel not found in scene.");
            }

            switch (message)
            {
                case IncomingUnrelatedResponseMessage msg:
                    Debug.Log($"Received unrelated response: {msg}");
                    terminal.AddMessageToChat("<b>[Agent]</b>: " + msg);
                    break;
                case IncomingConvertSpeechMessage msg:
                    Debug.Log($"Received convert speech message: {msg}");
                    terminal.AddMessageToChat("<b>[You]</b>: " + msg);
                    break;
                case IncomingGenerateImageMessage msg:
                    Debug.Log($"Received generate image message: {msg.ResponseText}");
                    terminal.AddMessageToChat("<b>[Agent]</b>: " + msg.ResponseText);
                    TerminalImage terminalImage = FindFirstObjectByType<TerminalImage>();
                    if (terminalImage != null)
                    {
                        terminalImage.LoadAndDisplayImages(msg.Data);
                    }
                    else
                    {
                        Debug.LogWarning("TerminalImage not found in scene.");
                    }
                    break;
                case IncomingErrorMessage msg:
                    Debug.LogError($"Error message received: {msg.Status} {msg.ErrorText}");
                    terminal.AddMessageToChat($"<b>[Agent]</b>: {msg.Status} {msg.ErrorText}");
                    break;
                default:
                    Debug.Log("Not implemented");
                    break;
            }

            //---------------------------
        }
    }
}
