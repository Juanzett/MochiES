<div align="center">

<img src="https://raw.githubusercontent.com/huykhoong/esp32_dasai_mochi_clone_and_how_to/main/250frames.gif" width="120" height="120" alt="Mochi animation"/>

# MochiES 🐾

**Asistente de escritorio open source para ESP32, programado en C# con .NET nanoFramework**

Mostrá animaciones, recibí notificaciones del celular, consultá la hora y el clima, y chateá con IA — todo desde un ESP32 en tu escritorio, programado en C# como cualquier app .NET.

[![License: MIT](https://img.shields.io/badge/Licencia-MIT-blue.svg)](LICENSE)
[![.NET nanoFramework](https://img.shields.io/badge/.NET-nanoFramework-purple)](https://nanoframework.net/)
[![Platform: ESP32](https://img.shields.io/badge/Hardware-ESP32-green)](https://docs.espressif.com/projects/esp-idf/)
[![PRs Welcome](https://img.shields.io/badge/PRs-bienvenidos-brightgreen.svg)](CONTRIBUTING.md)
[![Gratuito](https://img.shields.io/badge/100%25-Gratuito-success)]()
[![Estado](https://img.shields.io/badge/Estado-En%20desarrollo-orange)]()

[Qué es esto](#-qué-es-esto) •
[Hardware](#-hardware) •
[Instalación](#-instalación) •
[Roadmap](#-roadmap) •
[Referencias](#-referencias-y-créditos) •
[Contribuir](#-contribuir)

</div>

---

## ¿Qué es esto?

MochiES es un **tiny desk companion** (asistente de escritorio compacto) basado en ESP32. Arrancó como un clon del [Dasai Mochi](https://themochi.huykhong.com) — una carita animada en una pantalla OLED — y evolucionó hacia un asistente completo que se conecta al celular Android por BLE, muestra notificaciones, hora, clima, y puede responder preguntas usando IA.

**Todo el firmware está escrito en C#** usando [.NET nanoFramework](https://nanoframework.net/), lo que te permite programarlo desde Visual Studio 2022 como cualquier proyecto .NET — sin Arduino, sin C++.

> 💡 Este proyecto es **100% gratuito y open source**. Todas las integraciones usan APIs con tier gratuito. La idea es que cualquier desarrollador pueda usarlo como base, aprender de él y mejorarlo.

---

## ✨ Funcionalidades

| Feature | Estado | Notas |
|---|---|---|
| Display LCD1602 (texto 16×2) | ✅ Listo | Driver propio I2C — sin dependencias externas |
| Display SSD1306 OLED (128×64) | ✅ Listo | Driver I2C directo — sin dependencias externas |
| Animaciones custom chars (LCD) | ✅ Listo | |
| Animaciones GIF en OLED | ✅ Listo | GifConverter incluido |
| Herramienta GifConverter (WinForms) | ✅ Listo | GIF → byte arrays C# |
| Configurador con flasheo (WinForms) | ✅ Listo | Flashea nanoFW y firmware desde la PC |
| Navegación con 3 botones (↑ ↓ ✓) | 🔄 En desarrollo | Compacto — ideal para tiny desk |
| Sensor táctil TTP223 (alternativo) | 🔄 En desarrollo | Reemplaza los botones físicos |
| WiFi + hora sincronizada (SNTP) | 🔄 En desarrollo | |
| Clima en tiempo real | 🔄 En desarrollo | OpenWeatherMap — gratis |
| Notificaciones BLE desde Android | 📋 Planificado | |
| IA integrada por texto | 📋 Planificado | Qwen — gratis |
| Reconocimiento de voz (Whisper) | 📋 Planificado | Requiere módulo INMP441 |
| Respuesta por voz (TTS) | 📋 Planificado | Requiere MAX98357A + speaker |
| App Android | 📋 Planificado | Conexión BLE al asistente |

---

## 🔧 Hardware

### Lo que necesitás para empezar

| Componente | Para qué | Alternativa |
|---|---|---|
| **ESP32** (cualquier variante) | Microcontrolador principal | ESP32-S, ESP32-S3, ESP32-C3 |
| **LCD1602 con módulo I2C** | Display de texto 16×2 | SSD1306 OLED |
| **3 botones pulsadores 6mm** | Navegar menús (↑ ↓ ✓) | Sensor táctil TTP223 |
| **Cable USB** (con soporte de datos) | Programar y alimentar | — |
| **Protoboard** | Prototipado sin soldadura | — |
| **Jumpers** | Cables de conexión | — |

### Opcional para funcionalidades avanzadas

| Componente | Para qué | Costo aprox |
|---|---|---|
| **INMP441** | Micrófono I2S — reconocimiento de voz | ~$3 USD |
| **MAX98357A** | Amplificador I2S — reproducir audio TTS | ~$3 USD |
| **Speaker 35mm 4Ω / 8Ω** | Salida de audio | ~$2 USD |
| **SSD1306 OLED 128×64** | Display gráfico con GIFs animados | ~$4 USD |

### Conexión LCD1602 + 3 botones

```
ESP32                  LCD1602 (módulo I2C)
─────────────          ────────────────────
3.3V        ────────── VCC
GND         ────────── GND
GPIO 20     ────────── SDA
GPIO 21     ────────── SCL

ESP32                  Botones pulsadores
─────────────          ─────────────────────────────────
GPIO 34     ────────── Botón ARRIBA  (↑) → otro pin a GND
GPIO 35     ────────── Botón ABAJO   (↓) → otro pin a GND
GPIO 32     ────────── Botón SELECT  (✓) → otro pin a GND
```

> Los pines usan pull-up interno del ESP32 — no necesitás resistencias externas.
> La dirección I2C del LCD puede ser `0x27` o `0x3F` según el módulo. Probar `0x27` primero.

---

## 🚀 Instalación

### Prerrequisitos

- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/community/) — Community Edition (gratuito)
- [Python 3.x](https://www.python.org/downloads/) — para flashear el ESP32
- Extensión [.NET nanoFramework](https://marketplace.visualstudio.com/items?itemName=nanoframework.nanoFramework-VS2022-Extension) para Visual Studio

### Paso 1 — Flashear .NET nanoFramework al ESP32

Esto instala el runtime C# en el chip. Se hace **una sola vez** por dispositivo.

```bash
pip install nanoff

# ESP32 genérico / NodeMcu ESP-32S:
nanoff --target ESP32_WROVER_KIT --update --serialport COM3

# ESP32 C3 Mini:
nanoff --target ESP32_C3 --update --serialport COM3
```

> Reemplazá `COM3` por el puerto real de tu ESP32.
> En Windows: *Administrador de dispositivos → Puertos COM*.

### Paso 2 — Clonar el repositorio

```bash
git clone https://github.com/Juanzett/MochiES.git
cd MochiES
```

### Paso 3 — Abrir en Visual Studio

1. Abrir `MochiES.sln` — contiene los 3 proyectos
2. Click derecho en **MochiES_Firmware** → Manage NuGet Packages
3. Instalar únicamente:

```
nanoFramework.Hardware.Esp32
nanoFramework.Device.I2c
```

> ⚠️ **No instalar** `Iot.Device.Ssd13xx`. El driver SSD1306 está incluido en el proyecto y es incompatible con esa librería en nanoFramework.

### Paso 4 — Configurar el display y los botones

Editar en `MochiES_Firmware/Program.cs`:

```csharp
// Selección de display — usar static readonly enum, NO const
static readonly DisplayTarget DISPLAY_TARGET = DisplayTarget.Lcd1602;
static readonly int DISPLAY_ADDRESS = 0x27; // probar 0x3F si no muestra nada

// Pines de los botones de navegación
const int PIN_BTN_ARRIBA = 34;
const int PIN_BTN_ABAJO  = 35;
const int PIN_BTN_SELECT = 32;
```

### Paso 5 — Compilar y deployar

1. Conectar el ESP32 por USB
2. En **Device Explorer** de Visual Studio, seleccionar el dispositivo
3. Click en **Deploy** (`Ctrl+F5`)

---

## 📁 Estructura del proyecto

```
MochiES/
├── MochiES_Firmware/               ← Firmware ESP32 (C# + .NET nanoFramework)
│   ├── Drivers/
│   │   ├── IDisplayDriver.cs       ← Interfaz común para todos los displays
│   │   ├── Lcd1602Driver.cs        ← Driver LCD1602 + PCF8574
│   │   └── Ssd1306Driver.cs        ← Driver OLED — I2C directo, sin deps externas
│   ├── Conectividad/
│   │   ├── WifiManager.cs          ← Conexión WiFi
│   │   └── BleManager.cs           ← Servidor BLE Nordic SPP para Android
│   ├── Servicios/
│   │   ├── RelojManager.cs         ← Hora sincronizada (SNTP)
│   │   ├── ClimaManager.cs         ← Clima via OpenWeatherMap (gratis)
│   │   └── IaManager.cs            ← IA via Qwen API (gratis)
│   ├── UI/
│   │   ├── MenuManager.cs          ← Gestión de pantallas y modos
│   │   ├── ScrollManager.cs        ← Scroll de texto largo en LCD
│   │   └── BotonesManager.cs       ← 3 botones: arriba, abajo, seleccionar
│   ├── AnimacionesMochi.cs         ← Frames de animación para SSD1306
│   ├── AnimacionesLcd.cs           ← Custom chars para LCD1602
│   └── Program.cs                  ← Loop principal
│
├── MochiES_GifConverter/           ← Convierte GIFs a byte arrays C# (WinForms)
│   └── FormConverter.cs
│
├── MochiES_Configurador/           ← Configura y flashea el dispositivo (WinForms)
│   └── FormFlasher.cs
│
├── INSTRUCCIONES.cs                ← Contexto del proyecto para IAs (Claude/Copilot)
├── .gitignore
└── README.md
```

---

## 🗺️ Roadmap

### Fase 1 — Firmware base ✅
- [x] Driver LCD1602 (I2C + PCF8574)
- [x] Driver SSD1306 (I2C directo, sin dependencias externas)
- [x] Interfaz `IDisplayDriver` portable entre displays
- [x] Animaciones custom chars (LCD) y frames bitmap (OLED)
- [x] GifConverter — convierte GIFs animados a byte arrays C#
- [x] Configurador con flasheo integrado (WinForms)

### Fase 2 — Interacción y conectividad 🔄
- [ ] 3 botones de navegación (↑ ↓ ✓)
- [ ] Sensor táctil TTP223 como alternativa a los botones físicos
- [ ] WiFi + hora sincronizada (SNTP — pool.ntp.org, gratuito)
- [ ] Clima en tiempo real (OpenWeatherMap — gratuito)
- [ ] Notificaciones BLE desde Android

### Fase 3 — IA integrada 📋
- [ ] IA por texto — Qwen API (gratuito con registro)
- [ ] Scroll de texto largo en el display
- [ ] Menú de modos navegable con los botones

### Fase 4 — Voz 📋
- [ ] Micrófono I2S (INMP441) + transcripción via Whisper API
- [ ] TTS — respuesta hablada via speaker I2S

### Fase 5 — App Android 📋
- [ ] App Android para conectar por BLE
- [ ] Enviar notificaciones, nombre de canción, comandos al asistente

### Fase 6 — Movilidad (futuro) 📋
- [ ] Versión con ruedas — motor paso a paso 28BYJ-48
- [ ] Patrón de patrulla autónomo

---

## 💡 Por qué 3 botones en lugar de joystick

El joystick analógico es un módulo grande para un dispositivo tiny desk. Tres botones pulsadores de 6mm dan la misma funcionalidad de navegación (↑ ↓ ✓) ocupando mucho menos espacio, a costo mínimo, y son más intuitivos para el usuario final.

Como alternativa, el sensor táctil TTP223 permite navegación con un solo módulo — un toque para "siguiente" y toque sostenido para "seleccionar".

---

## 🌐 APIs — todas gratuitas

MochiES está diseñado para funcionar completamente con servicios gratuitos.

| API | Para qué | Tier gratuito |
|---|---|---|
| [OpenWeatherMap](https://openweathermap.org/api) | Clima actual | 1.000 llamadas/día |
| [Qwen (Alibaba)](https://dashscope.aliyuncs.com) | IA — respuestas de texto | Gratuito con registro |
| [pool.ntp.org](https://www.pool.ntp.org) | Sincronización de hora | Siempre gratuito |

> La transcripción de voz via OpenAI Whisper es el único componente con costo ($0.006/min) y es completamente **opcional**. El asistente funciona perfecto solo con texto.

---

## 🤝 Contribuir

Este proyecto nació para ser una base que otros desarrolladores puedan usar, aprender y mejorar. ¡Las contribuciones son bienvenidas!

1. Hacé un **fork** del repositorio en [github.com/Juanzett/MochiES](https://github.com/Juanzett/MochiES)
2. Creá una branch: `git checkout -b feature/mi-feature`
3. Commiteá los cambios: `git commit -m 'Agrego soporte para...'`
4. Pusheá: `git push origin feature/mi-feature`
5. Abrí un **Pull Request**

### Ideas para contribuir
- Nuevas animaciones (GIF → byte arrays listos para usar)
- Soporte para nuevas pantallas (SH1106, SSD1309, ST7735)
- Nuevos servicios (Spotify, calendario, RSS, clima extendido)
- App Android para conectar por BLE
- Mejoras al GifConverter o al Configurador

---

## 📚 Referencias y créditos

Este proyecto no existiría sin el trabajo de las siguientes personas y proyectos.

### Proyectos que lo inspiraron

**[huykhoong/esp32_dasai_mochi_clone_and_how_to](https://github.com/huykhoong/esp32_dasai_mochi_clone_and_how_to)**
El proyecto Arduino/C original que inspiró MochiES. Toda la lógica de animaciones y el concepto del Mochi viene de acá. Crédito completo a [@huykhoong](https://github.com/huykhoong).

**[78/xiaozhi-esp32](https://github.com/78/xiaozhi-esp32)**
Referencia de arquitectura para las integraciones de IA, voz y conectividad. Un proyecto increíble que demuestra todo lo que se puede construir con un ESP32 y modelos de lenguaje. Crédito completo a [@78](https://github.com/78).

**[The Mochi — themochi.huykhong.com](https://themochi.huykhong.com)**
El firmware original avanzado del proyecto, también por [@huykhoong](https://github.com/huykhoong).

### Tecnologías utilizadas

**[.NET nanoFramework](https://nanoframework.net/)**
El runtime open source que hace posible escribir C# para microcontroladores. Miembro de la .NET Foundation. Sin este proyecto, MochiES no existiría.

**[nanoFramework.IoT.Device](https://github.com/nanoframework/nanoFramework.IoT.Device)**
Librería de drivers para sensores y periféricos en .NET nanoFramework.

---

## 📄 Licencia

Distribuido bajo la licencia **MIT**. Ver [`LICENSE`](LICENSE) para más información.

Este proyecto es completamente **gratuito**. Podés usarlo, modificarlo y distribuirlo libremente, incluso para proyectos comerciales. La única condición es mantener el aviso de licencia original.

---

<div align="center">

Desarrollado con ❤️ en Argentina 🇦🇷

**[github.com/Juanzett/MochiES](https://github.com/Juanzett/MochiES)**

Si este proyecto te fue útil, dejale una ⭐ — ayuda a que otros devs lo encuentren

</div>
