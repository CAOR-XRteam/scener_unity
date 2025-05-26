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
