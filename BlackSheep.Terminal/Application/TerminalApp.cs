using BlackSheep.Terminal.Core.Interfaces;
using BlackSheep.Terminal.Core.Models;
using Spectre.Console;

namespace BlackSheep.Terminal.Application;

using System.Text;

public class TerminalApp
{
   private readonly IConfigurationService _configService;
   private readonly IFileSystemService _fileService;
   private readonly AppTheme _theme = new();
   
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


         // Limpiamos la línea del prompt y las líneas de abajo (el menú)
         // Usamos Console.SetCursorPosition para no parpadear
         var promptLine = Console.CursorTop;
         var maxLines = Console.BufferHeight;
         var maxVisibleMenu = 5;


         for (int i = 0; i <= maxVisibleMenu; i++)
         {
            var targetLine = promptLine + i;
            if (targetLine < maxLines)
            {
               Console.SetCursorPosition(0, targetLine);
               Console.Write(new string(' ', Console.WindowWidth));
            }
         }

         Console.SetCursorPosition(0, promptLine);

         // A. Pintamos el Prompt y Ghost Text
         AnsiConsole.Markup($"[{_theme.PrimaryColor}]>[/] {_inputBuffer}");
         AnsiConsole.Markup($"[{_theme.GhostTextColor}]{_ghostText}[/]");
         Console.WriteLine();

         // B. Pintamos el Menú de Sugerencias (estilo Warp)
         if (_suggestions.Count > 1)
         {
            var menuLines = Math.Min(_suggestions.Count, maxVisibleMenu);
            var menu = new Table()
               .NoBorder()
               .HideHeaders();

            menu.AddColumn("I");
            menu.AddColumn("N");

            for (int i = 0; i < menuLines; i++)
            {
               var isSelected = i == _selectedIndex;
               // Colores: El seleccionado en el color primario, el resto en gris oscuro
               var color = isSelected ? _theme.PrimaryColor : "grey35";
               var icon = isSelected ? "➜" : " ";

               menu.AddRow(
                  $"[{color}]{icon}[/]",
                  $"[{color}]{_suggestions[i]}[/]"
               );
            }

            AnsiConsole.Write(menu);
         }

         var cursorLeft = Math.Min(_inputBuffer.Length + 2, Console.WindowWidth - 1);
         Console.SetCursorPosition(cursorLeft, promptLine);
      }catch{}
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
      Console.WriteLine();

      if (string.IsNullOrEmpty(cmd)) return;

      if (cmd.StartsWith("#"))
      {
         // Lógica de Gemini (Siguiente paso)
         AnsiConsole.MarkupLine($"[{_theme.PrimaryColor}]AI:[/] Procesando: [italic]{cmd[1..]}[/]");
      }
      else
      {
         try
         {
            var process = new System.Diagnostics.Process();

            if (OperatingSystem.IsWindows())
            {
               process.StartInfo.FileName = "powershell.exe";
               process.StartInfo.Arguments = $"/c {cmd}";
            }
            else
            {
               process.StartInfo.FileName = "/bin/zsh";
               process.StartInfo.Arguments = $"-c \"{cmd}\"";
            }

            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();
         }
         catch (Exception ex)
         {
            AnsiConsole.MarkupLine($"[red]Error al ejecutar:[/] {ex.Message}");
         }
      }

      Console.WriteLine();
      _inputBuffer.Clear();
      _suggestions.Clear();
      _ghostText = "";
      // // Lógica de ejecución (limpiar buffers y saltar línea)
      // Console.SetCursorPosition(0, Console.CursorTop + (_suggestions.Count > 1 ? _suggestions.Count + 1 : 1));
      // var cmd = _inputBuffer.ToString();
      // AnsiConsole.MarkupLine($"[grey]Ejecutando:[/] {cmd}");
      // _inputBuffer.Clear();
      // _suggestions.Clear();
      // _ghostText = "";
   }

}