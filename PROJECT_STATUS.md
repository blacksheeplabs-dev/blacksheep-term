# Project Status: BlackSheep Terminal (Shell-like CLI)

## 🎭 ROL
- **Senior Developer / Instructor:** Mi función es guiarte, explicar detalladamente el código y proponer las mejores soluciones arquitectónicas.
- **Intervención:** Explicaré y diseñaré el código a escribir, pero **no realizaré cambios directos** en los archivos a menos que lo solicites explícitamente.

## 🚀 Overview
**BlackSheep Terminal** es una interfaz de línea de comandos (CLI) interactiva y moderna. Ha evolucionado de un prototipo básico a una terminal con **Interfaz de Pantalla Fija (Sticky Bottom)**, emulando la experiencia de herramientas premium como Warp o el CLI de Gemini.

---

## 🛠️ Stack Tecnológico
- **Runtime:** .NET 10.0 (Core)
- **UI/Rendering:** [Spectre.Console] (Tablas, Padder, Rule, Markup, AnsiConsole)
- **Serialization:** System.Text.Json con **Source Generation** (Compatible con Native AOT).
- **Compilación:** Native AOT (Ahead-of-Time) para alta velocidad y portabilidad.
- **Arquitectura:** Command Pattern + Dependency Injection manual + Elastic UI Logic + Memory Rendering.

---

## 🏗️ Arquitectura del Proyecto
1.  **`BlackSheep.Terminal/`**
    - `Program.cs`: Punto de entrada y composición.
    - **`Application/`**: 
        - `TerminalApp.cs`: Motor de UI. Gestiona el renderizado elástico con memoria (`_lastUiHeight`) y la sincronización del historial.
        - `CommandProcessor.cs`: Motor de despacho de comandos, gestión de alias y built-ins.
    - **`Core/`**:
        - `Interfaces/`: `ICommand`, `IFileSystemService`, `IConfigurationService`.
        - `Commands/`: Implementaciones de comandos internos (ej: `CdCommand`).
    - **`Infrastructure/`**:
        - `FileSystemService.cs`: Lógica de sugerencias de rutas.
        - `JsonConfigurationService.cs`: Persistencia compatible con AOT.
        - `AppJsonContext.cs`: Contexto de generación de código para JSON.

---

## ✨ Características Implementadas
- [x] **Sticky Bottom Horizon:** Barra de estado fija que actúa como "techo" de la zona de entrada.
- [x] **Elastic Smart Interface:** La zona interactiva es dinámica (2 a 5 líneas) y usa **Memoria de Renderizado** para limpiar "fantasmas" visuales quirúrgicamente.
- [x] **Clean History Execution:** El historial es blindado mediante un "push" dinámico, evitando que el output de los comandos (como `ls`) sea tapado por la UI.
- [x] **Smart Interaction:** Soporte para `Tab` y `Enter` para seleccionar sugerencias del menú.
- [x] **Native AOT Ready:** Código libre de reflexión dinámica y advertencias de nulidad (`CS8600`).
- [x] **Ghost Text:** Autocompletado visual que se consolida con `Tab` o `Flecha Derecha`.

---

## 🧠 Capas de Inteligencia (Warp-like Strategy)
Para lograr un autocompletado de nivel premium, implementaremos un **Motor de Fusión de Sugerencias** basado en 4 capas:

1.  **Capa 1: System Binaries (PATH):**
    - **Objetivo:** Sugerir ejecutables del sistema (`git`, `docker`, `dotnet`, etc.) cuando el cursor está en la primera palabra.
    - **Técnica:** Escaneo y caché de directorios del System PATH.

2.  **Capa 2: Frecency History (Smart History):**
    - **Objetivo:** Sugerir comandos completos basados en el uso real.
    - **Técnica:** Algoritmo de *Frecency* (Frecuencia + Recencia) almacenado localmente.

3.  **Capa 3: Semantic Grammar (Completions):**
    - **Objetivo:** Sugerir subcomandos y banderas (`git commit --message`) detectando el contexto del comando actual.
    - **Técnica:** Definiciones gramaticales en JSON o scripts de completado.

4.  **Capa 4: IA Prompting (Google Gemini):**
    - **Objetivo:** Resolver dudas o generar comandos complejos desde lenguaje natural usando el prefijo `#`.
    - **Técnica:** Integración directa con el API de Gemini mediante el Lexer.

---

## 📈 Próximos Pasos (Pendiente)
1.  **Mejora del Lexer (Prioridad #1):** Implementar `CommandLineParser` para soportar comillas y rutas con espacios (Vital para activar las Capas 1, 3 y 4).
2.  **Implementación de L1 (Binarios):** Crear el servicio de descubrimiento de comandos del sistema.
3.  **Persistencia de Historial (L2):** Implementar el guardado de comandos ejecutados con éxito.
4.  **Integración con Gemini (L4):** Implementar el cliente para interactuar con la IA de Google.

---

## 📝 Notas de Inspección (Sync para otra PC)
- **Renderizado:** La estabilidad visual se basa en la variable `_lastUiHeight` en `TerminalApp.cs`. No eliminarla, es la que evita los "fantasmas" del menú.
- **AOT:** Mantener el uso de `AppJsonContext` para cualquier nuevo modelo que requiera serialización JSON.
- **Lexer:** El `CommandLineParser` debe ser capaz de identificar en qué índice de argumento se encuentra el cursor para decidir qué Capa de Inteligencia activar.
