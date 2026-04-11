using BlackSheep.Terminal.Core.Commands;
using BlackSheep.Terminal.Core.Interfaces;
using BlackSheep.Terminal.Core.Logic;
using BlackSheep.Terminal.Core.Models;
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

        // 1. Parsear el input en tokens inteligentes (respetando comillas y espacios)
        var tokens = CommandLineParser.Parse(input);
        if(tokens.Count == 0) return;

        // El primer token siempre es el comando
        var cmdNameToken = tokens[0];
        var cmdName = cmdNameToken.Value.ToLower();

        // Los argumentos son los valores limpios (sin comillas) de los tokens restantes
        var args = tokens.Skip(1).Select(t => t.Value).ToArray();

        // 2. Resolver Alias (Lógica mejorada)
        if(_aliases.TryGetValue(cmdName, out var aliasedCmd))
        {
            // Si hay un alias (ej: ll -> ls -la), lo parseamos también para extraer sus partes
            var aliasedTokens = CommandLineParser.Parse(aliasedCmd);
            cmdName = aliasedTokens[0].Value.ToLower();

            // Combinamos los argumentos del alias con los argumentos originales
            args = aliasedTokens.Skip(1).Select(t => t.Value).Concat(args).ToArray();
        }

        // 3. Ejecutar Built-in si existe
        if(_builtIns.TryGetValue(cmdName, out var builtIn))
        {
            builtIn.Execute(args);
            return;
        }

        // 4. Ejecutar comando del sistema
        // IMPORTANTE: Al reconstruir para cmd.exe o zsh, debemos asegurarnos de
        // que los argumentos originales que tenian espacios sigan protegidos.
        ExecuteSystemCommand(cmdName, args);
    }

    private void ExecuteSystemCommand(string cmd, string[] args)
    {
        try
        {
            // Protegemos con comillas SOLO los argumentos que tienen espacios
            var protectedArgs = args.Select(a => (a.Contains(' ') && !a.StartsWith("\"")) ? $"\"{a}\"" : a);
            string fullArgs = string.Join(" ", protectedArgs);
            string fullCommand = string.IsNullOrEmpty(fullArgs) ? cmd : $"{cmd} {fullArgs}";

            using var process = new Process();
            if (OperatingSystem.IsWindows())
            {
                process.StartInfo.FileName = "cmd.exe";
                // Usamos /c y envolvemos TODO el comando en comillas para que CMD maneje bien los espacios internos
                process.StartInfo.Arguments = $"/c \"{fullCommand}\"";
            }
            else
            {
                process.StartInfo.FileName = "/bin/zsh";
                process.StartInfo.Arguments = $"-c \"{fullCommand}\"";
            }

            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();

        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error de sistema:[/] {ex.Message}");
        }
    }
}