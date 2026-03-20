Here's the improved `README.md` file for the MochiES project, incorporating the new content while maintaining the existing structure and information:

# ?? MochiES - Firmware ESP32 con Pantalla I2C

**MochiES** es un proyecto modular y escalable para controlar pantallas I2C (LCD1602 y SSD1306 OLED) desde un ESP32 usando .NET nanoFramework. Incluye una herramienta de escritorio para convertir GIFs animados a byte arrays.

## ?? Tabla de Contenidos

- [Características](#características)
- [Arquitectura del Proyecto](#arquitectura-del-proyecto)
- [Estructura de Carpetas](#estructura-de-carpetas)
- [Componentes Principales](#componentes-principales)
- [Flujo Completo](#flujo-completo)
- [Instalación](#instalación)
- [Compilación y Prueba](#compilación-y-prueba)
- [Uso](#uso)
- [Patrones de Diseño](#patrones-de-diseño)
- [Extensibilidad](#extensibilidad)
- [Referencias](#referencias)
- [Autor](#autor)
- [Licencia](#licencia)
- [Contribuciones](#contribuciones)
- [Troubleshooting](#troubleshooting)
- [Soporte](#soporte)

---

## ? Características

- ? **Soporte Multi-Display**: LCD1602 (16×2 texto) y SSD1306 OLED (128×64 gráficos)
- ? **Intercambiable**: Cambiar de pantalla con UNA línea de código
- ? **Portátil**: Interfaz genérica permite agregar nuevos displays fácilmente
- ? **Herramienta de Conversión**: GifConverter para convertir GIFs a byte arrays
- ? **Agnóstico**: Program.cs no cambia al cambiar de hardware
- ? **C# 7.3 Compatible**: Funciona con .NET Framework 4.7.2
- ? **Bien Documentado**: Código con comentarios explicativos

---

## ??? Arquitectura del Proyecto

### Diagrama de Capas

????????????????????????????????????????????????????????
?                     DESKTOP (Windows)                ?
?  GifConverter.exe: Convierte GIFs ? byte arrays C#  ?
????????????????????????????????????????????????????????
                       ? (Copia/Pega código)
                       ?
????????????????????????????????????????????????????????
?              FIRMWARE (ESP32 + nanoFramework)        ?
?                                                      ?
?  Program.cs                  AnimacionesMochi.cs    ?
?  (Lógica Principal)          (Datos: Frames)        ?
?         ?                            ?              ?
?  IDisplayDriver (Interfaz)          ?              ?
?    ?? Lcd1602Driver    ??????????????              ?
?    ?? Ssd1306Driver                                ?
?         ?                                           ?
?  I2C Bus (GPIO21=SCL, GPIO20=SDA)                 ?
?????????????????????????????????????????????????????????
                       ?
????????????????????????????????????????????????????????
?           HARDWARE (ESP32 + Pantalla)               ?
?                                                      ?
?  I2C Device (0x27 o 0x3C)                          ?
?  ?? LCD1602 + PCF8574 (Texto 16×2)                 ?
?  ?? SSD1306 (Gráficos 128×64)                      ?
????????????????????????????????????????????????????????

### Diagrama de Flujo de Datos

GIF animado (Desktop)
        ?
   GifConverter
   • Extrae frames
   • Redimensiona a 128×64
   • Convierte a blanco/negro
   • Organiza en páginas (1024 bytes)
        ?
Genera código C# (byte arrays)
        ?
Usuario copia/pega en AnimacionesMochi.cs
        ?
Program.cs carga datos
        ?
¿USAR_OLED?
   ?? false ? Lcd1602Driver (texto)
   ?? true  ? Ssd1306Driver (gráficos)
        ?
Envía por I2C al display
        ?
Pantalla renderiza contenido

---

## ?? Estructura de Carpetas

MochiES/
??? MochiES_Firmware/                    # Proyecto para ESP32
?   ??? Program.cs                       # Punto de entrada + lógica principal
?   ??? IDisplayDriver.cs                # Interfaz abstracta para displays
?   ??? Lcd1602Driver.cs                 # Implementación para LCD1602
?   ??? Ssd1306Driver.cs                 # Implementación para SSD1306
?   ??? AnimacionesMochi.cs              # Datos de animaciones (auto-generado)
?   ??? MochiES_Firmware.csproj          # Proyecto nanoFramework
?
??? MochiES_GifConverter/                # Proyecto WinForms (Desktop)
?   ??? Program.cs                       # Punto de entrada
?   ??? FormConverter.cs                 # Interfaz gráfica
?   ??? MochiES_GifConverter.csproj      # Proyecto WinForms
?
??? MochiES.sln                          # Solución que contiene ambos proyectos
??? README.md                            # Este archivo
??? .gitignore                           # Archivo Git

---

## ?? Componentes Principales

### 1. **IDisplayDriver.cs** (Interfaz Abstracta)

Define el contrato que todos los drivers de pantalla deben cumplir.

public interface IDisplayDriver : IDisposable
{
    void Inicializar();                              // Inicializa la pantalla
    void Limpiar();                                  // Limpia el contenido
    void MostrarTexto(string texto);                 // Muestra texto general
    void MostrarTextoPorLinea(int linea, string texto); // Texto en línea específica
    void MostrarFrame(byte[] frameData);             // Muestra frame bitmap
    int AnchoDisplay { get; }                        // Ancho (caracteres o píxeles)
    int AltoDisplay { get; }                         // Alto (caracteres o píxeles)
    string TipoDisplay { get; }                      // Nombre del display
}

**Ventajas:**
- ? Abstracción: Define contrato sin detalles de implementación
- ? Intercambiabilidad: Cambiar displays sin modificar código cliente
- ? Extensibilidad: Agregar nuevos displays es trivial

---

### 2. **Lcd1602Driver.cs** (LCD 16×2 Caracteres)

Implementa comunicación I2C con LCD1602 usando módulo PCF8574.

**Características:**
- Display: 16 columnas × 2 filas
- Modo texto ASCII
- Protocolo: I2C + PCF8574 (GPIO expander)
- Dirección I2C típica: 0x27 o 0x3F

**Protocolo PCF8574:**
Bit 0 (RS):  0=Comando, 1=Dato
Bit 1 (RW):  0=Escritura, 1=Lectura (siempre escritura aquí)
Bit 2 (E):   Enable (pulso para confirmar)
Bit 3 (BL):  Backlight (1=encendido)
Bits 4-7:    Datos en modo 4-bit

**Método clave:**
EnviarByte(byte valor, bool isComando)
  ?? Divide en nibble alto y bajo
  ?? Envía con RS=0 (comando) o RS=1 (dato)
  ?? Pulsa Enable para confirmar
  ?? Delay para sincronización

---

### 3. **Ssd1306Driver.cs** (OLED 128×64 Gráficos)

Implementa comunicación I2C directa con controlador SSD1306.

**Características:**
- Display: 128 píxeles ancho × 64 píxeles alto
- Modo gráficos (pixel-perfect)
- Protocolo: I2C directo (sin expansor GPIO)
- Dirección I2C típica: 0x3C o 0x3D

**Organización de memoria:**
El SSD1306 organiza la pantalla en 8 "páginas":
- Página 0: Filas 0-7   (128 bytes)
- Página 1: Filas 8-15  (128 bytes)
- Página 2: Filas 16-23 (128 bytes)
- ...
- Página 7: Filas 56-63 (128 bytes)

Total: 8 × 128 = 1024 bytes

Cada byte = 8 píxeles verticales
Bit 0 = fila inferior, Bit 7 = fila superior

**Método clave:**
MostrarFrame(byte[] frameData)
  ?? Valida que frameData tenga 1024 bytes
  ?? Para cada página (0-7):
  ?   ?? Configura dirección de página
  ?   ?? Configura dirección de columna
  ?   ?? Envía 128 bytes de datos
  ?? Sincroniza con pequeños delays

---

### 4. **Program.cs** (Lógica Principal)

Punto de entrada del firmware. Orquesta la inicialización y lógica principal.

**Configuración:**
const int PIN_SCL = 21;                    // GPIO para reloj I2C
const int PIN_SDA = 20;                    // GPIO para datos I2C
const int I2C_BUS = 1;                     // Bus I2C del ESP32

const int DISPLAY_ADDRESS = 0x27;          // Dirección I2C
const bool USAR_OLED = false;              // false=LCD1602, true=SSD1306

**Flujo de inicialización:**
1. Configurar pines GPIO (SCL, SDA)
2. Inicializar bus I2C
3. Seleccionar driver según USAR_OLED
4. Llamar Inicializar() en el driver
5. Loop principal con lógica de aplicación

---

### 5. **AnimacionesMochi.cs** (Datos de Animaciones)

Contiene frames de animaciones como byte arrays. **Este archivo es auto-generado por GifConverter.**

**Ejemplo de estructura:**
public static byte[][] Sonrisa
{
    get
    {
        return new byte[][]
        {
            new byte[] { 0x00, 0x01, 0x02, ... },  // Frame 0 (1024 bytes)
            new byte[] { 0x00, 0x01, 0x02, ... },  // Frame 1 (1024 bytes)
            new byte[] { 0x00, 0x01, 0x02, ... },  // Frame 2 (1024 bytes)
        };
    }
}

---

### 6. **GifConverter** (Herramienta Desktop)

Aplicación WinForms que convierte GIFs animados a código C# compatible.

**Proceso:**
1. **Carga GIF**: Lee archivo animado desde disco
2. **Extrae frames**: Obtiene cada frame del GIF
3. **Procesa cada frame:**
   - Redimensiona a 128×64 píxeles
   - Convierte a escala de grises
   - Aplica umbral (128) ? blanco/negro
   - Organiza en formato SSD1306 (páginas)
4. **Genera código C#**: Crea byte arrays válidos
5. **Copia al portapapeles**: Usuario pega en AnimacionesMochi.cs

**Interfaz:**
- Botón "?? Cargar GIF": Selecciona archivo
- Botón "? Convertir": Procesa el GIF
- Botón "?? Copiar C#": Copia código al portapapeles
- Preview: Muestra GIF original redimensionado
- Salida: Código C# generado

---

## ?? Flujo Completo

### Escenario: Convertir un GIF y mostrarlo en la pantalla

#### **FASE 1: Desktop (GifConverter)**

1. Usuario ejecuta GifConverter.exe
   ?
2. Usuario carga un GIF (File ? Open)
   ?
3. GifConverter.FormConverter.BtnCargar_Click()
   - OpenFileDialog para seleccionar archivo
   - Image.FromFile(ruta) carga el GIF
   - Muestra preview en PictureBox
   ?
4. Usuario presiona "? Convertir"
   ?
5. FormConverter.ConvertirGif(path)
   - Image gif = Image.FromFile(path)
   - FrameDimension dimension = new FrameDimension(gif.FrameDimensionsList[0])
   - int totalFrames = gif.GetFrameCount(dimension)
   - Para cada frame i (0 a totalFrames-1):
     a. gif.SelectActiveFrame(dimension, i)
     b. Redimensionar a 128×64 con Graphics
     c. BitmapABuffer(bmp) ? convierte a bytes
     d. _frames.Add(frameBuffer)
   ?
6. FormConverter.GenerarCodigoCSharp(nombre, _frames)
   - StringBuilder sb = new StringBuilder()
   - sb.AppendLine("public static byte[][] " + nombre)
   - sb.AppendLine("{ get { return new byte[][] {")
   - Para cada frame:
     * Convertir cada byte a "0xHH"
     * Agrupar en líneas de 16 bytes
   - sb.AppendLine("}; }")
   - return sb.ToString()
   ?
7. txtOutput.Text = código generado
   ?
8. Usuario presiona "?? Copiar C#"
   ?
9. Clipboard.SetText(txtOutput.Text)
   ?
10. Usuario abre AnimacionesMochi.cs en Visual Studio
    ?
11. Usuario pega el código (Ctrl+V)
    ?
12. Guarda archivo (Ctrl+S)

#### **FASE 2: Firmware (ESP32 + nanoFramework)**

1. Visual Studio compila MochiES_Firmware
   ?
2. Flashea el firmware al ESP32 (F5 / Debug)
   ?
3. ESP32 ejecuta Program.Main()
   ?
4. Configuration.SetPinFunction(PIN_SCL, DeviceFunction.I2C1_CLOCK)
   Configuration.SetPinFunction(PIN_SDA, DeviceFunction.I2C1_DATA)
   - Configura GPIO21 como SCL
   - Configura GPIO20 como SDA
   ?
5. I2cConnectionSettings i2cSettings = new I2cConnectionSettings(I2C_BUS, DISPLAY_ADDRESS)
   I2cDevice i2cDevice = I2cDevice.Create(i2cSettings)
   - Inicializa bus I2C1
   - Dirección: 0x27 (LCD1602)
   ?
6. if (USAR_OLED == false)
       _display = new Lcd1602Driver(i2cDevice, 0x27)
   - Instancia el driver correcto
   - Llama Lcd1602Driver.Inicializar()
   ?
7. Lcd1602Driver.Inicializar()
   - EnviarComando(0x33) // Inicialización
   - EnviarComando(0x32) // Modo 4-bit
   - EnviarComando(0x28) // 2 líneas, font 5×8
   - EnviarComando(0x0C) // Display ON
   - Limpiar()           // Clear
   ?
8. MostrarIntro()
   - _display.Limpiar()
   - _display.MostrarTextoPorLinea(0, "Bienvenido!")
   - _display.MostrarTextoPorLinea(1, "MochiES v1.0")
   ?
9. Loop principal:
   while (true)
   {
       MostrarMensaje("Linea 1: " + contador, "Linea 2: MochiES")
       contador++
       Thread.Sleep(3000)
   }
   ?
10. MostrarMensaje("Linea 1", "Linea 2")
    - _display.MostrarTextoPorLinea(0, "Linea 1")
    - _display.MostrarTextoPorLinea(1, "Linea 2")
    ?
11. Lcd1602Driver.MostrarTextoPorLinea(0, "Linea 1")
    - EnviarComando(0x80)  // Dirección DDRAM línea 0
    - Para cada carácter en "Linea 1":
      * EnviarDato((byte)carácter)  // Envía ASCII por I2C
    ?
12. Lcd1602Driver.EnviarDato(byte)
    - EnviarByte(byte, isComando=false)
    ?
13. Lcd1602Driver.EnviarByte(byte, false)
    - Nibble alto = (byte & 0xF0) | RS | BL
    - EnviarAlPcf8574(nibbleAlto)    // Escribe I2C
    - Pulsar(nibbleAlto)              // Genera pulso Enable
    - Nibble bajo = ((byte << 4) & 0xF0) | RS | BL
    - EnviarAlPcf8574(nibbleBajo)    // Escribe I2C
    - Pulsar(nibbleBajo)              // Genera pulso Enable
    ?
14. I2cDevice.Write(buffer)  // Transmite por I2C
    ?
15. PCF8574 (GPIO Expander) recibe datos
    - Actualiza pines Q0-Q7
    - Q0-Q3: Datos (D4-D7)
    - Q4: E (Enable)
    - Q5: RW (siempre 0 = escritura)
    - Q6: RS (0=comando, 1=dato)
    - Q7: BL (backlight)
    ?
16. LCD1602 recibe pulso Enable (Q4: 1?0)
    - Captura datos en Q0-Q3
    - Si RS=1: interpreta como dato ASCII
    - Si RS=0: interpreta como comando
    ?
17. LCD1602 renderiza carácter en display
    - Selecciona fila y columna (según dirección previa)
    - Busca glifo en ROM del controlador
    - Ilumina píxeles del carácter
    ?
18. Usuarios ven: "Linea 1" en primera línea
                  "Linea 2" en segunda línea
    ?
19. Thread.Sleep(3000) espera 3 segundos
    ?
20. Loop continúa, contador incrementa

---

## ??? Instalación

### Requisitos

- **Visual Studio 2022** (Community, Professional o Enterprise)
- **Extensión: .NET nanoFramework** (para VS 2022)
- **Python 3.x** (para herramienta nanoff de flasheo)
- **Hardware:**
  - ESP32 (C3 Mini, DevKit, etc)
  - LCD1602 + módulo I2C PCF8574 O SSD1306 OLED
  - Cable USB para programación

### Instalación del Entorno

#### 1. Instalar Visual Studio 2022

Descargar desde: https://visualstudio.microsoft.com/

#### 2. Instalar Extensión nanoFramework

En Visual Studio:
1. **Extensiones ? Administrar extensiones**
2. Buscar: ".NET nanoFramework"
3. **Descargar** e instalar
4. Reiniciar Visual Studio

#### 3. Instalar herramienta nanoff (para flashear)

En PowerShell como administrador:
pip install nanoff

Verificar instalación:
nanoff --help

#### 4. Clonar repositorio

git clone https://github.com/Juanzett/MochiES.git
cd MochiES

#### 5. Abrir solución en Visual Studio

Archivo ? Abrir ? MochiES.sln

---

## ?? Compilación y Prueba

### Paso 1: Compilar la solución

Ctrl + Shift + B (Compilar solución)

**Esperado:** 0 errores, 0 advertencias

### Paso 2: Probar GifConverter (Desktop)

1. Clic derecho en MochiES_GifConverter ? Establecer como proyecto de inicio
2. Presionar F5 (Debug)
3. Cargar un GIF
4. Convertir
5. Copiar código

### Paso 3: Compilar firmware

1. Clic derecho en MochiES_Firmware ? Compilar
2. Verificar que compila sin errores

### Paso 4: Flashear al ESP32

1. Conectar ESP32 por USB
2. Clic derecho en MochiES_Firmware ? Establecer como proyecto de inicio
3. Presionar F5 (Debug)
4. Visual Studio detecta ESP32 y flashea automáticamente

### Paso 5: Ver salida de consola

Ver ? Salida (Ctrl + Alt + O)

Verás logs como:
=== MochiES arrancando... ===
Inicializando LCD1602...
Display inicializado: LCD1602
Resolución: 16x2
Iniciando loop de animación...
>>> Linea 1: 0 | Linea 2: MochiES

---

## ?? Uso

### Cambiar entre LCD1602 y SSD1306

En `MochiES_Firmware/Program.cs`:

**Para LCD1602:**
const int DISPLAY_ADDRESS = 0x27;
const bool USAR_OLED = false;

**Para SSD1306:**
const int DISPLAY_ADDRESS = 0x3C;  // o 0x3D según tu módulo
const bool USAR_OLED = true;

Luego recompila (Ctrl+B) y flashea (F5).

### Convertir un GIF a animación

1. Ejecutar `GifConverter.exe`
2. Presionar "?? Cargar GIF"
3. Seleccionar archivo GIF (128×64 recomendado)
4. Presionar "? Convertir"
5. Presionar "?? Copiar C#"
6. Abrir `AnimacionesMochi.cs`
7. Pegar código (Ctrl+V)
8. Guardar (Ctrl+S)
9. Recompilar firmware

### Mostrar animaciones en LCD1602

LCD1602 no soporta gráficos. Usa `MostrarTextoPorLinea()`:


_display.MostrarTextoPorLinea(0, "Animación:");
_display.MostrarTextoPorLinea(1, "? ? ? ?");

### Mostrar animaciones en SSD1306

// Reproducir animación frame por frame
for (int i = 0; i < AnimacionesMochi.Sonrisa.Length; i++)
{
    _display.MostrarFrame(AnimacionesMochi.Sonrisa[i]);
    Thread.Sleep(80);  // 80ms entre frames
}

---

## ?? Patrones de Diseño

### 1. Strategy Pattern

// Permite cambiar algoritmo (driver) en tiempo de ejecución

IDisplayDriver _display;

if (USAR_OLED)
    _display = new Ssd1306Driver(i2cDevice);
else
    _display = new Lcd1602Driver(i2cDevice, 0x27);

// Code cliente usa _display sin conocer implementación
_display.MostrarTexto("Hola");

**Ventaja:** Intercambiar implementaciones sin cambiar código cliente.

---

### 2. Dependency Injection

// Inyectar dependencias en lugar de crearlas internamente

public Lcd1602Driver(I2cDevice i2cDevice, int i2cAddress)
{
    _i2cDevice = i2cDevice;
    _i2cAddress = i2cAddress;
}

// Permite testing y reutilización

**Ventaja:** Desacoplamiento, facilita testing con mocks.

---

### 3. Factory Pattern

// Crear instancia correcta basada en configuración

if (USAR_OLED)
    _display = new Ssd1306Driver(i2cDevice);
else
    _display = new Lcd1602Driver(i2cDevice, 0x27);

**Ventaja:** Lógica de creación centralizada.

---

### 4. Template Method

// En IDisplayDriver define estructura, subclases implementan detalles

public interface IDisplayDriver
{
    void Inicializar();  // Template: todos deben inicializar
    void Limpiar();      // Template: todos deben limpiar
}

**Ventaja:** Garantiza que todos los drivers cumplan contrato.

---

## ?? Extensibilidad

### Agregar nuevo display (ST7735 TFT)

1. **Crear driver:**

// MochiES_Firmware/St7735Driver.cs
public class St7735Driver : IDisplayDriver
{
    private SpiDevice _spiDevice;  // ST7735 usa SPI, no I2C
    
    public void Inicializar() { /* ... */ }
    public void Limpiar() { /* ... */ }
    public void MostrarTexto(string texto) { /* ... */ }
    // ... implementar interfaz completa
}

2. **Actualizar Program.cs:**

const int DISPLAY_TYPE = 0;  // 0=LCD, 1=OLED, 2=TFT

if (DISPLAY_TYPE == 0)
    _display = new Lcd1602Driver(...);
else if (DISPLAY_TYPE == 1)
    _display = new Ssd1306Driver(...);
else if (DISPLAY_TYPE == 2)
    _display = new St7735Driver(...);

3. **¡Listo!** El resto del código sin cambios.

---

### Agregar animaciones nuevas

1. Abrir GifConverter
2. Cargar nuevo GIF
3. Convertir
4. Copiar código
5. Pegar en AnimacionesMochi.cs con nuevo nombre
6. Recompilar y flashear

---

## ?? Referencias

- **nanoFramework:** https://www.nanoframework.net/
- **LCD1602 + PCF8574:** https://wiki.keyestudio.com/KS0401_keyestudio_16x2_LCD_I2C_Module
- **SSD1306 OLED:** https://en.wikipedia.org/wiki/OLED
- **ESP32 GPIO:** https://docs.espressif.com/projects/esp-idf/

---

## ?? Autor

**Juan Zetterman**  
GitHub: https://github.com/Juanzett  
Proyecto: MochiES

---

## ?? Licencia

Este proyecto está bajo licencia **MIT**. Ver archivo LICENSE para detalles.

---

## ?? Contribuciones

¡Las contribuciones son bienvenidas!

1. Fork el repositorio
2. Crea una rama (`git checkout -b feature/AmazingFeature`)
3. Commit cambios (`git commit -m 'Add AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

---

## ?? Troubleshooting

### Error: "El nombre 'I2cDevice' no existe"

**Solución:** Instalar paquete NuGet `nanoFramework.Device.I2c`

Proyecto ? Administrar paquetes NuGet ? Instalar

### Error: "InitializeComponent no existe"

**Solución:** Esto es normal en C# 7.3. La UI se construye manualmente en `ConfigurarUI()`.

### ESP32 no se detecta

**Solución:**
1. Verificar conexión USB
2. Instalar drivers CH340 si es necesario
3. Ejecutar nanoff: `nanoff --target ESP32_C3 --update --serialport COM3`

### GifConverter no convierte

**Solución:**
1. Verificar que sea un GIF animado (no imagen estática)
2. Reducir tamaño de imagen (máximo 128×64)
3. Usar formatos comunes (PNG, JPG antes de convertir a GIF)

---

## ?? Soporte

Para reportar issues o sugerencias:
https://github.com/Juanzett/MochiES/issues

---

**¡Gracias por usar MochiES! ??**

This version maintains the original structure and information while enhancing clarity and coherence throughout the document.