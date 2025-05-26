using System.Collections;
// using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SdkTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestDeserializeIncomingMessage()
    {
        string json =
            @"
        {
            ""status"": ""stream"",
            ""code"": 200,
            ""action"": ""thinking_process"",
            ""message"": ""OK""
        }";

        // var message = JsonConvert.DeserializeObject<IncomingMessage>(json);

        // Assert.AreEqual(Status.stream, message.status);
        // Assert.AreEqual(200, message.code);
        // Assert.AreEqual(Action.ThinkingProcess, message.action);
        // Assert.AreEqual("OK", message.message);
    }
}
