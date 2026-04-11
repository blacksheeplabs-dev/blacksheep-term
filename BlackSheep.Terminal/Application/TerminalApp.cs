using BlackSheep.Terminal.Core.Interfaces;
using BlackSheep.Terminal.Core.Models;
using Spectre.Console;
using System.Text;

namespace BlackSheep.Terminal.Application;

public class TerminalApp
{
    private readonly IConfigurationService _configService;
    private readonly IFileSystemService _fileService;
    private readonly AppTheme _theme = new();
    private readonly CommandProcessor _commandProcessor = new();

    private StringBuilder _inputBuffer = new();
    private List<string> _suggestions = new();
    private int _selectedIndex = 0;
    private string _ghostText = "";
    private int _lastUiHeight = 2; // Memoria de limpieza para evitar fantasmas

    public TerminalApp(IConfigurationService configService, IFileSystemService fileService)
    {
        _configService = configService;
        _fileService = fileService;
    }

    public void Run()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[{_theme.PrimaryColor}]BlackSheep Shell[/]\n");
        while (true)
        {
            RenderEverything();

            var key = Console.ReadKey(intercept: true);
            bool inputChanged = false;

            if (key.Key == ConsoleKey.Enter)
            {
                // Si hay sugerencias y texto fantasma, el primer Enter selecciona/completa
                if (_suggestions.Count > 0 && !string.IsNullOrEmpty(_ghostText))
                {
                    _inputBuffer.Append(_ghostText);
                    _ghostText = "";
                    _suggestions.Clear();
                }
                else
                {
                    // Si no hay nada que sugerir, ejecutamos el comando
                    ExecuteCommand();
                }
                continue;
            }

            if (key.Key == ConsoleKey.DownArrow && _suggestions.Count > 0)
            {
                _selectedIndex = (_selectedIndex + 1) % _suggestions.Count;
                UpdateGhostFromSelection();
                continue;
            }

            if (key.Key == ConsoleKey.UpArrow && _suggestions.Count > 0)
            {
                _selectedIndex = (_selectedIndex - 1 + _suggestions.Count) % _suggestions.Count;
                UpdateGhostFromSelection();
                continue;
            }

            if (key.Key == ConsoleKey.Tab || key.Key == ConsoleKey.RightArrow)
            {
                if (!string.IsNullOrEmpty(_ghostText))
                {
                    _inputBuffer.Append(_ghostText);
                    _ghostText = "";
                    _suggestions.Clear();
                }
                continue;
            }

            if (key.Key == ConsoleKey.Backspace && _inputBuffer.Length > 0)
            {
                _inputBuffer.Remove(_inputBuffer.Length - 1, 1);
                inputChanged = true;
            }
            else if (!char.IsControl(key.KeyChar))
            {
                _inputBuffer.Append(key.KeyChar);
                inputChanged = true;
            }

            if (inputChanged)
            {
                UpdateSuggestions();
            }
        }
    }

    private void RenderEverything()
    {
        try
        {
            int windowHeight = Console.WindowHeight;
            int windowWidth = Console.WindowWidth;

            // --- COORDENADAS (Declaradas primero para evitar CS0841) ---
            int promptLine = windowHeight - 1;
            int statusBarLine = windowHeight - 2;
            int suggestionsCount = (_suggestions.Count > 0) ? Math.Min(_suggestions.Count, 3) : 0;
            int suggestionsStartLine = statusBarLine - suggestionsCount;

            // --- CÁLCULO DE LIMPIEZA ELÁSTICA ---
            int currentUiHeight = 2 + suggestionsCount;
            int heightToClean = Math.Max(currentUiHeight, _lastUiHeight);
            int cleanStart = windowHeight - heightToClean;

            // Limpieza quirúrgica basada en la memoria de la UI anterior
            for (int i = cleanStart; i <= promptLine; i++)
            {
                if (i >= 0 && i < Console.BufferHeight)
                {
                    Console.SetCursorPosition(0, i);
                    Console.Write(new string(' ', windowWidth - 1));
                }
            }
            _lastUiHeight = currentUiHeight;

            // --- 1. DIBUJAR SUGERENCIAS ---
            if (suggestionsCount > 0)
            {
                Console.SetCursorPosition(0, suggestionsStartLine);
                var menu = new Table().NoBorder().HideHeaders();
                menu.AddColumn("Icon");
                menu.AddColumn("Text");

                for (int i = 0; i < suggestionsCount; i++)
                {
                    var isSelected = i == _selectedIndex;
                    var color = isSelected ? _theme.PrimaryColor : "grey35";
                    var icon = isSelected ? "➜" : " ";
                    menu.AddRow($"[{color}]{icon}[/]", $"[{color}]{_suggestions[i]}[/]");
                }
                AnsiConsole.Write(new Padder(menu, new Padding(2, 0, 0, 0)));
            }

            // --- 2. DIBUJAR BARRA DE ESTADO ---
            string cwd = Directory.GetCurrentDirectory();
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string displayPath = cwd.Replace(home, "~");

            if (statusBarLine >= 0)
            {
                Console.SetCursorPosition(0, statusBarLine);
                var rule = new Rule($"[{_theme.PrimaryColor} bold] {displayPath} [/]")
                    .LeftJustified()
                    .RuleStyle($"{_theme.PrimaryColor} dim");
                AnsiConsole.Write(rule);
            }

            // --- 3. DIBUJAR PROMPT ---
            Console.SetCursorPosition(0, promptLine);
            AnsiConsole.Markup($"[{_theme.PrimaryColor}]>[/] ");

            string currentInput = _inputBuffer.ToString();
            Console.Write(currentInput);

            if (!string.IsNullOrEmpty(_ghostText))
            {
                AnsiConsole.Markup($"[{_theme.GhostTextColor}]{_ghostText}[/]");
            }

            // --- 4. POSICIONAR CURSOR ---
            int finalCursorLeft = Math.Min(currentInput.Length + 2, windowWidth - 1);
            Console.SetCursorPosition(finalCursorLeft, promptLine);
        }
        catch { }
    }

    private void UpdateSuggestions()
    {
        var fullText = _inputBuffer.ToString();
        if (string.IsNullOrWhiteSpace(fullText))
        {
            _suggestions.Clear();
            _ghostText = "";
            return;
        }

        var parts = fullText.Split(' ');
        var lastWord = parts[^1];

        _suggestions = _fileService.GetPathSuggestions(lastWord);
        _selectedIndex = 0;
        UpdateGhostFromSelection();
    }

    private void UpdateGhostFromSelection()
    {
        if (_suggestions.Count > 0)
        {
            var fullText = _inputBuffer.ToString();
            var parts = fullText.Split(' ');
            var lastWord = parts[^1];
            var selected = _suggestions[_selectedIndex];

            if (selected.StartsWith(lastWord, StringComparison.OrdinalIgnoreCase))
            {
                _ghostText = selected[lastWord.Length..];
            }
            else
            {
                _ghostText = "";
            }
        }
        else
        {
            _ghostText = "";
        }
    }

    private void ExecuteCommand()
    {
        var cmd = _inputBuffer.ToString().Trim();
        int windowHeight = Console.WindowHeight;

        // Limpieza dinámica antes de imprimir
        int activeSuggestions = (_suggestions.Count > 0) ? Math.Min(_suggestions.Count, 3) : 0;
        int currentUiHeight = 2 + activeSuggestions; 
        int cleanStart = windowHeight - currentUiHeight;

        for (int i = cleanStart; i < windowHeight; i++)
        {
            if (i >= 0 && i < Console.BufferHeight)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth - 1));
            }
        }

        Console.SetCursorPosition(0, cleanStart);

        if (string.IsNullOrEmpty(cmd))
        {
            ResetInput();
            return;
        }

        AnsiConsole.MarkupLine($"[{_theme.PrimaryColor}]>[/] {cmd}");

        if (cmd.StartsWith("#"))
        {
            AnsiConsole.MarkupLine($"[{_theme.PrimaryColor}]AI:[/] Procesando... ");
        }
        else
        {
            _commandProcessor.Process(cmd);
        }

        // Push compacto de 2 líneas
        for (int i = 0; i < 2; i++)
        {
            Console.WriteLine();
        }

        ResetInput();
    }

    private void ResetInput()
    {
        _inputBuffer.Clear();
        _suggestions.Clear();
        _ghostText = "";
    }
}