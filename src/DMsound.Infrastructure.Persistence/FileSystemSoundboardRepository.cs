using System.Text.Json;
using DMsound.Application.Abstractions;
using DMsound.Domain;
using DMsound.Infrastructure.Persistence.Demo;
using DMsound.Infrastructure.Persistence.Dtos;

namespace DMsound.Infrastructure.Persistence;

public sealed class FileSystemSoundboardRepository : ISoundboardRepository
{
    private readonly List<Soundboard> _soundboards = new();
    private readonly string _dataFilePath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private FileSystemSoundboardRepository(string dataFilePath)
    {
        _dataFilePath = dataFilePath;
    }

    public static FileSystemSoundboardRepository Create(string? dataDirectory = null)
    {
        var directory = dataDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DMsound");

        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, "soundboards.json");
        var repository = new FileSystemSoundboardRepository(filePath);
        repository.Load();
        return repository;
    }

    public void Add(Soundboard soundboard)
    {
        _soundboards.Add(soundboard);
        Save();
    }

    public void Update(Soundboard soundboard)
    {
        Save();
    }

    public Soundboard? GetById(SoundboardId id)
    {
        return _soundboards.FirstOrDefault(item => item.Id == id);
    }

    public IReadOnlyCollection<Soundboard> GetAll()
    {
        return _soundboards.AsReadOnly();
    }

    public bool ExistsByName(string name)
    {
        return _soundboards.Any(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private void Load()
    {
        _soundboards.Clear();

        if (!File.Exists(_dataFilePath))
        {
            CreateDemoSoundboard();
            Save();
            return;
        }

        try
        {
            var json = File.ReadAllText(_dataFilePath);
            var dtos = JsonSerializer.Deserialize<List<SoundboardDto>>(json, _jsonOptions);

            if (dtos is not null)
            {
                foreach (var dto in dtos)
                {
                    _soundboards.Add(MapFromDto(dto));
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des soundboards: {ex.Message}");
            _soundboards.Clear();
        }

        if (_soundboards.Count == 0)
        {
            CreateDemoSoundboard();
            Save();
        }
    }

    private void CreateDemoSoundboard()
    {
        DemoSoundAssetFactory.EnsureAudioOriginalsFolder();
        DemoSoundAssetFactory.EnsureAudioTrimmedFolder();

        var gaming = new Soundboard(SoundboardId.New(), "Gaming");

        var sounds = new[]
        {
            ("sncf.mp3", "sncf", "A"),
            ("fah.mp3", "fah", "S"),
            ("discord-notif.mp3", "discord-notif", "D")
        };

        foreach (var (fileName, soundName, hotkeyChar) in sounds)
        {
            var initialPath = DemoSoundAssetFactory.TryGetOriginalAudioPath(fileName);

            if (initialPath is null)
            {
                continue;
            }

            var sound = new Sound(SoundId.New(), soundName, initialPath, new Hotkey(hotkeyChar));
            var trimmedPath = DemoSoundAssetFactory.GetTrimmedAudioPath(fileName);

            if (trimmedPath is not null)
            {
                sound.UpdateModifiedFilePath(trimmedPath);
            }

            gaming.AddSound(sound);
        }

        _soundboards.Add(gaming);
    }

    private void Save()
    {
        try
        {
            var dtos = _soundboards.Select(MapToDto).ToList();
            var json = JsonSerializer.Serialize(dtos, _jsonOptions);
            File.WriteAllText(_dataFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la sauvegarde des soundboards: {ex.Message}");
        }
    }

    private static SoundboardDto MapToDto(Soundboard soundboard)
    {
        return new SoundboardDto
        {
            Id = soundboard.Id.Value.ToString(),
            Name = soundboard.Name,
            IsVisible = soundboard.IsVisible,
            Sounds = soundboard.Sounds.Select(sound => new SoundDto
            {
                Id = sound.Id.Value.ToString(),
                Name = sound.Name,
                InitialFilePath = sound.InitialFilePath,
                ModifiedFilePath = sound.ModifiedFilePath,
                Hotkey = sound.Hotkey?.Value
            }).ToList()
        };
    }

    private static Soundboard MapFromDto(SoundboardDto dto)
    {
        var soundboard = new Soundboard(
            new SoundboardId(Guid.Parse(dto.Id)),
            dto.Name,
            dto.IsVisible);

        foreach (var soundDto in dto.Sounds ?? [])
        {
            try
            {
                var initialFilePath = ResolveInitialFilePath(soundDto);
                var modifiedFilePath = ResolveModifiedFilePath(soundDto, initialFilePath);
                var hotkey = string.IsNullOrWhiteSpace(soundDto.Hotkey)
                    ? (Hotkey?)null
                    : new Hotkey(soundDto.Hotkey);

                var sound = new Sound(
                    new SoundId(Guid.Parse(soundDto.Id)),
                    soundDto.Name,
                    initialFilePath,
                    hotkey);

                if (!string.Equals(modifiedFilePath, initialFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    sound.UpdateModifiedFilePath(modifiedFilePath);
                }

                soundboard.AddSound(sound);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Son ignore lors du chargement: {ex.Message}");
            }
        }

        return soundboard;
    }

    private static string ResolveInitialFilePath(SoundDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.InitialFilePath))
        {
            return dto.InitialFilePath;
        }

        return dto.OriginalFilePath
            ?? throw new InvalidOperationException("Le son ne contient pas de fichier initial.");
    }

    private static string ResolveModifiedFilePath(SoundDto dto, string initialFilePath)
    {
        if (!string.IsNullOrWhiteSpace(dto.ModifiedFilePath))
        {
            return dto.ModifiedFilePath;
        }

        return dto.FilePath ?? initialFilePath;
    }
}
