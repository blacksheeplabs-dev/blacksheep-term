using BlackSheep.Terminal.Core.Interfaces;
using Spectre.Console;

namespace BlackSheep.Terminal.Core.Commands;

public class CdCommand : ICommand
{
    public string Name => "cd";
    public string Description => "Cambia el directorio de trabajo actual.";

    public void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            // Si no hay argumentos, vamos al Home del usuario
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            Directory.SetCurrentDirectory(home);
            return;
        }

        var targetPath = string.Join(" ", args);
        try
        {
            if (Directory.Exists(targetPath))
            {
                Directory.SetCurrentDirectory(targetPath);
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] El directorio '{targetPath}' no existe.");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error al cambiar de directorio:[/] {ex.Message}");
        }
    }
}