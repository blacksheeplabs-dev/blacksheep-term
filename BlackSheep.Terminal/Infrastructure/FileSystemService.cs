using BlackSheep.Terminal.Core.Interfaces;

namespace BlackSheep.Terminal.Infrastructure;

public class FileSystemService: IFileSystemService
{
    public List<string> GetPathSuggestions(string currentInput)
    {
        if (string.IsNullOrWhiteSpace(currentInput)) return new List<string>();

        try
        {
            //Normalizamos la ruta para que funcione en todos los sistemas operativos
            var path = Path.GetDirectoryName(currentInput) ?? ".";
            var searchPattern = Path.GetFileName(currentInput) + "*";

            if (!Directory.Exists(path)) return new List<string>();
            
            // Buscamos archivos y directorios que empiecen por lo que escribió el usuario
            return Directory.GetFileSystemEntries(path, searchPattern)
                .Select(Path.GetFileName)
                .Where(f => f != null)
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