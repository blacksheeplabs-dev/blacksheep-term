using BlackSheep.Terminal.Core.Commands;
using BlackSheep.Terminal.Core.Interfaces;
using Spectre.Console;
using System.Diagnostics;

namespace BlackSheep.Terminal.Application;

public class CommandProcessor
{
    private readonly Dictionary<string, ICommand> _builtIns = new();
    private readonly Dictionary<string, string> _aliases = new();

    public CommandProcessor()
    {
        //Registramos los built-ins
        RegisterBuiltIn(new CdCommand());

        //Registramos Aliases Multiplataforma
        if (OperatingSystem.IsWindows())
        {
            _aliases["ll"] = "dir";
            _aliases["ls"] = "dir";
            _aliases["clear"] = "cls";
        }
        else
        {
            _aliases["ll"] = "ls -la";
        }
    }

    private void RegisterBuiltIn(ICommand command)
    {
        _builtIns[command.Name.ToLower()] = command;
    }

    public void Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        // 1. Dividir el input para analizar el primer término
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var cmdName = parts[0].ToLower();
        var args = parts.Skip(1).ToArray();

        //2. Resolver Alias
        string finalCommand;
        if(_aliases.TryGetValue(cmdName, out var aliasedCmd))
        {
            // Reconstruimos el comando: Alias + Argumentos originales
            var remainingArgs = args.Length > 0 ? " " + string.Join(" ", args) : string.Empty;
            finalCommand = aliasedCmd + remainingArgs;

            // Volvemos a extraer el cmdName del alias por si es un built-in (ej: clear -> cls)
            var aliasedParts = aliasedCmd.Split(' ');
            cmdName = aliasedParts[0].ToLower();
            // Si el alias tiene argumentos propios (como ls -la), los combinamos
            args = aliasedParts.Skip(1).Concat(args).ToArray();
        }
        else
        {
            finalCommand = input;
        }

        // 3. Ejecutar Built-in si existe (ahora funciona incluso si vino de un alias)
        if (_builtIns.TryGetValue(cmdName, out var builtIn))
        {
            builtIn.Execute(args);
            return;
        }

        // 4. Ejecutar el comando final (con alias aplicado) en el Shell
        ExecuteSystemCommand(finalCommand);
    }

    private void ExecuteSystemCommand(string fullCommand)
    {
        try
        {
            using var process = new Process();
            if (OperatingSystem.IsWindows())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c {fullCommand}";
            }
            else
            {
                process.StartInfo.FileName = "/bin/zsh";
                process.StartInfo.Arguments = $"-c \"{fullCommand}\"";
            }

            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();

        }catch(Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error de sistema:[/] {ex.Message}");
        }
    }
}