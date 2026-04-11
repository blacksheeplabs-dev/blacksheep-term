# Project Status: BlackSheep Terminal (Shell-like CLI)

## 🎭 ROL
- **Senior Developer / Instructor:** Mi función es guiarte, explicar detalladamente el código y proponer las mejores soluciones arquitectónicas.
- **Intervención:** Explicaré y diseñaré el código a escribir, pero **no realizaré cambios directos** en los archivos a menos que lo solicites explícitamente.

## 🚀 Overview
**BlackSheep Terminal** es una interfaz de línea de comandos (CLI) interactiva y moderna. Ha evolucionado de un prototipo básico a una terminal con **Interfaz de Pantalla Fija (Sticky Bottom)**, emulando la experiencia de herramientas premium como Warp o el CLI de Gemini.

---

## 🛠️ Stack Tecnológico
- **Runtime:** .NET 10.0 (Core)
- **UI/Rendering:** [Spectre.Console](https://spectreconsole.net/) (Tablas, Padder, Rule, Markup, AnsiConsole)
- **Compilación:** Native AOT (Ahead-of-Time) para alta velocidad y portabilidad.
- **Arquitectura:** Command Pattern + Dependency Injection manual + Elastic UI Logic.

---

## 🏗️ Arquitectura del Proyecto
1.  **`BlackSheep.Terminal/`**
    - `Program.cs`: Punto de entrada y composición.
    - **`Application/`**: 
        - `TerminalApp.cs`: Motor de UI. Gestiona el renderizado elástico y la sincronización del historial.
        - `CommandProcessor.cs`: Motor de despacho de comandos, gestión de alias y built-ins.
    - **`Core/`**:
        - `Interfaces/`: `ICommand`, `IFileSystemService`, `IConfigurationService`.
        - `Commands/`: Implementaciones de comandos internos (ej: `CdCommand`).
    - **`Infrastructure/`**:
        - `FileSystemService.cs`: Lógica de sugerencias de rutas.

---

## ✨ Características Implementadas
- [x] **Sticky Bottom Horizon:** Barra de estado fija que muestra el CWD (Current Working Directory) con soporte para el símbolo `~` en el Home del usuario.
- [x] **Elastic Interface (Compacta):** La zona interactiva es dinámica (2 a 5 líneas). El menú de sugerencias está limitado a 3 elementos para maximizar el área de historial.
- [x] **Clean History Execution:** Lógica avanzada en `ExecuteCommand` que limpia la barra de estado y el menú de la pantalla antes de imprimir el comando, evitando que la interfaz se "grabe" en el historial de la terminal.
- [x] **Command Dispatcher:** Soporte para comandos internos (`cd`), alias multiplataforma (`ll` -> `dir`/`ls -la`) y comandos nativos del sistema.
- [x] **Ghost Text:** Autocompletado visual que se consolida con `Tab` o `Flecha Derecha`.
- [x] **Safe & Robust Rendering:** Uso de `Console.Write` plano para el input del usuario para evitar errores de interpretación de markup y mantener el cursor siempre sincronizado.

---

## 📈 Próximos Pasos (Pendiente)
1. **Integración con Gemini:** Implementar la lógica para que los comandos con `#` interactúen con la API de Google Gemini (Próximo gran hito).
2. **Historial de Comandos:** Navegación por comandos anteriores (Flecha Arriba/Abajo) con persistencia entre sesiones.
3. **Persistencia de Configuración:** Guardar y cargar el API Key de Gemini y el tema visual desde un archivo JSON.
4. **Mejora del Lexer:** Soporte para rutas con espacios y comillas (importante para el comando `cd`).
5. **Comando de Ayuda:** Implementar `help` visual con Spectre.Console para listar funcionalidades.

---

## 📝 Notas de Inspección (Sync para otra PC)
- **Coordenadas:** Los métodos `RenderEverything` y `ExecuteCommand` están sincronizados usando un rango de acción de 5 líneas máximas. No cambiar uno sin ajustar el otro.
- **Limpieza:** La limpieza del historial se basa en sobreescribir con espacios en blanco antes de que el scroll de la terminal ocurra.
- **Native AOT:** El proyecto está configurado para publicarse como binario nativo; evitar librerías que usen demasiada reflexión dinámica para mantener la compatibilidad.
