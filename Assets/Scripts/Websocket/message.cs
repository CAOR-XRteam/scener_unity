using System.IO;
using System.Threading.Tasks;
using Scener.Importer;
using Scener.Sdk;
using Ui.Terminal;
using UnityEngine;

namespace Scener.Ws
{
    public class WsMessage : MonoBehaviour
    {
        public async Task ProcessMessage(byte[] bytes)
        {
            //---------------------------

            var protoContent = Content.Parser.ParseFrom(bytes);
            IIncomingMessage message = IncomingMessageFactory.FromProto(protoContent);

            TerminalLabel terminal = FindFirstObjectByType<TerminalLabel>();
            if (terminal == null)
            {
                Debug.LogWarning("TerminalLabel not found in scene.");
                return;
            }

            switch (message)
            {
                case IncomingUnrelatedResponseMessage msg:
                    Debug.Log($"Received unrelated response: {msg}");
                    terminal.AddMessageToChat("<b>[Agent]</b>: " + msg.ResponseText);
                    break;
                case IncomingConvertSpeechMessage msg:
                    Debug.Log($"Received convert speech message: {msg}");
                    terminal.AddMessageToChat("<b>[You]</b>: " + msg.ResponseText);
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
                case IncomingGenerate3DObjectMessage msg:
                    Debug.Log($"Received generate 3D object message: {msg.ResponseText}");
                    terminal.AddMessageToChat("<b>[Agent]</b>: " + msg.ResponseText);
                    ModelInstantiator modelInstantiator =
                        FindFirstObjectByType<ModelInstantiator>();
                    if (modelInstantiator != null)
                    {
                        modelInstantiator.LoadAndPlaceModel(msg.Data[0]);
                    }
                    else
                    {
                        Debug.LogError("ModelInstantiator service not found in scene.");
                    }
                    break;
                case IncomingGenerate3DSceneMessage msg:
                    Debug.Log($"Received generate 3D scene message: {msg.ResponseText}");
                    terminal.AddMessageToChat("<b>[Agent]</b>: " + msg.ResponseText);

                    string resourcesPath = Path.Combine(Application.dataPath, "Resources");
                    foreach (var obj in msg.Data)
                    {
                        string fullPath = Path.Combine(resourcesPath, obj.Filename);

                        try
                        {
                            await File.WriteAllBytesAsync(fullPath, obj.Data);

                            Debug.Log($"Successfully saved GLB file to: {fullPath}");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError(
                                $"Failed to save GLB file '{obj.Filename}'. Error: {e.Message}"
                            );
                        }
                    }
                    UnityEditor.AssetDatabase.Refresh();
                    Debug.Log("AssetDatabase refreshed to import new models.");

                    SceneBuilder sceneBuilder = FindFirstObjectByType<SceneBuilder>();
                    if (sceneBuilder != null)
                    {
                        sceneBuilder.BuildSceneFromJSON(msg.Scene);
                    }
                    else
                    {
                        Debug.LogError("SceneBuilder not found in scene.");
                    }
                    break;
                case IncomingErrorMessage msg:
                    Debug.LogError($"Error message received: {msg.Status} {msg.ErrorText}");
                    terminal.AddMessageToChat(
                        $"<b>Error occured</b>: {msg.Status} {msg.ErrorText}"
                    );
                    break;
                default:
                    Debug.Log("Not implemented");
                    break;
            }

            //---------------------------
        }
    }
}
