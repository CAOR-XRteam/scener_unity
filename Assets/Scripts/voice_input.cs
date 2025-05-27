using System.Collections;
using System.IO;
using UnityEngine;

public class VoiceInput : MonoBehaviour
{
    private static byte[] ConvertAudioToByteArray(AudioClip clip)
    {
        var samples = new float[clip.samples];
        clip.GetData(samples, 0);

        MemoryStream stream = new();
        BinaryWriter writer = new(stream);

        //RIFF CHUNK
        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + clip.samples * clip.channels * 2); // 36 + data size
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        //FMT CHUNK
        writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
        writer.Write(16); // Subchunk size for PCM
        writer.Write((ushort)1); // Audio format (1 for PCM)
        writer.Write((ushort)clip.channels); // Number of channels
        writer.Write(clip.frequency); // Sample rate
        writer.Write(clip.frequency * clip.channels * 2); // Byte rate
        writer.Write((ushort)(clip.channels * 2)); // Block align
        writer.Write((ushort)16); // Bits per sample

        //DATA CHUNK
        writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
        writer.Write(samples.Length * 2);

        foreach (var sample in samples)
        {
            short pcmSample = (short)(sample * 32767.0f);
            writer.Write(pcmSample);
        }

        byte[] bytes = stream.ToArray();

        return bytes;
    }
}
