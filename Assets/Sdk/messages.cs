using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Google.Protobuf;

// Workaround for record types in older .NET/Unity versions
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace Scener.Sdk
{
    public enum Status
    {
        stream,
        error,
    };

    public enum OutgoingMessageType
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
        [EnumMember(Value = "unrelated_response")]
        UnrelatedResponse,

        [EnumMember(Value = "generate_image")]
        GenerateImage,

        [EnumMember(Value = "generate_3d_object")]
        Generate3DObject,

        [EnumMember(Value = "generate_3d_scene")]
        Generate3DScene,

        [EnumMember(Value = "convert_speech")]
        ConvertSpeech,
    }

    public interface IOutgoingMessage
    {
        Content ToProto();
    }

    public record OutgoingTextMessage(string Message) : IOutgoingMessage
    {
        public Content ToProto()
        {
            return new Content
            {
                Type = OutgoingMessageType.Text.ToEnumString(),
                Text = this.Message,
                Status = 200,
            };
        }
    }

    public record OutgoingAudioMessage(byte[] AudioData) : IOutgoingMessage
    {
        public Content ToProto()
        {
            var content = new Content
            {
                Type = OutgoingMessageType.Audio.ToEnumString(),
                Status = 200,
            };
            var audioAsset = new MediaAsset
            {
                Id = "user_voice_recording",
                Data = ByteString.CopyFrom(this.AudioData),
                Filename = "voice.wav",
            };
            content.Assets.Add(audioAsset);

            return content;
        }
    }

    public record OutgoingGestureMessage(string GestureData) : IOutgoingMessage
    {
        public Content ToProto()
        {
            return new Content
            {
                Type = OutgoingMessageType.Gesture.ToEnumString(),
                Text = this.GestureData,
                Status = 200,
            };
        }
    }

    public interface IIncomingMessage { }

    public static class IncomingMessageFactory
    {
        public static IIncomingMessage FromProto(Content protoContent)
        {
            if (protoContent.Status != 200)
            {
                return new IncomingErrorMessage(protoContent.Status, $"Error: {protoContent.Text}");
            }

            return protoContent.Type switch
            {
                "unrelated_response" => new IncomingUnrelatedResponseMessage(protoContent.Text),
                "generate_image" => new IncomingGenerateImageMessage(
                    protoContent.Text,
                    protoContent
                        .Assets.Select(asset => new AppMediaAsset(
                            asset.Id,
                            asset.Filename,
                            asset.Data.ToByteArray()
                        ))
                        .ToList()
                ),
                "generate_3d_object" => new IncomingGenerate3DObjectMessage(
                    protoContent.Text,
                    protoContent
                        .Assets.Select(asset => new AppMediaAsset(
                            asset.Id,
                            asset.Filename,
                            asset.Data.ToByteArray()
                        ))
                        .ToList()
                ),
                "generate_3d_scene" => new IncomingGenerate3DSceneMessage(
                    protoContent.Text,
                    protoContent.Metadata,
                    protoContent
                        .Assets.Select(asset => new AppMediaAsset(
                            asset.Id,
                            asset.Filename,
                            asset.Data.ToByteArray()
                        ))
                        .ToList()
                ),
                "convert_speech" => new IncomingConvertSpeechMessage(protoContent.Text),
                _ => new IncomingUnknownMessage(protoContent.Type),
            };
        }
    }

    public record AppMediaAsset(string Id, string Filename, byte[] Data);

    public record IncomingUnrelatedResponseMessage(string ResponseText) : IIncomingMessage;

    public record IncomingGenerateImageMessage(string ResponseText, List<AppMediaAsset> Data)
        : IIncomingMessage;

    public record IncomingGenerate3DObjectMessage(string ResponseText, List<AppMediaAsset> Data)
        : IIncomingMessage;

    public record IncomingGenerate3DSceneMessage(
        string ResponseText,
        string Scene,
        List<AppMediaAsset> Data
    ) : IIncomingMessage;

    public record IncomingConvertSpeechMessage(string ResponseText) : IIncomingMessage;

    public record IncomingErrorMessage(int Status, string ErrorText) : IIncomingMessage;

    public record IncomingUnknownMessage(string OriginalType) : IIncomingMessage;
}
