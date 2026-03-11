using BlackSheep.Terminal;
using BlackSheep.Terminal.Application;
using BlackSheep.Terminal.Infrastructure;
using Spectre.Console;

var configService = new JsonConfigurationService();
var fileService = new FileSystemService();
var app = new TerminalApp(configService, fileService);
app.Run();
