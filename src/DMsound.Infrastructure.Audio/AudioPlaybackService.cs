using DMsound.Application.Abstractions;
using DMsound.Application.Models;
using NAudio.CoreAudioApi;
using NAudio.Wave;

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
        _outputDevice = new WasapiOut(outputDevice, AudioClientShareMode.Shared, useEventSync: false, latency: 50);

        _outputDevice.PlaybackStopped += HandlePlaybackStopped;
        _outputDevice.Init(_audioFileReader);
        _outputDevice.Play();
    }

    public void Dispose()
    {
        DisposePlaybackResources();
    }

    private void HandlePlaybackStopped(object? sender, StoppedEventArgs e)
    {
        DisposePlaybackResources();
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