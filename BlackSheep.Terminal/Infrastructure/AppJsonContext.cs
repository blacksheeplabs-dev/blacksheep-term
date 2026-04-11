using System.Text.Json.Serialization;
using BlackSheep.Terminal.Core.Models;

namespace BlackSheep.Terminal.Infrastructure;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppConfig))]
internal partial class AppJsonContext : JsonSerializerContext
{
}