using BlackSheep.Terminal.Core.Interfaces;
using BlackSheep.Terminal.Core.Models;
using Spectre.Console;

namespace BlackSheep.Terminal.Application;

using System.Numerics;
using System.Text;

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

            if (key.Key == ConsoleKey.Enter)
            {
                ExecuteCommand();
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
            }
            else if (!char.IsControl(key.KeyChar))
            {
                _inputBuffer.Append(key.KeyChar);
            }

            UpdateSuggestions();
        }
    }

    private void RenderEverything()
    {
        try
        {
            int windowHeight = Console.WindowHeight;
            int windowWidth = Console.WindowWidth;

            // 1. CONFIGURACIÓN COMPACTA (Máximo 3 sugerencias)
            int maxVisibleMenu = 3;
            int menuLinesCount = (_suggestions.Count > 1) ? Math.Min(_suggestions.Count, maxVisibleMenu) : 0;

            // La interfaz es elástica: 1 (Barra) + menuLinesCount + 1 (Prompt)
            int currentInterfaceHeight = menuLinesCount + 2;
            int maxInterfaceHeight = 5;

            int promptLine = windowHeight - 1;
            int statusBarLine = promptLine - menuLinesCount - 1;
            int menusStartLine = statusBarLine + 1;

            // 2. LIMPIEZA DE SEGURIDAD (Limpiamos el rango máximo de acción)
            // Esto evita que queden restos de barras antiguas en el historial
            for (int i = windowHeight - maxInterfaceHeight; i <= promptLine; i++)
            {
                if (i >= 0 && i < Console.BufferHeight)
                {
                    Console.SetCursorPosition(0, i);
                    Console.Write(new string(' ', windowWidth - 1));
                }
            }

            // 3. OBTENER RUTA ACTUAL (Toque Senior: ~ para el Home)
            string cwd = Directory.GetCurrentDirectory();
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string displayPath = cwd.Replace(home, "~");

            // 4. DIBUJAR BARRA DE ESTADO (El "Horizonte")
            if (statusBarLine >= 0)
            {
                Console.SetCursorPosition(0, statusBarLine);
                var rule = new Rule($"[{_theme.PrimaryColor} bold] {displayPath} [/]")
                    .LeftJustified()
                    .RuleStyle($"{_theme.PrimaryColor} dim");
                AnsiConsole.Write(rule);
            }

            // 5. DIBUJAR MENÚ DE SUGERENCIAS (Compacto)
            if (menuLinesCount > 0)
            {
                Console.SetCursorPosition(0, menusStartLine);
                var menu = new Table().NoBorder().HideHeaders();
                menu.AddColumn("Icon");
                menu.AddColumn("Text");

                for (int i = 0; i < menuLinesCount; i++)
                {
                    var isSelected = i == _selectedIndex;
                    var color = isSelected ? _theme.PrimaryColor : "grey35";
                    var icon = isSelected ? "➜" : " ";
                    menu.AddRow($"[{color}]{icon}[/]", $"[{color}]{_suggestions[i]}[/]");
                }
                //Margen de 2 espacios para alineacion visual
                AnsiConsole.Write(new Padder(menu, new Padding(2,0,0,0)));
            }

            // 6. DIBUJAR PROMPT (Última línea)
            Console.SetCursorPosition(0, promptLine);
            AnsiConsole.Markup($"[{_theme.PrimaryColor}]>[/] ");

            string currentInput = _inputBuffer.ToString();
            Console.Write(currentInput);

            if (!string.IsNullOrEmpty(_ghostText))
            {
                AnsiConsole.Markup($"[{_theme.GhostTextColor}]{_ghostText}[/]");
            }

            // 7. RE-POSICIONAR CURSOR (Exactamente tras el input)
            int finalCursorLeft = Math.Min(currentInput.Length + 2, windowWidth - 1);
            Console.SetCursorPosition(finalCursorLeft, promptLine);


        }
        catch{}
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
        // var text = _inputBuffer.ToString();
        // _suggestions = _fileService.GetPathSuggestions(text);
        // _selectedIndex = 0;
        // UpdateGhostFromSelection();
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
        // 1. CALCULAR ALTURA DE LA INTERFAZ ANTES DE BORRAR
        int menuHeight = (_suggestions.Count > 1) ? Math.Min(_suggestions.Count, 3) : 0;
        int currentInterfaceHeight = menuHeight + 2;
        int windowHeight = Console.WindowHeight;
        
        // 2. LIMPIEZA TOTAL ANTES DE IMPRIMIR AL HISTORIAL
        // Borramos físicamente la barra y el prompt para que no se "graben" en el historial al hacer scroll
        int cleanStart = Math.Max(0, windowHeight - 5);
        for (int i = cleanStart; i < windowHeight; i++)
        {
            Console.SetCursorPosition(0, i);
            Console.Write(new string(' ', Console.WindowWidth - 1));
        }

        // 3. POSICIONAR CURSOR PARA EL COMANDO
        // El comando se imprime donde estaba la barra de estado, integrándose al historial
        Console.SetCursorPosition(0, windowHeight - currentInterfaceHeight);

        if (string.IsNullOrEmpty(cmd))
        {
            _inputBuffer.Clear();
            _suggestions.Clear();
            _ghostText = "";
            return;
        }

        // Imprimimos el comando que el usuario ejecutó
        AnsiConsole.MarkupLine($"[{_theme.PrimaryColor}]>[/] {cmd}");

        // 4. EJECUTAR EL COMANDO
        if (cmd.StartsWith("#"))
        {
            AnsiConsole.MarkupLine($"[{_theme.PrimaryColor}]AI:[/] Procesando: [italic]{cmd[1..]}[/]");
        }
        else
        {
            _commandProcessor.Process(cmd);
        }

        // 5. EMPUJAR EL HISTORIAL (CRÍTICO)
        // Imprimimos líneas vacías para que el output del comando suba
        // y deje espacio libre para el próximo RenderEverything()
        for (int i = 0; i < currentInterfaceHeight; i++)
        {
            Console.WriteLine();
        }

        // 6. LIMPIEZA FINAL Y RESET
        _inputBuffer.Clear();
        _suggestions.Clear();
        _ghostText = "";
    }

}