namespace DMsound.Infrastructure.Persistence.Dtos;

internal sealed class SoundDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string InitialFilePath { get; set; }
    public required string ModifiedFilePath { get; set; }
    public string? Hotkey { get; set; }
    public bool IsEnabled { get; set; } = true;

    // Legacy JSON fields (read-only compatibility).
    public string? OriginalFilePath { get; set; }
    public string? FilePath { get; set; }
}
