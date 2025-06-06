using System.IO;
using UnityEngine;


namespace scener.input {
public class VoiceInput : MonoBehaviour
{
    private static float[] TrimAudioClip(int pos, AudioClip clip)
    {
        // Mic recording pads clips with silence to maxRecordDuration so have to trim before sendind to the server

        var originalClip = new float[clip.samples * clip.channels];
        var trimmedClip = new float[pos * clip.channels];

        clip.GetData(originalClip, 0);
        System.Array.Copy(originalClip, 0, trimmedClip, 0, pos * clip.channels);

        return trimmedClip;
    }

    public static byte[] ConvertToWav(int pos, AudioClip clip)
    {
        var samples = TrimAudioClip(pos, clip);

        MemoryStream stream = new();
        BinaryWriter writer = new(stream);

        // Various WAV headers so that whisper can read it directly from a file

        // RIFF CHUNK
        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + samples.Length * 2); // 36 + data size
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        // FMT CHUNK
        writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
        writer.Write(16); // Subchunk size for PCM
        writer.Write((ushort)1); // Audio format (1 for PCM)
        writer.Write((ushort)clip.channels); // Number of channels
        writer.Write(clip.frequency); // Sample rate
        writer.Write(clip.frequency * clip.channels * 2); // Byte rate
        writer.Write((ushort)(clip.channels * 2)); // Block align
        writer.Write((ushort)16); // Bits per sample

        // DATA CHUNK
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
}
