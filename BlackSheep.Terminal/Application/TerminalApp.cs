using Gui = Terminal.Gui;
using BlackSheep.Terminal.Core.Interfaces;
using BlackSheep.Terminal.Core.Models;
using Terminal.Gui;

namespace BlackSheep.Terminal.Application;

public class TerminalApp
{
   private readonly IConfigurationService _configService;
   private readonly IFileSystemService _fileService;
   private AppConfig _config;

   public TerminalApp(IConfigurationService configService, IFileSystemService fileService)
   {
      _configService = configService;
      _fileService = fileService;
      _config = _configService.Load();
   }

   public void Run()
   {
      //Inicializa el motor de la terminal
      Gui.Application.Init();
      var win = new Window("BlackSheep Terminal")
      {
         X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
      };

      var chatView = new TextView()
      {
         X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() - 2,
         ReadOnly = true,
         CanFocus = false
      };
      
      var prompt = new Label("> ")
      {
         X = 0, Y = Pos.Bottom(chatView), Width = 2, Height = 1
      };

      var inputField = new TextField("")
      {
         X = 2, Y = Pos.Bottom(chatView), Width = Dim.Fill(), Height = 1
      };

      //autocompletado
      inputField.Autocomplete.AllSuggestions = new List<string>();

      inputField.TextChanged += (prev) =>
      {
         var currentText = inputField.Text.ToString() ?? "";
         inputField.Autocomplete.AllSuggestions = _fileService.GetPathSuggestions(currentText);
      };

      inputField.KeyDown += (e) =>
      {
         if (e.KeyEvent.Key == Key.Enter)
         {
            var text = inputField.Text.ToString();
            if (string.IsNullOrWhiteSpace(text)) return;

            //con esto se actualiza el historial
            chatView.Text = chatView.Text.ToString() + "\n> " + text;

            if (text.StartsWith("/auth "))
            {
               _config.GeminiApiKey = text[6..].Trim();
               _configService.Save(_config);
               chatView.Text = chatView.Text.ToString() + "\n[SISTEMA] API Key guardada con éxito";
            }
            else
            {
               chatView.Text = chatView.Text.ToString() + "\n[AI] Gemini responderá aquí pronto...";
            }

            inputField.Text = "";

            //Para que baje automaticamente
            chatView.CursorPosition = new Point(0, chatView.Lines - 1);
         }
      };

      win.Add(chatView, inputField);
      Gui.Application.Top.Add(win);
      inputField.SetFocus();
      Gui.Application.Run();
      Gui.Application.Shutdown();
   }
   
}