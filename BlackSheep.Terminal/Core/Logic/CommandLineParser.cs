using BlackSheep.Terminal.Core.Models;

namespace BlackSheep.Terminal.Core.Logic;

public static class CommandLineParser
{
    /// <summary>
    /// Divide un string de comando en tokens inteligentes, respetando comillas y espacios.
    /// </summary>
    public static List<CommandToken> Parse(string input)
    {
        var tokens = new List<CommandToken>();
        if(string.IsNullOrWhiteSpace(input)) return tokens;

        bool inQuotes = false;
        int start = -1;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            // Manejo de comillas
            if (c == '\"')
            {
                if (!inQuotes)
                {
                    inQuotes = true;
                    start = i + 1; // El token comienza después de la comilla
                }
                else
                {
                    inQuotes = false;
                    // Guardamos el token cerrado con sus índices reales
                    tokens.Add(new CommandToken(input[start..i], start - 1, i, true));
                    start = -1;
                }
                continue;
            }

            // Manejo de espacios (solo si no estamos dentro de comillas)
            if(char.IsWhiteSpace(c) && !inQuotes)
            {
                if (start != -1)
                {
                    tokens.Add(new CommandToken(input[start..i], start, i - 1, false));
                    start = -1;
                }
            }

            // Inicio de un nuevo token de texto plano
            else if (start == -1)
            {
                start = i;
            }
        }
        // Si el string termina y hay un token pendiente (última palabra o comilla abierta)
        if(start != -1)
        {
            tokens.Add(new CommandToken(
                input[start..], 
                start, 
                input.Length - 1, 
                inQuotes, 
                inQuotes)); // Si inQuotes es true aquí, significa que quedó abierta
        }

        return tokens;
    }
}