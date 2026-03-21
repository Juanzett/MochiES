// ============================================================
//  INSTRUCCIONES.cs
//  Archivo de contexto compartido entre Claude y GitHub Copilot
//
//  Proyecto : MochiES
//  Autor    : Juan Nuñez
//  GitHub   : https://github.com/Juanzett
//  Repo     : https://github.com/Juanzett/MochiES
//
//  PROPÓSITO:
//  Este archivo es un proxy de contexto para IAs.
//  Antes de generar o sugerir cualquier código, toda IA
//  (Claude o Copilot) debe leerlo completo.
//  Contiene la visión, estado actual, convenciones, errores
//  conocidos y el plan técnico completo del proyecto.
// ============================================================

// ════════════════════════════════════════════════════════════
//  VISIÓN CENTRAL
// ════════════════════════════════════════════════════════════
//
//  MochiES es un TINY DESK COMPANION — un asistente de
//  escritorio compacto basado en ESP32 programado en C# con
//  .NET nanoFramework.
//
//  IDEA CENTRAL (palabras de Juan):
//  "Generar un asistente que se conecte por BLE al celular
//  Android y que podamos ver notificaciones, el nombre de la
//  canción que escuchamos, la hora y el clima. Tiene una IA
//  integrada liviana para consultas, TTS para responder,
//  micrófono para recibir preguntas por voz. Dependiendo de
//  la pantalla disponemos GIF o solo texto. Versiones futuras
//  con ruedas para moverse."
//
//  FOCO ACTUAL:
//  Hacer funcionar y testear el hardware en la protoboard.
//  Programar primero, ensamblar después, testear parte por parte.
//  No avanzar a BLE/voz hasta que WiFi + display estén estables.

// ════════════════════════════════════════════════════════════
//  HARDWARE DISPONIBLE Y ESTADO
// ════════════════════════════════════════════════════════════
//
//  ✅ EN MANO — disponible para programar y testear ahora:
//  · ESP32-S NodeMcu 38 pines  (Kit Hemmel TEK-002)
//  · LCD1602 con módulo I2C PCF8574 — display activo actual
//  · Protoboard 830 puntos (2 mitades de 400 + rieles)
//  · Jumpers macho-macho (65) y macho-hembra (10)
//  · 3 botones pulsadores 6mm del kit — navegación ↑ ↓ ✓
//  · Motor 28BYJ-48 + Driver ULN2003 — movilidad (futuro)
//  · Buzzer activo — alertas simples
//  · Speaker 35mm 4Ω / 8Ω — listo para TTS (necesita MAX98357A)
//  · Sensor táctil TTP223 — alternativa a los 3 botones
//  · Módulo RC522 RFID, Joystick, Teclado 4x4, DHT11,
//    RTC DS1307, Sensor nivel agua, Módulo RGB, Relay,
//    Servomotor 9G, Micrófono LM393, Control IR, Display 7seg,
//    Matrix LED 8x8, LM35DZ, 74HC595, Potenciómetro 10K
//
//  📦 EN CAMINO — pedido realizado, llegando pronto:
//  · SSD1306 OLED 128x64 I2C — segundo display gráfico
//    IMPORTANTE: cuando llegue, usar Iot.Device.Ssd13xx NuGet
//    junto con el Ssd1306Driver.cs propio ya implementado
//
//  🛒 POR COMPRAR — necesarios para fases futuras:
//  · INMP441 (~$3 USD) — micrófono I2S digital para voz
//  · MAX98357A (~$3 USD) — amplificador I2S para speaker
//
//  CONEXIÓN ACTIVA EN LA PROTOBOARD:
//  LCD1602 I2C:
//    VCC → Riel + (3.3V desde ESP32)
//    GND → Riel − (GND desde ESP32)
//    SDA → GPIO 20
//    SCL → GPIO 21
//    Dirección I2C: 0x27 (probar 0x3F si no responde)
//
//  3 BOTONES (pines configurables en Program.cs):
//    BTN_ARRIBA  → GPIO 34 → GND (pull-up interno ESP32)
//    BTN_ABAJO   → GPIO 35 → GND (pull-up interno ESP32)
//    BTN_SELECT  → GPIO 32 → GND (pull-up interno ESP32)
//    NO necesitan resistencias externas — pull-up del ESP32

// ════════════════════════════════════════════════════════════
//  ESTRUCTURA DE LA SOLUCIÓN (3 proyectos en MochiES.sln)
// ════════════════════════════════════════════════════════════
//
//  MochiES_Firmware  (nanoFramework — corre en el ESP32)
//  ├── Program.cs                  Loop principal + selección display + menú
//  ├── Drivers/
//  │   ├── IDisplayDriver.cs       Interfaz común ✅ IMPLEMENTADO
//  │   ├── Lcd1602Driver.cs        Driver LCD1602 ✅ IMPLEMENTADO
//  │   └── Ssd1306Driver.cs        Driver OLED I2C directo ✅ IMPLEMENTADO
//  ├── Conectividad/
//  │   ├── WifiManager.cs          📋 M1 — POR PROGRAMAR
//  │   └── BleManager.cs           📋 M3 — POR PROGRAMAR
//  ├── Servicios/
//  │   ├── RelojManager.cs         📋 M1 — POR PROGRAMAR
//  │   ├── ClimaManager.cs         📋 M2 — POR PROGRAMAR
//  │   └── IaManager.cs            📋 M4 — POR PROGRAMAR
//  ├── Audio/
//  │   ├── MicManager.cs           📋 M5 — espera INMP441
//  │   └── SpeakerManager.cs       📋 M6 — espera MAX98357A
//  ├── UI/
//  │   ├── MenuManager.cs          📋 M7 — POR PROGRAMAR
//  │   ├── ScrollManager.cs        📋 M7 — POR PROGRAMAR
//  │   └── BotonesManager.cs       📋 M7 — POR PROGRAMAR
//  ├── AnimacionesMochi.cs         Frames SSD1306 ✅ IMPLEMENTADO
//  └── AnimacionesLcd.cs           Custom chars LCD ✅ IMPLEMENTADO
//
//  MochiES_Configurador  (WinForms .NET 6+ — corre en la PC)
//  ├── FormFlasher.cs              Flashea nanoFW y firmware ✅ IMPLEMENTADO
//  └── Program.cs                  Abre FormFlasher ✅ IMPLEMENTADO
//
//  MochiES_GifConverter  (WinForms .NET 6+ — corre en la PC)
//  ├── FormConverter.cs            GIF → byte arrays C# ✅ IMPLEMENTADO
//  └── Program.cs                  Punto de entrada ✅ IMPLEMENTADO

// ════════════════════════════════════════════════════════════
//  ORDEN DE PROGRAMACIÓN (programar primero, testear después)
// ════════════════════════════════════════════════════════════
//
//  M1  WifiManager.cs + RelojManager.cs
//      Conectar WiFi, sincronizar hora SNTP, mostrar en LCD fila 0
//      NuGets: Windows.Devices.WiFi + nanoFramework.Networking.Sntp
//      Test: hora cambiando en tiempo real en el LCD
//
//  M2  ClimaManager.cs
//      HTTP GET a OpenWeatherMap (GRATIS — 1000 req/día)
//      Mostrar temperatura en LCD fila 1
//      NuGet: System.Net.Http + nanoFramework.Json
//      Test: temperatura de Buenos Aires en fila 1 del LCD
//
//  M3  BleManager.cs
//      Servidor BLE Nordic SPP — recibe strings del celular Android
//      Mostrar mensajes recibidos en el LCD
//      NuGet: nanoFramework.Device.Bluetooth
//      IMPORTANTE: necesita firmware con BLE habilitado
//      Test: app "Bluetooth Terminal" Android conectada, enviar texto
//
//  M4  IaManager.cs
//      HTTP POST a Qwen API (GRATIS con registro en Alibaba)
//      Usuario envía pregunta por BLE → ESP32 consulta IA → muestra resp
//      NuGet: System.Net.Http (ya instalado en M2)
//      Test: enviar "qué hora es?" por BLE, ver respuesta en LCD
//
//  M5  ScrollManager.cs + MenuManager.cs + BotonesManager.cs
//      3 botones (↑ ↓ ✓) para navegar entre modos del asistente
//      Modos: Reloj | Clima | IA | Notificaciones
//      Scroll horizontal para texto largo en LCD
//      NuGets: ninguno extra (GPIO + ADC del ESP32)
//      Test: navegar entre modos con los botones físicos
//
//  M6  MicManager.cs + WhisperManager.cs
//      ESPERA llegada del INMP441
//      Grabar audio I2S, POST a OpenAI Whisper, recibir transcripción
//      NuGet: System.Device.I2s
//
//  M7  SpeakerManager.cs + TtsManager.cs
//      ESPERA llegada del MAX98357A
//      POST a API TTS, recibir audio WAV, reproducir por speaker I2S
//      NuGet: System.Device.I2s (mismo que M6)
//
//  M8  MotorManager.cs (futuro)
//      Motor paso a paso 28BYJ-48 + ULN2003 para movilidad
//      4 pines GPIO con secuencia de 4 fases

// ════════════════════════════════════════════════════════════
//  APIS EXTERNAS — TODAS GRATUITAS
// ════════════════════════════════════════════════════════════
//
//  OpenWeatherMap  → clima actual
//  URL: api.openweathermap.org/data/2.5/weather?q=BuenosAires,AR
//  Tier gratuito: 1.000 llamadas/día — más que suficiente
//  Registro: openweathermap.org → API keys
//
//  Qwen (Alibaba)  → IA — respuestas de texto
//  URL: dashscope.aliyuncs.com/api/v1/services/aigc/text-generation
//  Tier gratuito: generoso con registro
//  Registro: dashscope.aliyuncs.com
//
//  pool.ntp.org  → sincronización de hora
//  Siempre gratuito — sin registro — sin API key
//
//  OpenAI Whisper  → transcripción de voz (OPCIONAL)
//  Costo: $0.006/minuto — MUY bajo, pero NO es gratis
//  Alternativa gratuita: correr Whisper local en la PC como servidor
//  y que el ESP32 envíe el audio a la PC por HTTP
//
//  OpenAI TTS  → respuesta hablada (OPCIONAL)
//  Costo: $0.015/1000 caracteres
//  Alternativa gratuita: Google Cloud TTS — 1 millón chars/mes gratis
//
//  POLÍTICA DEL PROYECTO:
//  MochiES siempre va a mantener una ruta 100% gratuita.
//  Whisper y TTS son opcionales — el asistente funciona
//  perfectamente sin voz usando solo texto y BLE.

// ════════════════════════════════════════════════════════════
//  NUGETS DEL FIRMWARE — LISTA COMPLETA
// ════════════════════════════════════════════════════════════
//
//  ✅ YA INSTALADOS:
//  nanoFramework.Hardware.Esp32     GPIO, ADC, PWM, I2S, config de pines
//  nanoFramework.Device.I2c         Comunicación con LCD1602 y SSD1306
//
//  📋 AGREGAR EN M1:
//  Windows.Devices.WiFi             Conexión a red WiFi
//  nanoFramework.System.Net         TCP/IP, sockets, DNS
//  System.Net.Http                  HTTP GET / POST a APIs externas
//  nanoFramework.Networking.Sntp    Sincronización de hora por internet
//
//  📋 AGREGAR EN M2:
//  nanoFramework.Json               Parsear respuestas JSON de las APIs
//
//  📋 AGREGAR EN M3:
//  nanoFramework.Device.Bluetooth   BLE — recibir datos del celular Android
//
//  📋 AGREGAR EN M6/M7 (cuando llegue hardware):
//  System.Device.I2s                Grabar micrófono y reproducir audio
//
//  ⚠️  SOBRE Iot.Device.Ssd13xx:
//  Este NuGet SE PUEDE usar en el proyecto SSD1306 cuando llegue
//  la pantalla OLED, pero tener en cuenta que su API puede diferir
//  entre versiones. El proyecto ya tiene Ssd1306Driver.cs propio
//  escrito sobre I2C puro que NO depende de este NuGet.
//  Usar el driver propio primero — agregar Iot.Device.Ssd13xx
//  solo si se necesita alguna funcionalidad específica de la librería.

// ════════════════════════════════════════════════════════════
//  CONVENCIONES OBLIGATORIAS
// ════════════════════════════════════════════════════════════
//
//  1. SELECCIÓN DE DISPLAY — siempre static readonly enum:
//     ✅ static readonly DisplayTarget DISPLAY_TARGET = DisplayTarget.Lcd1602;
//     ❌ const bool USAR_OLED = false;       → genera CS0162
//     ❌ const string DISPLAY_TARGET = "..."; → genera CS0162
//     RAZÓN: const se evalúa en compilación → rama else = código muerto
//
//  2. SINTAXIS C# 7.3 en el firmware (nanoFramework NO soporta C# 8+):
//     ❌ using var x = new Y();     → C# 8.0+ solamente
//     ✅ var x = new Y();
//     ❌ x?.metodo()               → cuidado en loops críticos
//     ✅ if (x != null) x.metodo();
//     ❌ public int Prop => valor;  → en interfaces puede dar problemas
//     ✅ public int Prop { get { return valor; } }
//
//  3. DRIVER OLED — usar Ssd1306Driver.cs propio PRIMERO:
//     Los métodos DrawPoint, BasicFont y Ssd13xxDisplayResolution
//     NO existen en la versión nanoFramework de Iot.Device.Ssd13xx.
//     Usar siempre Ssd1306Driver.cs del proyecto (I2C puro).
//
//  4. NUGETS mínimos del firmware:
//     nanoFramework.Hardware.Esp32 + nanoFramework.Device.I2c
//     No agregar librerías innecesarias.
//
//  5. UI en WinForms — sin InitializeComponent():
//     La UI se construye manualmente en ConfigurarUI() o similar.
//     NO llamar InitializeComponent() — el diseñador de VS no se usa.
//
//  6. NOMBRES en español:
//     Métodos, variables, comentarios → en español
//     Ejemplos: MostrarTexto(), CargarFrame(), ReproducirAnimacion()
//
//  7. GITIGNORE — siempre excluir:
//     .vs/, bin/, obj/, *.vsidx, *.suo, *.user
//     Si .vs/ ya está trackeado:
//       git rm -r --cached .vs/
//       git commit -m "Eliminar .vs/ del tracking"

// ════════════════════════════════════════════════════════════
//  ERRORES CONOCIDOS Y SOLUCIONES
// ════════════════════════════════════════════════════════════
//
//  CS0162 — "Se detectó código inalcanzable"
//  CAUSA: const bool / const string para seleccionar display
//  FIX:   static readonly DisplayTarget DISPLAY_TARGET = DisplayTarget.Lcd1602;
//
//  'DrawPoint' / 'BasicFont' / 'Ssd13xxDisplayResolution' no existe
//  CAUSA: Iot.Device.Ssd13xx tiene API incompatible con nanoFramework
//  FIX:   Usar Ssd1306Driver.cs propio — no depende de esa librería
//
//  'InitializeComponent' no existe
//  CAUSA: UI construida manualmente — no hay designer de VS
//  FIX:   No llamar InitializeComponent(). La UI está en ConfigurarUI().
//
//  using var — C# 7.3 no soportado
//  CAUSA: nanoFramework usa C# 7.3. La sintaxis es de C# 8.0+
//  FIX:   var x = new Y();  (sin using delante)
//
//  Permission denied al commitear (*.vsidx)
//  CAUSA: Visual Studio bloquea .vs/ con acceso exclusivo
//  FIX:   1) Agregar .vs/ al .gitignore
//         2) git rm -r --cached .vs/
//         3) git commit -m "Eliminar .vs/ del tracking"
//
//  LCD no muestra nada
//  CAUSA: Dirección I2C incorrecta (0x27 vs 0x3F)
//  FIX:   Probar DISPLAY_ADDRESS = 0x3F si 0x27 no funciona
//
//  ESP32 no detectado en Visual Studio
//  CAUSA: Driver CH340 no instalado o nanoFramework no flasheado
//  FIX:   1) Instalar driver CH340
//         2) pip install nanoff
//         3) nanoff --target ESP32_WROVER_KIT --update --serialport COM3
//
//  nanoff no encontrado en el Configurador
//  CAUSA: Python o nanoff no están en el PATH del sistema
//  FIX:   pip install nanoff — verificar que Python esté en el PATH

// ════════════════════════════════════════════════════════════
//  REFERENCIAS DEL PROYECTO
// ════════════════════════════════════════════════════════════
//
//  Proyectos que inspiraron MochiES (dar crédito siempre):
//
//  huykhoong/esp32_dasai_mochi_clone_and_how_to
//  https://github.com/huykhoong/esp32_dasai_mochi_clone_and_how_to
//  → Lógica de animaciones y concepto del Mochi
//
//  78/xiaozhi-esp32
//  https://github.com/78/xiaozhi-esp32
//  → Referencia de arquitectura para IA, voz y conectividad
//
//  The Mochi (firmware original)
//  https://themochi.huykhong.com
//  → Firmware avanzado de referencia
//
//  .NET nanoFramework
//  https://nanoframework.net/
//  → Runtime C# para microcontroladores — base del proyecto
//
//  ESTADO ACTUAL POR MÓDULO:
//
//  Módulo  │ Estado     │ % Progreso │ Bloqueado por
//  ────────┼────────────┼────────────┼─────────────────
//  M0      │ ✅ Done    │ 100%       │ N/A
//  M1      │ 📋 Todo    │ 0%         │ Nada (empezar aquí)
//  M2      │ 📋 Todo    │ 0%         │ M1 debe completarse
//  M3      │ 📋 Todo    │ 0%         │ M1 debe completarse
//  M4      │ 📋 Todo    │ 0%         │ M3 debe completarse
//  M5      │ 📋 Todo    │ 0%         │ M1, M2, M3
//  M6      │ 🛒 Blocked │ 0%         │ INMP441 llegue
//  M7      │ 🛒 Blocked │ 0%         │ MAX98357A llegue
//  M8      │ 📋 Future  │ 0%         │ Motor 28BYJ-48
// ════════════════════════════════════════════════════════════
//  COMANDOS CLI FRECUENTES
// ════════════════════════════════════════════════════════════
//
//  Flashear nanoFramework (primera vez):
//  nanoff --target ESP32_WROVER_KIT --update --serialport COM3
//
//  Limpiar carpeta bin/obj:
//  git rm -r --cached bin obj .vs
//  git commit -m "Limpiar artefactos"
//
//  Ver estado de rama:
//  git status
//  git log --oneline -10
//
//  Compilar firmware desde CLI:
//  dotnet build MochiES_Firmware/MochiES_Firmware.csproj
//
// ════════════════════════════════════════════════════════════
//  INSTRUCCIONES PARA LA IA QUE LEE ESTE ARCHIVO
// ════════════════════════════════════════════════════════════
//
//  SI SOS GITHUB COPILOT leyendo este archivo:
//  · El firmware usa C# 7.3 — no sugieras sintaxis 8.0+
//  · No sugieras InitializeComponent() en proyectos WinForms
//  · Usar static readonly enum para selección de display
//  · Los nombres de métodos van en español
//  · No sugieras Iot.Device.Ssd13xx para el driver OLED
//    a menos que el SSD1306 físico ya haya llegado Y se esté
//    agregando funcionalidad específica de esa librería
//  · El display activo HOY es LCD1602 en la protoboard
//  · Los botones son 3 pulsadores físicos (↑ ↓ ✓), NO joystick
//  · Todas las APIs deben tener un tier gratuito — no sugerir
//    integraciones que requieran pago obligatorio
//
//  SI SOS CLAUDE leyendo este archivo:
//  · El autor es Juan Nuñez — github.com/Juanzett
//  · Hardware activo: LCD1602 en protoboard con ESP32-S (Hemmel)
//  · Hardware en camino: SSD1306 OLED (ya comprado)
//  · Hardware por comprar: INMP441 + MAX98357A (para voz)
//  · El siguiente módulo a programar es M1: WifiManager.cs
//  · Programar primero → ensamblar → testear parte por parte
//  · No avanzar a BLE/voz hasta que WiFi + display estén estables
//  · El proyecto es 100% open source y gratuito — mantenerlo así
//  · Respetar la estructura de 3 proyectos en la solución
//  · El GifConverter puede tener un nombre más específico en el futuro

namespace MochiES
{
    /// <summary>
    /// Clase de marcador — este archivo no contiene lógica ejecutable.
    /// Su único propósito es ser leído por Claude y GitHub Copilot
    /// como contexto completo del proyecto MochiES.
    ///
    /// Mantener actualizado manualmente a medida que el proyecto evoluciona.
    /// </summary>
    internal static class Instrucciones
    {
        // Estado actual del proyecto:
        // · Fase 1 completada — firmware base + drivers + herramientas PC
        // · SSD1306 OLED comprado y en camino
        // · Próximo paso: programar M1 (WifiManager + RelojManager)
        // · Display activo en protoboard: LCD1602 (0x27)
        // · Navegación: 3 botones físicos ↑ ↓ ✓ (GPIO 34, 35, 32)
    }
}
