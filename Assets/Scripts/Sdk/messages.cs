using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

public class IncomingMessage
{
    public Status status;

    public int code;

    [JsonConverter(typeof(StringEnumConverter))]
    public Action action;

    public string message;
}

public enum OutputType
{
    [EnumMember(Value = "text")]
    Text,

    [EnumMember(Value = "audio")]
    Audio,

    [EnumMember(Value = "gesture")]
    Gesture,
}

public enum Command
{
    [EnumMember(Value = "chat")]
    Chat,
}

public class OutgoingMessageMeta
{
    [JsonConverter(typeof(StringEnumConverter))]
    public Command command;

    [JsonConverter(typeof(StringEnumConverter))]
    public OutputType type;
}
