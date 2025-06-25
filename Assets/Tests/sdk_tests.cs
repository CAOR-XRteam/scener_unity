using Newtonsoft.Json;
using NUnit.Framework;
using SceneDeserialization;

//using scener.ws;

// TODO: unit tests

public class MessagesTests
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

// public class SceneTests
// {
//     [Test]
//     public void TestDeserializeFullScene()
//     {
//         string json =
//             @"{
//             'action': 'scene_generation',
//             'message': 'Full scene test with detailed objects.',
//             'final_scene_json': {
//                 'skybox': {
//                     'type': 'sun',
//                     'top_color': {'r': 0.1, 'g': 0.2, 'b': 0.3},
//                     'top_exponent': 1.1,
//                     'horizon_color': {'r': 0.4, 'g': 0.5, 'b': 0.6},
//                     'bottom_color': {'r': 0.7, 'g': 0.8, 'b': 0.9},
//                     'bottom_exponent': 1.2,
//                     'sky_intensity': 1.3,
//                     'sun_color': {'r': 1.0, 'g': 0.9, 'b': 0.8},
//                     'sun_intensity': 1.4,
//                     'sun_alpha': 1.5,
//                     'sun_beta': 1.6,
//                     'sun_vector': {'x': 0.1, 'y': 0.2, 'z': 0.3, 'w': 0.4}
//                 },
//                 'lights': [
//                     {
//                         'id': 'light_spot', 'type': 'spot', 'intensity': 5.0, 'spot_angle': 45.0,
//                         'range': 15.0, 'mode': 'realtime', 'shadow_type': 'soft_shadows', 'color': {'r':1,'g':1,'b':1},
//                         'position': {'x':0,'y':5,'z':0}, 'rotation': {'x':90,'y':0,'z':0}, 'scale': {'x':1,'y':1,'z':1},
//                         'indirect_multiplier': 1.0
//                     },
//                     {
//                         'id': 'light_dir', 'type': 'directional', 'intensity': 1.1,
//                         'mode': 'realtime', 'shadow_type': 'no_shadows', 'color': {'r':1,'g':1,'b':1},
//                         'position': {'x':0,'y':0,'z':0}, 'rotation': {'x':50,'y':-30,'z':0}, 'scale': {'x':1,'y':1,'z':1},
//                         'indirect_multiplier': 1.0
//                     }
//                 ],
//                 'objects': [
//                     {
//                         'id': 'obj_prim', 'name': 'MyCube', 'type': 'primitive', 'shape': 'cube',
//                         'position': {'x':0,'y':0.5,'z':0}, 'rotation': {'x':0,'y':0,'z':0}, 'scale': {'x':1,'y':1,'z':1},
//                         'color': {'r': 1, 'g': 0, 'b': 0}
//                     },
//                     {
//                         'id': 'obj_dyn', 'name': 'theatre', 'type': 'dynamic',
//                         'position': {'x':5,'y':0,'z':5}, 'rotation': {'x':0,'y':0,'z':0}, 'scale': {'x':1,'y':1,'z':1},
//                         'shape': null
//                     }
//                 ]
//             }
//         }";

//         var fullOutput = JsonConvert.DeserializeObject<FinalDecompositionOutput>(
//             json.Replace("'", "\"")
//         );
//         var scene = fullOutput?.final_scene_json;

//         Assert.NotNull(scene.skybox, "Skybox should not be null.");
//         Assert.IsInstanceOf<SunSkybox>(scene.skybox, "Skybox should be of type SunSkybox.");
//         var sunSkybox = scene.skybox as SunSkybox;
//         Assert.AreEqual(1.3f, sunSkybox.sky_intensity, 0.001f, "Sky intensity should match.");
//         Assert.AreEqual(
//             0.5f,
//             sunSkybox.horizon_color.g,
//             0.001f,
//             "Horizon color G component should match."
//         );
//         Assert.AreEqual(1.6f, sunSkybox.sun_beta, 0.001f, "Sun beta should match.");

//         Assert.NotNull(scene.lights, "Lights list should not be null.");
//         Assert.AreEqual(2, scene.lights.Count, "There should be 2 lights.");
//         Assert.IsInstanceOf<SpotLight>(scene.lights[0], "First light should be a SpotLight.");
//         Assert.IsInstanceOf<DirectionalLight>(
//             scene.lights[1],
//             "Second light should be a DirectionalLight."
//         );

//         Assert.NotNull(scene.objects, "Objects list should not be null.");
//         Assert.AreEqual(2, scene.objects.Count, "There should be 2 objects.");
//         Assert.AreEqual(SceneObjectType.Primitive, scene.objects[0].type);
//         Assert.AreEqual(SceneObjectType.Dynamic, scene.objects[1].type);
//         Assert.IsFalse(scene.objects[1].shape.HasValue, "Shape should be null.");
//     }

//     [Test]
//     public void TestDeserializeGradientSkybox()
//     {
//         string json =
//             @"{
//             'skybox': {
//                 'type': 'gradient',
//                 'color1': {'r': 0.1, 'g': 0.2, 'b': 0.3, 'a': 1.0},
//                 'color2': {'r': 0.4, 'g': 0.5, 'b': 0.6, 'a': 1.0},
//                 'up_vector': {'x': 0.0, 'y': 1.0, 'z': 0.0, 'w': 0.0},
//                 'intensity': 1.5,
//                 'exponent': 2.5
//             }
//         }";

//         var scene = JsonConvert.DeserializeObject<Scene>(json.Replace("'", "\""));
//         var skybox = scene?.skybox as GradientSkybox;

//         Assert.AreEqual(0.1f, skybox.color1.r, 0.001f);
//         Assert.AreEqual(0.2f, skybox.color1.g, 0.001f);
//         Assert.AreEqual(0.3f, skybox.color1.b, 0.001f);
//         Assert.AreEqual(0.4f, skybox.color2.r, 0.001f);
//         Assert.AreEqual(0.5f, skybox.color2.g, 0.001f);
//         Assert.AreEqual(0.6f, skybox.color2.b, 0.001f);
//         Assert.AreEqual(0.0f, skybox.up_vector.x, 0.001f);
//         Assert.AreEqual(1.0f, skybox.up_vector.y, 0.001f);
//         Assert.AreEqual(1.5f, skybox.intensity, 0.001f);
//         Assert.AreEqual(2.5f, skybox.exponent, 0.001f);
//     }

//     [Test]
//     public void TestDeserializeSunSkybox()
//     {
//         string json =
//             @"{
//             'skybox': {
//                 'type': 'sun',
//                 'top_color': {'r': 0.1, 'g': 0.2, 'b': 0.3, 'a': 1.0},
//                 'top_exponent': 1.1,
//                 'horizon_color': {'r': 0.4, 'g': 0.5, 'b': 0.6, 'a': 1.0},
//                 'bottom_color': {'r': 0.7, 'g': 0.8, 'b': 0.9, 'a': 1.0},
//                 'bottom_exponent': 1.2,
//                 'sky_intensity': 1.3,
//                 'sun_color': {'r': 1.0, 'g': 0.9, 'b': 0.8, 'a': 1.0},
//                 'sun_intensity': 1.4,
//                 'sun_alpha': 1.5,
//                 'sun_beta': 1.6,
//                 'sun_vector': {'x': 0.1, 'y': 0.2, 'z': 0.3, 'w': 0.4}
//             }
//         }";

//         var scene = JsonConvert.DeserializeObject<Scene>(json.Replace("'", "\""));
//         var skybox = scene?.skybox as SunSkybox;

//         Assert.AreEqual(1.1f, skybox.top_exponent, 0.001f);
//         Assert.AreEqual(1.2f, skybox.bottom_exponent, 0.001f);
//         Assert.AreEqual(1.3f, skybox.sky_intensity, 0.001f);
//         Assert.AreEqual(1.4f, skybox.sun_intensity, 0.001f);
//         Assert.AreEqual(1.5f, skybox.sun_alpha, 0.001f);
//         Assert.AreEqual(1.6f, skybox.sun_beta, 0.001f);
//         Assert.AreEqual(0.1f, skybox.top_color.r, 0.001f);
//         Assert.AreEqual(0.5f, skybox.horizon_color.g, 0.001f);
//         Assert.AreEqual(0.9f, skybox.bottom_color.b, 0.001f);
//         Assert.AreEqual(1.0f, skybox.sun_color.r, 0.001f);
//         Assert.AreEqual(0.1f, skybox.sun_vector.x, 0.001f);
//     }

//     [Test]
//     public void TestDeserializeCubedSkybox()
//     {
//         string json =
//             @"{
//             'skybox': {
//                 'type': 'cubed',
//                 'tint_color': {'r': 0.9, 'g': 0.8, 'b': 0.7, 'a': 1.0},
//                 'exposure': 1.2,
//                 'rotation': 90.0,
//                 'cube_map': 'test'
//             }
//         }";

//         var scene = JsonConvert.DeserializeObject<Scene>(json.Replace("'", "\""));
//         var skybox = scene?.skybox as CubedSkybox;

//         Assert.AreEqual(0.9f, skybox.tint_color.r, 0.001f);
//         Assert.AreEqual(0.8f, skybox.tint_color.g, 0.001f);
//         Assert.AreEqual(0.7f, skybox.tint_color.b, 0.001f);
//         Assert.AreEqual(1.2f, skybox.exposure, 0.001f);
//         Assert.AreEqual(90.0f, skybox.rotation, 0.001f);
//         Assert.AreEqual("test", skybox.cube_map);
//     }

//     [Test]
//     public void TestDeserializeSpotLight()
//     {
//         string json =
//             @"{
//             'lights': [
//                 {
//                     'id': 'spot_01', 'type': 'spot',
//                     'position': {'x': 1, 'y': 2, 'z': 3},
//                     'rotation': {'x': 4, 'y': 5, 'z': 6},
//                     'scale': {'x': 7, 'y': 8, 'z': 9},
//                     'color': {'r': 0.1, 'g': 0.2, 'b': 0.3},
//                     'intensity': 1.5,
//                     'indirect_multiplier': 0.5,
//                     'range': 20.0,
//                     'spot_angle': 45.5,
//                     'mode': 'realtime',
//                     'shadow_type': 'soft_shadows'
//                 }
//             ]
//         }";

//         var scene = JsonConvert.DeserializeObject<Scene>(json.Replace("'", "\""));
//         var light = scene?.lights[0] as SpotLight;

//         Assert.AreEqual("spot_01", light.id);
//         Assert.AreEqual(2f, light.position.y, 0.001f);
//         Assert.AreEqual(5f, light.rotation.y, 0.001f);
//         Assert.AreEqual(8f, light.scale.y, 0.001f);
//         Assert.AreEqual(0.2f, light.color.g, 0.001f);
//         Assert.AreEqual(1.5f, light.intensity, 0.001f);
//         Assert.AreEqual(0.5f, light.indirect_multiplier, 0.001f);
//         Assert.AreEqual(20.0f, light.range, 0.001f);
//         Assert.AreEqual(45.5f, light.spot_angle, 0.001f);
//         Assert.AreEqual(LightMode.Realtime, light.mode);
//         Assert.AreEqual(ShadowType.SoftShadows, light.shadow_type);
//     }

//     [Test]
//     public void TestDeserializeDirectionalLight()
//     {
//         string json =
//             @"{
//             'lights': [
//                 {
//                     'id': 'dir_01', 'type': 'directional',
//                     'position': {'x': 0, 'y': 0, 'z': 0},
//                     'rotation': {'x': 50, 'y': -30, 'z': 0},
//                     'scale': {'x': 1, 'y': 1, 'z': 1},
//                     'color': {'r': 1.0, 'g': 0.95, 'b': 0.8},
//                     'intensity': 1.1,
//                     'indirect_multiplier': 1.0,
//                     'mode': 'mixed',
//                     'shadow_type': 'hard_shadows'
//                 }
//             ]
//         }";

//         var scene = JsonConvert.DeserializeObject<Scene>(json.Replace("'", "\""));
//         var light = scene?.lights[0] as DirectionalLight;

//         Assert.AreEqual("dir_01", light.id);
//         Assert.AreEqual(50f, light.rotation.x, 0.001f);
//         Assert.AreEqual(0.95f, light.color.g, 0.001f);
//         Assert.AreEqual(1.1f, light.intensity, 0.001f);
//         Assert.AreEqual(LightMode.Mixed, light.mode);
//         Assert.AreEqual(ShadowType.HardShadows, light.shadow_type);
//     }

//     [Test]
//     public void TestDeserializePointLight()
//     {
//         string json =
//             @"{
//             'lights': [
//                 {
//                     'id': 'point_01', 'type': 'point',
//                     'position': {'x': 10, 'y': 5, 'z': -2},
//                     'rotation': {'x': 0, 'y': 0, 'z': 0},
//                     'scale': {'x': 1, 'y': 1, 'z': 1},
//                     'color': {'r': 0.2, 'g': 0.5, 'b': 1.0},
//                     'intensity': 10.0,
//                     'indirect_multiplier': 1.0,
//                     'range': 15.0,
//                     'mode': 'baked',
//                     'shadow_type': 'no_shadows'
//                 }
//             ]
//         }";

//         var scene = JsonConvert.DeserializeObject<Scene>(json.Replace("'", "\""));
//         var light = scene?.lights[0] as PointLight;

//         Assert.AreEqual("point_01", light.id);
//         Assert.AreEqual(10f, light.position.x, 0.001f);
//         Assert.AreEqual(1.0f, light.color.b, 0.001f);
//         Assert.AreEqual(10.0f, light.intensity, 0.001f);
//         Assert.AreEqual(15.0f, light.range, 0.001f);
//         Assert.AreEqual(LightMode.Baked, light.mode);
//         Assert.AreEqual(ShadowType.NoShadows, light.shadow_type);
//     }

//     [Test]
//     public void TestDeserializeAreaLight()
//     {
//         string json =
//             @"{
//             'lights': [
//                 {
//                     'id': 'area_01', 'type': 'area',
//                     'position': {'x': 0, 'y': 3, 'z': 0},
//                     'rotation': {'x': 90, 'y': 0, 'z': 0},
//                     'scale': {'x': 1, 'y': 1, 'z': 1},
//                     'color': {'r': 0.8, 'g': 0.8, 'b': 1.0},
//                     'intensity': 2.5,
//                     'indirect_multiplier': 1.0,
//                     'shape': 'rectangle',
//                     'range': 10.0,
//                     'width': 5.0,
//                     'height': 3.0,
//                     'radius': null
//                 }
//             ]
//         }";

//         var scene = JsonConvert.DeserializeObject<Scene>(json.Replace("'", "\""));
//         var light = scene?.lights[0] as AreaLight;

//         Assert.AreEqual("area_01", light.id);
//         Assert.AreEqual(90f, light.rotation.x, 0.001f);
//         Assert.AreEqual(2.5f, light.intensity, 0.001f);
//         Assert.AreEqual(LightShape.Rectangle, light.shape);
//         Assert.AreEqual(10.0f, light.range, 0.001f);
//         Assert.IsTrue(light.width.HasValue);
//         Assert.AreEqual(5.0f, light.width.Value, 0.001f);
//         Assert.IsTrue(light.height.HasValue);
//         Assert.AreEqual(3.0f, light.height.Value, 0.001f);
//         Assert.IsFalse(light.radius.HasValue, "Radius should be null for a rectangle area light.");
//     }
// }
