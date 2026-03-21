// ============================================================
//  MochiES - Firmware ESP32 en C# con .NET nanoFramework
//  Etapa 1: Control de pantalla I2C portátil (LCD1602 o SSD1306)
// ============================================================
//
//  HARDWARE ACTUAL:
//    - ESP32 C3 Mini (o cualquier ESP32)
//    - Pantalla LCD1602 16x2 con módulo I2C PCF8574
//      VCC → 3.3V | GND → GND | SCL → GPIO21 | SDA → GPIO20
//      Dirección I2C típica: 0x27
//
//  HARDWARE FUTURO (cambiar driver solamente):
//    - Pantalla OLED SSD1306 128x64 (I2C)
//      Dirección I2C típica: 0x3C o 0x3D
//
//  DEPENDENCIAS (NuGet en nanoFramework):
//    - nanoFramework.Hardware.Esp32
//    - nanoFramework.Device.I2c
// ============================================================

using System;
using System.Threading;
using System.Device.I2c;
using nanoFramework.Hardware.Esp32;
using MochiES.Drivers;
using MochiES.UI;
using MochiES.Conectividad;
using MochiES.Servicios;

namespace MochiES
{
    public class Program
    {
        // --- Enumeración de pantallas disponibles ---
        private enum DisplayTarget
        {
            Lcd1602,
            Ssd1306
        }

        // --- Configuración de pines I2C ---
        private const int PIN_SCL = 21;
        private const int PIN_SDA = 20;
        private const int I2C_BUS = 1;

        // --- Selección de display (CAMBIAR AQUÍ SEGÚN TU HARDWARE) ---
        // Usar static readonly en lugar de const para evitar CS0162
        private static readonly DisplayTarget DISPLAY_TARGET = DisplayTarget.Lcd1602;

        // Dirección I2C según el display seleccionado
        private static readonly int DISPLAY_ADDRESS = (DISPLAY_TARGET == DisplayTarget.Lcd1602) ? 0x27 : 0x3C;

        // --- Configuración WiFi (CAMBIAR AQUÍ CON TUS CREDENCIALES) ---
        private const string WIFI_SSID = "TU_RED_WIFI";        // ← Cambiar aquí
        private const string WIFI_PASSWORD = "TU_CONTRASEÑA";  // ← Cambiar aquí

        // --- Configuración de animación ---
        private const int FRAME_DELAY_MS = 80;
        private const int PAUSE_BETWEEN_MSGS_MS = 3000;

        // --- Pantalla (interfaz genérica) ---
        private static IDisplayDriver _display;
        private static BotonesManager _botones;
        private static MenuManager _menu;
        private static ScrollManager _scroll;
        private static WifiManager _wifi;
        private static RelojManager _reloj;

        public static void Main()
        {
            Console.WriteLine("=== MochiES arrancando... ===");
            Console.WriteLine($"Display seleccionado: {DISPLAY_TARGET}");
            Console.WriteLine($"Dirección I2C: 0x{DISPLAY_ADDRESS:X2}\n");

            try
            {
                // Configurar pines I2C en el ESP32
                // IMPORTANTE: Esto DEBE ejecutarse antes de crear I2cDevice
                Console.WriteLine("Configurando pines I2C...");
                Configuration.SetPinFunction(PIN_SCL, DeviceFunction.I2C1_CLOCK);
                Configuration.SetPinFunction(PIN_SDA, DeviceFunction.I2C1_DATA);
                Console.WriteLine("✓ Pines I2C configurados correctamente\n");

                // Inicializar comunicación I2C
                Console.WriteLine($"Inicializando I2C en bus {I2C_BUS}, dirección 0x{DISPLAY_ADDRESS:X2}...");
                I2cConnectionSettings i2cSettings = new I2cConnectionSettings(I2C_BUS, DISPLAY_ADDRESS);
                I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);

                if (i2cDevice == null)
                {
                    throw new Exception("No se pudo crear el dispositivo I2C. Verifica la conexión hardware.");
                }

                Console.WriteLine("✓ I2C inicializado correctamente\n");

                // Inicializar el driver correcto según el hardware
                Console.WriteLine("Inicializando driver de display...");
                if (DISPLAY_TARGET == DisplayTarget.Ssd1306)
                {
                    Console.WriteLine("Inicializando SSD1306 OLED...");
                    _display = new Ssd1306Driver(i2cDevice);
                }
                else
                {
                    Console.WriteLine("Inicializando LCD1602...");
                    _display = new Lcd1602Driver(i2cDevice, DISPLAY_ADDRESS);
                }

                Console.WriteLine($"✓ Display inicializado: {_display.TipoDisplay}");
                Console.WriteLine($"✓ Resolución: {_display.AnchoDisplay}x{_display.AltoDisplay}\n");

                // Mostrar pantalla de bienvenida
                MostrarIntro();
                Thread.Sleep(2000);

                // Inicializar gestor de botones
                Console.WriteLine("Inicializando botones...");
                _botones = new BotonesManager(
                    pinArriba: 34,
                    pinAbajo: 35,
                    pinSelect: 32
                );
                _botones.BotonPulsado += ManejadorBotonPulsado;
                Console.WriteLine("✓ Botones inicializados\n");

                // Inicializar gestor de menús
                Console.WriteLine("Inicializando menú...");
                _menu = new MenuManager(_display);
                Console.WriteLine("✓ Menú inicializado\n");

                // Inicializar gestor de scroll
                Console.WriteLine("Inicializando scroll...");
                _scroll = new ScrollManager(_display, velocidadMs: 250);
                Console.WriteLine("✓ Scroll inicializado\n");

                // ===== M1: INICIALIZAR WiFi Y RELOJ =====
                Console.WriteLine("===== INICIANDO MÓDULO M1: WiFi + Reloj =====\n");

                // Inicializar WifiManager
                _wifi = new WifiManager();
                _wifi.Inicializar();

                _display.Limpiar();
                _display.MostrarTextoPorLinea(0, "WiFi: conectando");
                _display.MostrarTextoPorLinea(1, "...");

                // Intentar conectar a WiFi
                if (_wifi.ConectarAWifi(WIFI_SSID, WIFI_PASSWORD))
                {
                    _display.Limpiar();
                    _display.MostrarTextoPorLinea(0, "WiFi OK!");
                    _display.MostrarTextoPorLinea(1, _wifi.DireccionIP);
                    Thread.Sleep(2000);

                    // Inicializar RelojManager
                    _reloj = new RelojManager(_display, linea: 0, intervaloMs: 1000);

                    _display.Limpiar();
                    _display.MostrarTextoPorLinea(0, "Sincronizando");
                    _display.MostrarTextoPorLinea(1, "hora...");

                    // Sincronizar hora
                    if (_reloj.SincronizarHora())
                    {
                        _display.Limpiar();
                        _display.MostrarTextoPorLinea(0, "Hora OK!");
                        _display.MostrarTextoPorLinea(1, _reloj.HoraActual.ToString("HH:mm:ss"));
                        Thread.Sleep(2000);

                        // Iniciar actualización continua de la hora
                        _reloj.IniciarActualizacionDisplay();

                        Console.WriteLine("\n✅ M1 completado: WiFi + Reloj funcionando\n");
                    }
                    else
                    {
                        _display.Limpiar();
                        _display.MostrarTextoPorLinea(0, "Error sincronizar");
                        _display.MostrarTextoPorLinea(1, "hora");
                        Thread.Sleep(2000);
                    }
                }
                else
                {
                    _display.Limpiar();
                    _display.MostrarTextoPorLinea(0, "WiFi: Error");
                    _display.MostrarTextoPorLinea(1, "Reintentando...");
                    Thread.Sleep(2000);
                }

                // Mostrar menú principal
                _menu.MostrarModoActual();

                // Loop principal - esperar eventos de botones
                Console.WriteLine("=== Sistema listo. Esperando eventos de botones...\n");

                while (true)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERROR CRÍTICO] Excepción en Main: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack: {ex.StackTrace}");

                if (_display != null)
                {
                    _display.Limpiar();
                    _display.MostrarTextoPorLinea(0, "ERROR CRITICO");
                    _display.MostrarTextoPorLinea(1, ex.Message.Substring(0, Math.Min(16, ex.Message.Length)));
                }

                Thread.Sleep(5000);
            }
            finally
            {
                // Limpiar recursos
                if (_reloj != null)
                {
                    _reloj.DetenerActualizacionDisplay();
                    _reloj.Dispose();
                }

                if (_wifi != null)
                {
                    _wifi.Dispose();
                }

                if (_scroll != null)
                {
                    _scroll.DetenerScroll();
                }

                if (_botones != null)
                {
                    _botones.Dispose();
                }

                if (_display != null)
                {
                    _display.Dispose();
                }

                Console.WriteLine("\n=== MochiES finalizado ===");
            }
        }

        /// <summary>
        /// Manejador de eventos para pulsaciones de botones.
        /// </summary>
        private static void ManejadorBotonPulsado(object sender, BotonesManager.BotonEventArgs args)
        {
            Console.WriteLine($"[EVENT] Botón pulsado: {args.Boton}");

            switch (args.Boton)
            {
                case BotonesManager.BotonTipo.Arriba:
                    _menu.NavigarArriba();
                    break;

                case BotonesManager.BotonTipo.Abajo:
                    _menu.NavigarAbajo();
                    break;

                case BotonesManager.BotonTipo.Select:
                    _menu.ConfirmarSeleccion();
                    DemoScrollText();  // Demo temporal
                    break;
            }
        }

        /// <summary>
        /// Muestra la pantalla de introducción.
        /// </summary>
        private static void MostrarIntro()
        {
            _display.Limpiar();
            _display.MostrarTextoPorLinea(0, "MOCHIES v1.0");
            _display.MostrarTextoPorLinea(1, "Inicializando...");
        }

        /// <summary>
        /// Demo: muestra el scroll de texto cuando se presiona Select.
        /// (Este método es temporal para pruebas)
        /// </summary>
        private static void DemoScrollText()
        {
            string textoLargo = "Bienvenido a MochiES - Tiny Desk Companion";
            Console.WriteLine($"\n[DEMO] Mostrando scroll: '{textoLargo}'");

            // Pausar actualización del reloj
            if (_reloj != null)
            {
                _reloj.DetenerActualizacionDisplay();
            }

            _scroll.IniciarScroll(textoLargo, linea: 1);

            // Esperar a que termine el scroll
            Thread.Sleep(5000);

            _scroll.DetenerScroll();

            // Reanudar actualización del reloj
            if (_reloj != null)
            {
                _reloj.IniciarActualizacionDisplay();
            }

            // Volver al menú
            _menu.MostrarModoActual();
        }
    }
}