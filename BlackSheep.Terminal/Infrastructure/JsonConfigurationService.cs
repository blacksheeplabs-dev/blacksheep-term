using System.Text.Json;
using BlackSheep.Terminal.Core.Interfaces;
using BlackSheep.Terminal.Core.Models;

namespace BlackSheep.Terminal.Infrastructure;

public class JsonConfigurationService: IConfigurationService
{
    private readonly string _configPath;

    public JsonConfigurationService()
    {
        _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "black-sheep",
            "config.json");
    }

    public AppConfig Load()
    {
        if (!File.Exists(_configPath)) return new AppConfig();

        try
        {
            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
}

    public void Save(AppConfig config)
    {
        var directory = Path.GetDirectoryName(_configPath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, json);
    }
}