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
            //Normalizamos la ruta para que funcione en todos los sistemas operativos
            // var path = Path.GetDirectoryName(currentInput) ?? ".";
            // var searchPattern = Path.GetFileName(currentInput) + "*";
            if (currentInput.EndsWith(":") && currentInput.Length == 2)
            {
                directory = currentInput + Path.DirectorySeparatorChar;
                searchPattern = "*";
            }else if (currentInput.EndsWith(Path.DirectorySeparatorChar) || currentInput.EndsWith("/"))
            {
                directory = currentInput;
                searchPattern = "*";
            }
            else
            {
                directory = Path.GetDirectoryName(currentInput);
                if(string.IsNullOrEmpty(directory)) directory = ".";
                searchPattern = Path.GetFileName(currentInput) + "*";
            }

            if (!Directory.Exists(directory)) return new List<string>();
            
            // Buscamos archivos y directorios que empiecen por lo que escribió el usuario
            return Directory.GetFileSystemEntries(directory, searchPattern)
                .Select(Path.GetFileName)
                .Where(f => !f!.StartsWith("."))
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