using DMsound.Application.Abstractions;
using DMsound.Application.Models;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace DMsound.Infrastructure.Audio;

public sealed class AudioPlaybackService : ISoundPlaybackService, IDisposable
{
    private int? _selectedOutputDeviceNumber;
    private WasapiOut? _outputDevice;
    private AudioFileReader? _audioFileReader;

    public IReadOnlyList<AudioOutputDevice> GetOutputDevices()
    {
        using var enumerator = new MMDeviceEnumerator();
        var endpoints = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        var devices = new List<AudioOutputDevice>(endpoints.Count);

        for (var index = 0; index < endpoints.Count; index++)
        {
            devices.Add(new AudioOutputDevice(index, endpoints[index].FriendlyName));
        }

        return devices;
    }

    public int? GetSelectedOutputDeviceNumber()
    {
        return _selectedOutputDeviceNumber;
    }

    public void SelectOutputDevice(int deviceNumber)
    {
        var deviceCount = GetOutputDevices().Count;

        if (deviceNumber < 0 || deviceNumber >= deviceCount)
        {
            throw new InvalidOperationException("Le peripherique de sortie audio est invalide.");
        }

        _selectedOutputDeviceNumber = deviceNumber;
    }

    public void Play(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Le chemin du fichier audio ne peut pas etre vide.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Le fichier audio est introuvable.", filePath);
        }

        DisposePlaybackResources();

        using var enumerator = new MMDeviceEnumerator();
        var outputDevice = ResolveOutputDevice(enumerator);

        _audioFileReader = new AudioFileReader(filePath);
        PlaySource(outputDevice, (ISampleProvider)_audioFileReader);
    }

    public AudioWaveformAnalysis AnalyzeWaveform(string filePath, int peakCount)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Le chemin du fichier audio ne peut pas etre vide.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Le fichier audio est introuvable.", filePath);
        }

        if (peakCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(peakCount));
        }

        using var reader = new AudioFileReader(filePath);
        var peaks = new List<double>(peakCount);
        var totalSamples = Math.Max(1, (int)(reader.Length / sizeof(float)));
        var samplesPerPeak = Math.Max(1, (int)Math.Ceiling(totalSamples / (double)peakCount));
        var buffer = new float[samplesPerPeak];

        int read;
        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0 && peaks.Count < peakCount)
        {
            var maxPeak = 0d;

            for (var index = 0; index < read; index++)
            {
                maxPeak = Math.Max(maxPeak, Math.Abs(buffer[index]));
            }

            peaks.Add(Math.Round(maxPeak * 100d, 2));
        }

        if (peaks.Count == 0)
        {
            peaks.Add(0);
        }

        return new AudioWaveformAnalysis(reader.TotalTime.TotalSeconds, peaks);
    }

    public void Stop()
    {
        _outputDevice?.Stop();
    }

    public void PreviewSegment(string filePath, TimeSpan start, TimeSpan end)
    {
        ValidateSegment(filePath, start, end);

        DisposePlaybackResources();

        using var enumerator = new MMDeviceEnumerator();
        var outputDevice = ResolveOutputDevice(enumerator);

        _audioFileReader = new AudioFileReader(filePath);
        _audioFileReader.CurrentTime = start;

        var source = new OffsetSampleProvider(_audioFileReader)
        {
            Take = end - start,
        };

        PlaySource(outputDevice, source);
    }

    public string TrimSegment(string filePath, TimeSpan start, TimeSpan end)
    {
        ValidateSegment(filePath, start, end);

        using var reader = new AudioFileReader(filePath);
        reader.CurrentTime = start;

        var source = new OffsetSampleProvider(reader)
        {
            Take = end - start,
        };

        // Créer le fichier trimmed dans le dossier AudioTrimmed
        var trimmedFolder = Path.Combine(AppContext.BaseDirectory, "Assets\\AudioTrimmed");
        Directory.CreateDirectory(trimmedFolder);
        
        var outputPath = Path.Combine(
            trimmedFolder,
            $"{Path.GetFileNameWithoutExtension(filePath)}_trimmed.wav");

        WaveFileWriter.CreateWaveFile16(outputPath, source);
        return outputPath;
    }

    public void Dispose()
    {
        DisposePlaybackResources();
    }

    private void HandlePlaybackStopped(object? sender, StoppedEventArgs e)
    {
        DisposePlaybackResources();
    }

    private void PlaySource(MMDevice outputDevice, IWaveProvider source)
    {
        _outputDevice = new WasapiOut(outputDevice, AudioClientShareMode.Shared, useEventSync: false, latency: 50);

        _outputDevice.PlaybackStopped += HandlePlaybackStopped;
        _outputDevice.Init(source);
        _outputDevice.Play();
    }

    private void PlaySource(MMDevice outputDevice, ISampleProvider source)
    {
        PlaySource(outputDevice, source.ToWaveProvider16());
    }

    private static void ValidateSegment(string filePath, TimeSpan start, TimeSpan end)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Le chemin du fichier audio ne peut pas etre vide.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Le fichier audio est introuvable.", filePath);
        }

        if (start < TimeSpan.Zero || end <= start)
        {
            throw new ArgumentException("La plage audio selectionnee est invalide.");
        }
    }

    private void DisposePlaybackResources()
    {
        if (_outputDevice is not null)
        {
            _outputDevice.PlaybackStopped -= HandlePlaybackStopped;
            _outputDevice.Dispose();
            _outputDevice = null;
        }

        _audioFileReader?.Dispose();
        _audioFileReader = null;
    }

    private MMDevice ResolveOutputDevice(MMDeviceEnumerator enumerator)
    {
        var endpoints = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        if (_selectedOutputDeviceNumber.HasValue)
        {
            return endpoints[_selectedOutputDeviceNumber.Value];
        }

        return enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
    }
}