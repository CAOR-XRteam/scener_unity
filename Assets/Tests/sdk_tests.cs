using Newtonsoft.Json;
using NUnit.Framework;

//using scener.ws;

// TODO: unit tests

public class SdkTests
{
    // A Test behaves as an ordinary method
    /*   [Test]
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
               ""status"": ""stream"",
               ""code"": 200,
               ""action"": ""converted_speech"",
               ""message"": ""OK""
           }";
   
           string input5 =
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
           var res5 = JsonConvert.DeserializeObject<IncomingMessage>(input5);
   
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
   
           Assert.AreEqual(Status.stream, res4.status);
           Assert.AreEqual(200, res4.code);
           Assert.AreEqual(Action.ConvertedSpeech, res4.action);
           Assert.AreEqual("OK", res4.message);
   
           Assert.AreEqual(Status.error, res5.status);
           Assert.AreEqual(200, res5.code);
           Assert.AreEqual(Action.GenerateImage, res5.action);
           Assert.AreEqual("OK", res5.message);
       }
   
       [Test]
       public void TestSereializeOutgoingMessageMeta()
       {
           var meta1 = new OutgoingMessageMeta { command = Command.Chat, type = OutputType.Text };
           var meta2 = new OutgoingMessageMeta { command = Command.Chat, type = OutputType.Audio };
           var meta3 = new OutgoingMessageMeta { command = Command.Chat, type = OutputType.Gesture };
   
           string ser1 = JsonConvert.SerializeObject(meta1);
           string ser2 = JsonConvert.SerializeObject(meta2);
           string ser3 = JsonConvert.SerializeObject(meta3);
   
           var deser1 = JsonConvert.DeserializeObject<OutgoingMessageMeta>(ser1);
           var deser2 = JsonConvert.DeserializeObject<OutgoingMessageMeta>(ser2);
           var deser3 = JsonConvert.DeserializeObject<OutgoingMessageMeta>(ser3);
   
           Assert.AreEqual(Command.Chat, deser1.command);
           Assert.AreEqual(OutputType.Text, deser1.type);
   
           Assert.AreEqual(Command.Chat, deser2.command);
           Assert.AreEqual(OutputType.Audio, deser2.type);
   
           Assert.AreEqual(Command.Chat, deser3.command);
           Assert.AreEqual(OutputType.Gesture, deser3.type);
       }*/
}
