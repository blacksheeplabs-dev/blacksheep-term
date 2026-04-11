using BlackSheep.Terminal.Core.Interfaces;

namespace BlackSheep.Terminal.Infrastructure;

public class FileSystemService: IFileSystemService
{
    public List<string> GetPathSuggestions(string currentInput)
    {
        if (string.IsNullOrWhiteSpace(currentInput)) return new List<string>();

        try
        {
            string directory;
            string searchPattern;

            if (currentInput.EndsWith(":") && currentInput.Length == 2)
            {
                directory = currentInput + Path.DirectorySeparatorChar;
                searchPattern = "*";
            }
            else if (currentInput.EndsWith(Path.DirectorySeparatorChar) || currentInput.EndsWith("/"))
            {
                directory = currentInput;
                searchPattern = "*";
            }
            else
            {
                var dirName = Path.GetDirectoryName(currentInput);
                directory = string.IsNullOrEmpty(dirName) ? "." : dirName;
                searchPattern = (Path.GetFileName(currentInput) ?? "") + "*";
            }

            if (!Directory.Exists(directory)) return new List<string>();
            
            return Directory.GetFileSystemEntries(directory, searchPattern)
                .Select(Path.GetFileName)
                .Where(f => f != null && !f.StartsWith("."))
                .Cast<string>()
                .Take(10)
                .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }
}