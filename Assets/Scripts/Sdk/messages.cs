using System.Runtime.Serialization;

namespace Sdk.Messages
{
    public enum Action
    {
        [EnumMember(Value = "thinking_process")]
        ThinkingProcess,

        [EnumMember(Value = "agent_response")]
        AgentResponse,

        [EnumMember(Value = "image_generation")]
        GenerateImage,

        [EnumMember(Value = "converted_speech")]
        ConvertedSpeech,
    }

    public enum Status
    {
        stream,
        error,
    };

    public enum OutcomingMessageType
    {
        [EnumMember(Value = "text")]
        Text,

        [EnumMember(Value = "audio")]
        Audio,

        [EnumMember(Value = "gesture")]
        Gesture,
    }

    public enum IncomingMessageType
    {
        [EnumMember(Value = "thinking_process")]
        ThinkingProcess,

        [EnumMember(Value = "unrelated_response")]
        UnrelatedResponse,

        [EnumMember(Value = "image_generation")]
        GenerateImage,

        [EnumMember(Value = "3d_object_generation")]
        Generate3DObject,

        [EnumMember(Value = "3d_scene_generation")]
        Generate3DScene,

        [EnumMember(Value = "converted_speech")]
        ConvertedSpeech,
    }

    public enum Command
    {
        [EnumMember(Value = "chat")]
        Chat,
    }
}
