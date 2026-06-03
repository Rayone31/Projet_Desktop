namespace DMsound.Application.Abstractions;

public interface IAudioLibraryStorage
{
    string StoreOriginal(string sourceFilePath);
}
