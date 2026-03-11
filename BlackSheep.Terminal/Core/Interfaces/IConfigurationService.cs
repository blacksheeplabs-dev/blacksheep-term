using BlackSheep.Terminal.Core.Models;

namespace BlackSheep.Terminal.Core.Interfaces;

public interface IConfigurationService
{
    AppConfig Load();
    void Save(AppConfig config);
}