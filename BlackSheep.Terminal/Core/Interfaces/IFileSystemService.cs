namespace BlackSheep.Terminal.Core.Interfaces;

public interface IFileSystemService
{
    List<string> GetPathSuggestions(string currentInput);
}