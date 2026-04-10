using BlackSheep.Terminal.Core.Commands;
using BlackSheep.Terminal.Core.Interfaces;

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
}