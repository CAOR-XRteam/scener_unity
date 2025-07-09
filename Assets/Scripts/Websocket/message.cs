using System;
using System.IO;
using System.Threading.Tasks;
using Scener.Exporter;
using Scener.Importer;
using Scener.ModelInstantiator;
using Scener.Sdk;
using Ui.Terminal;
using Unity.Burst.Intrinsics;
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

            string resourcesPath = Path.Combine(Application.dataPath, "Resources");
            SceneBuilder sceneBuilder = FindFirstObjectByType<SceneBuilder>();

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
                    ObjectBuilder modelInstantiator = FindFirstObjectByType<ObjectBuilder>();
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

                    foreach (var obj in msg.Data)
                    {
                        string fullPath = Path.Combine(resourcesPath, obj.Filename);

                        try
                        {
                            await File.WriteAllBytesAsync(fullPath, obj.Data);

                            Debug.Log($"Successfully saved GLB file to: {fullPath}");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(
                                $"Failed to save GLB file '{obj.Filename}'. Error: {e.Message}"
                            );
                        }
                    }
                    UnityEditor.AssetDatabase.Refresh();
                    Debug.Log("AssetDatabase refreshed to import new models.");

                    if (sceneBuilder != null)
                    {
                        sceneBuilder.BuildSceneFromJSON(msg.Scene);
                    }
                    else
                    {
                        Debug.LogError("SceneBuilder not found in scene.");
                    }
                    break;
                // case IncomingRequestContextMessage:
                //     SceneSerializer sceneSerializer = FindFirstObjectByType<SceneSerializer>();
                //     if (sceneSerializer != null)
                //     {
                //         try
                //         {
                //             var json_scene = sceneSerializer.SerializeScene();
                //             var contextMessage = new OutgoingRequestContextMessage(json_scene);
                //             await WsClient.instance.SendMessage(contextMessage);
                //         }
                //         catch (Exception ex)
                //         {
                //             Debug.LogError("Failed to serialize scene: " + ex.Message);
                //             terminal.AddMessageToChat(
                //                 $"<b>Error occured</b>: {500} {"No scene found"}"
                //             );
                //             break;
                //         }
                //     }
                //     else
                //     {
                //         Debug.LogError("SceneSerializer not found in scene.");
                //         terminal.AddMessageToChat(
                //             $"<b>Error occured</b>: {500} {"No scene found"}"
                //         );
                //         break;
                //     }
                //     break;
                // case IncomingModify3DSceneMessage msg:
                //     Debug.Log($"Received generate 3D scene message: {msg.ResponseText}");
                //     terminal.AddMessageToChat("<b>[Agent]</b>: " + msg.ResponseText);

                //     foreach (var obj in msg.Data)
                //     {
                //         string fullPath = Path.Combine(resourcesPath, obj.Filename);

                //         try
                //         {
                //             await File.WriteAllBytesAsync(fullPath, obj.Data);

                //             Debug.Log($"Successfully saved GLB file to: {fullPath}");
                //         }
                //         catch (Exception e)
                //         {
                //             Debug.LogError(
                //                 $"Failed to save GLB file '{obj.Filename}'. Error: {e.Message}"
                //             );
                //         }
                //     }
                //     UnityEditor.AssetDatabase.Refresh();
                //     Debug.Log("AssetDatabase refreshed to import new models.");

                //     if (sceneBuilder != null)
                //     {
                //         sceneBuilder.ModifySceneFromJSON(msg.Scene);
                //     }
                //     else
                //     {
                //         Debug.LogError("SceneBuilder not found in scene.");
                //     }
                //     break;
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
