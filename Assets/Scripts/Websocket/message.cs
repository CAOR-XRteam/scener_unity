using System.Threading.Tasks;
using Scener.Exporter;
using Scener.Importer;
using Scener.ModelInstantiator;
using Scener.Redis;
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

            SceneBuilder sceneBuilder = FindFirstObjectByType<SceneBuilder>();
            SceneSerializer sceneSerializer = FindFirstObjectByType<SceneSerializer>();

            switch (message)
            {
                case IncomingSessionStartMessage msg:
                    Debug.Log($"Received session start message: {msg.Text}");
                    WsClient.instance.clientId = msg.Text;
                    break;
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

                    if (sceneBuilder != null)
                    {
                        sceneBuilder.BuildSceneFromMessage(msg);
                    }
                    else
                    {
                        Debug.LogError("SceneBuilder not found in scene.");
                    }
                    if (sceneSerializer != null)
                    {
                        string scene_json = sceneSerializer.SerializeScene();
                        if (
                            await RedisClient.instance.WriteSceneAsync(
                                WsClient.instance.clientId,
                                scene_json
                            )
                        )
                        {
                            Debug.Log("Scene data written to Redis successfully.");
                        }
                        else
                        {
                            Debug.LogError("Failed to write scene data to Redis.");
                        }
                    }
                    else
                    {
                        Debug.LogError("SceneSerializer not found in scene.");
                    }
                    break;
                case IncomingModify3DSceneMessage msg:
                    Debug.Log($"Received generate 3D scene message: {msg.ResponseText}");
                    terminal.AddMessageToChat("<b>[Agent]</b>: " + msg.ResponseText);

                    if (sceneBuilder != null)
                    {
                        sceneBuilder.ModifySceneFromMessage(msg);
                    }
                    else
                    {
                        Debug.LogError("SceneBuilder not found in scene.");
                    }
                    if (sceneSerializer != null)
                    {
                        string scene_json = sceneSerializer.SerializeScene();
                        await RedisClient.instance.WriteSceneAsync(
                            WsClient.instance.clientId,
                            scene_json
                        );
                    }
                    else
                    {
                        Debug.LogError("SceneSerializer not found in scene.");
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
