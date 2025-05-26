using System.Collections;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SdkTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestDeserializeIncomingMessage()
    {
        string input1 =
            @"
        {
            ""status"": ""stream"",
            ""code"": 200,
            ""action"": ""thinking_process"",
            ""message"": ""OK""
        }";

        string input2 =
            @"
        {
            ""status"": ""stream"",
            ""code"": 200,
            ""action"": ""agent_response"",
            ""message"": ""OK""
        }";

        string input3 =
            @"
        {
            ""status"": ""stream"",
            ""code"": 200,
            ""action"": ""image_generation"",
            ""message"": ""OK""
        }";

        string input4 =
            @"
        {
            ""status"": ""error"",
            ""code"": 200,
            ""action"": ""image_generation"",
            ""message"": ""OK""
        }";

        var res1 = JsonConvert.DeserializeObject<IncomingMessage>(input1);
        var res2 = JsonConvert.DeserializeObject<IncomingMessage>(input2);
        var res3 = JsonConvert.DeserializeObject<IncomingMessage>(input3);
        var res4 = JsonConvert.DeserializeObject<IncomingMessage>(input4);

        Assert.AreEqual(Status.stream, res1.status);
        Assert.AreEqual(200, res1.code);
        Assert.AreEqual(Action.ThinkingProcess, res1.action);
        Assert.AreEqual("OK", res1.message);

        Assert.AreEqual(Status.stream, res2.status);
        Assert.AreEqual(200, res2.code);
        Assert.AreEqual(Action.AgentResponse, res2.action);
        Assert.AreEqual("OK", res1.message);

        Assert.AreEqual(Status.stream, res3.status);
        Assert.AreEqual(200, res3.code);
        Assert.AreEqual(Action.GenerateImage, res3.action);
        Assert.AreEqual("OK", res3.message);

        Assert.AreEqual(Status.error, res4.status);
        Assert.AreEqual(200, res4.code);
        Assert.AreEqual(Action.GenerateImage, res4.action);
        Assert.AreEqual("OK", res4.message);
    }
}
