using BlackSheep.Terminal;
using Spectre.Console;

AnsiConsole.Write(
    new FigletText("BlackSheep")
        .LeftJustified()
        .Color(Color.Purple));

AnsiConsole.MarkupLine("[bold grey]Cross-platform AI Terminal Powered by Gemini[/]\n");

var config = ConfigManager.Load();

if (string.IsNullOrWhiteSpace(config.GeminiApiKey))
{
    AnsiConsole.MarkupLine("[yellow]No se encontró una Gemini API Key configurada.[/]");
    var apiKey = AnsiConsole.Prompt(
        new TextPrompt<string>("Por favor, ingresa tu [bold]Gemini API Key[/]:")
            .PromptStyle("purple")
            .Secret());

    config.GeminiApiKey = apiKey;
    ConfigManager.Save(config);
    AnsiConsole.MarkupLine("[green]¡Configuración guardada correctamente![/]\n");
}
else
{
    AnsiConsole.MarkupLine("[green]API Key cargada correctamente.[/]\n");
}

AnsiConsole.MarkupLine("[bold white]¿En qué puedo ayudarte hoy?[/]");

// TODO: Implementar bucle principal y cliente Gemini.
var prompt = AnsiConsole.Ask<string>("[purple]>[/]");
AnsiConsole.MarkupLine($"Recibido: [italic]{prompt}[/]");
