<div align="center">

<img src="https://raw.githubusercontent.com/huykhoong/esp32_dasai_mochi_clone_and_how_to/main/250frames.gif" width="120" height="120" alt="Mochi animation"/>

# MochiES 🐾

**Clone open source del Dasai Mochi escrito en C# con .NET nanoFramework**

Mostrá animaciones, recibí notificaciones y navegá con Google Maps — todo desde un ESP32 programado en C#.

[![License: MIT](https://img.shields.io/badge/Licencia-MIT-blue.svg)](LICENSE)
[![.NET nanoFramework](https://img.shields.io/badge/.NET-nanoFramework-purple)](https://nanoframework.net/)
[![Platform: ESP32](https://img.shields.io/badge/Hardware-ESP32%20C3-green)](https://docs.espressif.com/projects/esp-idf/en/latest/esp32c3/)
[![PRs Welcome](https://img.shields.io/badge/PRs-bienvenidos-brightgreen.svg)](CONTRIBUTING.md)
[![Estado](https://img.shields.io/badge/Estado-En%20desarrollo-orange)]()

[Qué es esto](#-qué-es-esto) •
[Hardware](#-hardware) •
[Instalación](#-instalación) •
[Animaciones](#-animaciones) •
[Roadmap](#-roadmap) •
[Contribuir](#-contribuir)

</div>

---

## ¿Qué es esto?

MochiES es una implementación open source del [Dasai Mochi](https://themochi.huykhong.com) — una carita animada que corre en un microcontrolador ESP32 con pantalla OLED.

A diferencia del proyecto original (escrito en C/Arduino), **MochiES está escrito completamente en C#** usando [.NET nanoFramework](https://nanoframework.net/), lo que permite programarlo con Visual Studio como cualquier aplicación .NET.

### Funcionalidades

| Feature | Estado |
|---|---|
| Animaciones en pantalla OLED SSD1306 | ✅ Disponible |
| Conversor de GIF a byte arrays (WinForms) | ✅ Disponible |
| Sensor táctil TTP223 | 🔄 En desarrollo |
| Reloj sincronizado | 🔄 En desarrollo |
| Notificaciones BLE (Android) | 📋 Planificado |
| Navegación Google Maps | 📋 Planificado |

---

## 🔧 Hardware

### Lista de componentes

| Componente | Descripción | Alternativa |
|---|---|---|
| **ESP32 C3 Mini** | Microcontrolador principal | ESP32, ESP32 S3 |
| **OLED SSD1306 128×64** | Pantalla I2C 0.96" o 1.3" | SSD1309 |
| **Cable USB Type-C** | Con soporte para datos (no solo carga) | — |
| **TTP223** *(opcional)* | Sensor táctil capacitivo | Botón pulsador |
| **TP4056 + batería LiPo** *(opcional)* | Para uso portátil con batería | — |

### Diagrama de conexión

```
ESP32 C3 Mini          OLED SSD1306
─────────────          ────────────
3.3V        ────────── VCC
GND         ────────── GND
GPIO 21     ────────── SCL
GPIO 20     ────────── SDA

                       TTP223 (opcional)
                       ─────────────────
3.3V        ────────── VCC
GND         ────────── GND
GPIO 1      ────────── OUT
```

> **Nota:** Los pines SCL/SDA son configurables desde el firmware si necesitás usar otros GPIO.

---

## 🚀 Instalación

### Prerrequisitos

- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/community/) (Community es gratuito)
- [Python 3.x](https://www.python.org/downloads/) (para flashear el ESP32)
- Extensión [.NET nanoFramework](https://marketplace.visualstudio.com/items?itemName=nanoframework.nanoFramework-VS2022-Extension) para Visual Studio

### Paso 1 — Flashear nanoFramework al ESP32

Esto instala el runtime C# en el chip. Se hace **una sola vez** por dispositivo.

```bash
pip install nanoff
nanoff --target ESP32_C3 --update --serialport COM3
```

> Reemplazá `COM3` por el puerto de tu ESP32.
> En Windows: *Administrador de dispositivos → Puertos COM*.
> En Linux/Mac: `/dev/ttyUSB0` o `/dev/cu.usbserial-...`

### Paso 2 — Clonar el repositorio

```bash
git clone https://github.com/Juanzett/MochiES.git
cd MochiES
```

### Paso 3 — Abrir en Visual Studio

1. Abrir `MochiES_Firmware/MochiES_Firmware.sln`
2. Click derecho en el proyecto → **Manage NuGet Packages**
3. Instalar los siguientes paquetes:

```
nanoFramework.Hardware.Esp32
nanoFramework.Device.I2c
Iot.Device.Ssd13xx
```

### Paso 4 — Compilar y deployar

1. Conectar el ESP32 por USB
2. En el panel **Device Explorer** de Visual Studio, seleccionar el dispositivo
3. Click en **Deploy** (`Ctrl+F5`)

El código se compila y sube al chip automáticamente. La primera vez puede tardar 1-2 minutos.

---

## 🎨 Animaciones

### Cómo funcionan

Cada animación es un array de frames (`byte[][]`). Cada frame es un bitmap de 128×64 pixels en formato SSD1306 (1024 bytes), donde cada bit representa un pixel.

```csharp
// Agregar una animación en AnimacionesMochi.cs
public static byte[][] MiAnimacion => new byte[][]
{
    new byte[] { 0x00, 0xFF, ... },  // frame 0
    new byte[] { 0x00, 0xFF, ... },  // frame 1
    // ...
};

// Reproducirla en Program.cs
ReproducirAnimacion(AnimacionesMochi.MiAnimacion, "Mi animación");
```

### Conversor de GIF

El proyecto incluye una herramienta de escritorio (**MochiES_GifConverter**) que convierte cualquier GIF animado al formato de bytes del firmware.

```
1. Abrir MochiES_GifConverter.sln en Visual Studio
2. Compilar y ejecutar
3. Cargar un GIF → Convertir → Copiar el C# generado
4. Pegar en AnimacionesMochi.cs
```

> El conversor redimensiona automáticamente cualquier GIF a 128×64 pixels.

---

## 📁 Estructura del proyecto

```
MochiES/
│
├── MochiES_Firmware/               ← Firmware ESP32 (C# + nanoFramework)
│   ├── MochiES_Firmware.sln
│   ├── Program.cs                  ← Loop principal, inicialización OLED
│   └── AnimacionesMochi.cs         ← Biblioteca de animaciones
│
├── MochiES_GifConverter/           ← Herramienta de escritorio (WinForms)
│   ├── MochiES_GifConverter.sln
│   └── FormConverter.cs            ← Convierte GIFs → byte arrays
│
├── docs/                           ← Documentación adicional
│   └── wiring_diagram.png
│
├── LICENSE
└── README.md
```

---

## 🗺️ Roadmap

- [x] Animaciones básicas en OLED SSD1306
- [x] Conversor GIF → byte arrays (WinForms)
- [ ] Soporte sensor táctil TTP223
- [ ] Reloj sincronizado con hora local
- [ ] Soporte pantalla 1.3" (SSD1309)
- [ ] Conexión BLE con app Android (Chronos)
- [ ] Notificaciones de llamadas y mensajes
- [ ] Navegación con Google Maps
- [ ] Panel web de configuración

¿Tenés una idea o feature request? [Abrí un issue](../../issues/new) 👋

---

## 🤝 Contribuir

¡Las contribuciones son bienvenidas! Si querés mejorar el proyecto:

1. Hacé un **fork** del repositorio
2. Creá una branch: `git checkout -b feature/mi-feature`
3. Commiteá los cambios: `git commit -m 'Agrego soporte para...'`
4. Pusheá la branch: `git push origin feature/mi-feature`
5. Abrí un **Pull Request**

### Ideas para contribuir

- Más animaciones (`.gif` → byte arrays listos para usar)
- Soporte para otras pantallas OLED
- Mejoras al conversor de GIF
- Documentación y ejemplos

---

## 📚 Referencias y créditos

- [The Mochi](https://themochi.huykhong.com) — Proyecto original de firmware ESP32 por [@huykhoong](https://github.com/huykhoong)
- [esp32_dasai_mochi_clone](https://github.com/huykhoong/esp32_dasai_mochi_clone_and_how_to) — Repo Arduino de referencia
- [.NET nanoFramework](https://nanoframework.net/) — Runtime C# para microcontroladores
- [nanoFramework.IoT.Device](https://github.com/nanoframework/nanoFramework.IoT.Device) — Drivers para sensores y pantallas
- [Dasai Mochi](https://dasai.com.au) — Proyecto original del personaje

---

## 📄 Licencia

Distribuido bajo la licencia **MIT**. Ver [`LICENSE`](LICENSE) para más información.

---

<div align="center">

Hecho con ❤️ en Argentina 🇦🇷

Si este proyecto te fue útil, dejale una ⭐ al repo

</div>
