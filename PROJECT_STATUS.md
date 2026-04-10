# Project Status: BlackSheep Terminal (Shell-like CLI)

## 🎭 ROL
- **Senior Developer / Instructor:** Mi función es guiarte, explicar detalladamente el código y proponer las mejores soluciones arquitectónicas.
- **Intervención:** Explicaré y diseñaré el código a escribir, pero **no realizaré cambios directos** en los archivos a menos que lo solicites explícitamente.

## 🚀 Overview
**BlackSheep Terminal** es una interfaz de línea de comandos (CLI) interactiva y moderna, diseñada para ofrecer una experiencia similar a terminales avanzadas (como Warp) con autocompletado en tiempo real, "ghost text" y sugerencias inteligentes. Está construida sobre el ecosistema .NET y optimizada para ser distribuida como un binario nativo.

---

## 🛠️ Stack Tecnológico
- **Runtime:** .NET 10.0 (Core)
- **UI/Rendering:** [Spectre.Console](https://spectreconsole.net/) (Tablas, Markup, AnsiConsole)
- **Compilación:** Native AOT (Ahead-of-Time) para alta velocidad y portabilidad sin dependencias.
- **Lenguaje:** C# 14+ (usando características modernas como colecciones y slicing).

---

## 🏗️ Arquitectura del Proyecto
El proyecto sigue una estructura limpia y desacoplada:

1.  **`BlackSheep.Terminal/`**
    - `Program.cs`: Punto de entrada y composición de servicios.
    - **`Application/`**: 
        - `TerminalApp.cs`: El "corazón" del CLI. Gestiona el bucle principal, la captura de teclas (`Console.ReadKey`), el renderizado y la ejecución de comandos.
    - **`Core/`**:
        - `Interfaces/`: Definición de contratos (`IFileSystemService`, `IConfigurationService`).
        - `Models/`: Entidades de dominio como `AppConfig` y `AppTheme`.
    - **`Infrastructure/`**:
        - `FileSystemService.cs`: Lógica de exploración del sistema de archivos para sugerencias.
        - `JsonConfigurationService.cs`: Manejo de persistencia de configuración.

---

## ✨ Características Implementadas
- [x] **Bucle Interactivo:** Captura de entrada carácter por carácter sin interrumpir el flujo.
- [x] **Ghost Text:** Sugerencias visuales en gris oscuro que se completan con `Tab` o `Flecha Derecha`.
- [x] **Menú de Sugerencias (Warp-style):** Menú vertical dinámico que aparece bajo el prompt para navegación de archivos.
- [x] **Integración con el Sistema:** Ejecución de comandos nativos mediante `zsh` (macOS/Linux) o `powershell` (Windows).
- [x] **Tematización:** Soporte inicial para colores personalizados mediante `AppTheme`.
- [x] **Preparado para IA:** Prefijo `#` reservado para comandos procesados por Gemini.

---

## 📈 Próximos Pasos (Pendiente)
1. **Integración con Gemini:** Implementar la lógica para que los comandos con `#` interactúen con la API de Google Gemini.
2. **Persistencia de Configuración:** Activar el uso de `JsonConfigurationService` para guardar el API Key y preferencias.
3. **Historial de Comandos:** Implementar navegación por comandos anteriores con las flechas.
4. **Comandos de Sistema y Multiplataforma:** 
    - Corregir el funcionamiento de `cd` (debe ser un comando interno o "builtin" ya que un proceso hijo no puede cambiar el directorio del padre).
    - Implementar alias universales (ej: `ll` debe funcionar en Windows mapeando a una lógica interna o a `dir`).
    - Asegurar que el CLI sea agnóstico al SO en sus comandos básicos.
5. **Refactorización de Renderizado:** Optimizar la lógica de `RenderEverything` para evitar parpadeos y mejorar el manejo del scroll.
5. **Robustez:** Manejo de errores más detallado en la ejecución de procesos externos.

---

## 📝 Notas de Inspección
- El código está bien estructurado y utiliza interfaces para facilitar el testeo.
- La lógica de sugerencias de archivos en `FileSystemService` es funcional pero limitada a los primeros 10 resultados para mantener el rendimiento.
- El uso de `Native AOT` es un gran acierto para una herramienta de terminal por su tiempo de arranque instantáneo.
