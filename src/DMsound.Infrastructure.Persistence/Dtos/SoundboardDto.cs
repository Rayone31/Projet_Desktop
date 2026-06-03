namespace DMsound.Infrastructure.Persistence.Dtos;

internal sealed class SoundboardDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required bool IsVisible { get; set; }
    public required List<SoundDto> Sounds { get; set; }
}
