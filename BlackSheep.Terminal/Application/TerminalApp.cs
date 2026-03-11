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
            _selectedIndex (_selectedIndex + 1) % _suggestions.Count;
            UpdateGhostFromSelection();
            continue;
         }
      }
   }

}