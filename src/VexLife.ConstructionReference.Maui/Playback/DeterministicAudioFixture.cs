// [VXG RealForever]
// Deterministic in-memory PCM WAV used only by the native Play/Pause practicum.

using System.Text;

namespace VexLife.ConstructionReference.Maui.Playback;

public static class DeterministicAudioFixture
{
    public static MemoryStream Create()
    {
        const int sampleRate = 44_100;
        const short channels = 1;
        const short bitsPerSample = 16;
        const double durationSeconds = 0.75;
        const double frequencyHz = 440.0;
        const double amplitude = 0.18;

        var sampleCount = (int)(sampleRate * durationSeconds);
        var dataBytes = sampleCount * channels * (bitsPerSample / 8);
        var stream = new MemoryStream(capacity: 44 + dataBytes);
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);

        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataBytes);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * (bitsPerSample / 8));
        writer.Write((short)(channels * (bitsPerSample / 8)));
        writer.Write(bitsPerSample);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataBytes);

        for (var sample = 0; sample < sampleCount; sample++)
        {
            var phase = 2.0 * Math.PI * frequencyHz * sample / sampleRate;
            var value = (short)(Math.Sin(phase) * amplitude * short.MaxValue);
            writer.Write(value);
        }

        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
