namespace BlackSheep.Terminal.Core.Models;

/// <summary>
/// Representa una palabra o ruta dentro de un comando.
/// </summary>
/// <param name="Value">El texto del token (sin comillas).</param>
/// <param name="StartIndex">Posición inicial en el string original.</param>
/// <param name="EndIndex">Posición final en el string original.</param>
/// <param name="IsQuoted">Indica si el token estaba entre comillas.</param>
/// <param name="IsUnclosed">Indica si el token tiene una comilla abierta sin cerrar.</param>
public record CommandToken(
    string Value, 
    int StartIndex,
    int EndIndex,
    bool IsQuoted,
    bool IsUnclosed = false
    );