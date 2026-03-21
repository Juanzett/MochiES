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
        // --- Configuración de pines I2C ---
        const int PIN_SCL = 21;
        const int PIN_SDA = 20;
        const int I2C_BUS = 1;

        // --- Selección de display (CAMBIAR AQUÍ SEGÚN TU HARDWARE) ---
        // Para LCD1602:
        const int DISPLAY_ADDRESS = 0x27;  // Dirección I2C del LCD1602
        const bool USAR_OLED = false;       // false = LCD1602, true = SSD1306

        // Para SSD1306 (futuro):
        // const int DISPLAY_ADDRESS = 0x3C;
        // const bool USAR_OLED = true;

        // --- Configuración WiFi (CAMBIAR AQUÍ CON TUS CREDENCIALES) ---
        const string WIFI_SSID = "TU_RED_WIFI";        // ← Cambiar aquí
        const string WIFI_PASSWORD = "TU_CONTRASEÑA";  // ← Cambiar aquí

        // --- Configuración de animación ---
        const int FRAME_DELAY_MS = 80;
        const int PAUSE_BETWEEN_MSGS_MS = 3000;

        // --- Pantalla (interfaz genérica) ---
        static IDisplayDriver _display;
        static BotonesManager _botones;
        static MenuManager _menu;
        static ScrollManager _scroll;
        static WifiManager _wifi;
        static RelojManager _reloj;

        public static void Main()
        {
            Console.WriteLine("=== MochiES arrancando... ===");

            try
            {
                // Configurar pines I2C en el ESP32
                Configuration.SetPinFunction(PIN_SCL, DeviceFunction.I2C1_CLOCK);
                Configuration.SetPinFunction(PIN_SDA, DeviceFunction.I2C1_DATA);

                // Inicializar comunicación I2C
                I2cConnectionSettings i2cSettings = new I2cConnectionSettings(I2C_BUS, DISPLAY_ADDRESS);
                I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);

                // Inicializar el driver correcto según el hardware
                if (USAR_OLED)
                {
                    Console.WriteLine("Inicializando SSD1306 OLED...");
                    _display = new Ssd1306Driver(i2cDevice);
                }
                else
                {
                    Console.WriteLine("Inicializando LCD1602...");
                    _display = new Lcd1602Driver(i2cDevice, DISPLAY_ADDRESS);
                }

                Console.WriteLine("Display inicializado: " + _display.TipoDisplay);
                Console.WriteLine("Resolución: " + _display.AnchoDisplay + "x" + _display.AltoDisplay);

                // Mostrar pantalla de bienvenida
                MostrarIntro();
                Thread.Sleep(2000);

                // Inicializar gestor de botones
                Console.WriteLine("\nInicializando botones...");
                _botones = new BotonesManager(
                    pinArriba: 34,
                    pinAbajo: 35,
                    pinSelect: 32
                );
                _botones.BotonPulsado += ManejadorBotonPulsado;

                // Inicializar gestor de menús
                Console.WriteLine("Inicializando menú...");
                _menu = new MenuManager(_display);

                // Inicializar gestor de scroll
                Console.WriteLine("Inicializando scroll...");
                _scroll = new ScrollManager(_display, velocidadMs: 250);

                // ===== M1: INICIALIZAR WiFi Y RELOJ =====
                Console.WriteLine("\n===== INICIANDO MÓDULO M1: WiFi + Reloj =====");

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

                        Console.WriteLine("\n✅ M1 completado: WiFi + Reloj funcionando");
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
                Console.WriteLine("\n=== Sistema listo. Esperando eventos de botones...\n");

                while (true)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Excepción en Main: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");

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
                    _wifi.Dispose();

                if (_scroll != null)
                    _scroll.DetenerScroll();

                if (_botones != null)
                    _botones.Dispose();

                if (_display != null)
                    _display.Dispose();

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
                _reloj.DetenerActualizacionDisplay();

            _scroll.IniciarScroll(textoLargo, linea: 1);

            // Esperar a que termine el scroll
            Thread.Sleep(5000);

            _scroll.DetenerScroll();

            // Reanudar actualización del reloj
            if (_reloj != null)
                _reloj.IniciarActualizacionDisplay();

            // Volver al menú
            _menu.MostrarModoActual();
        }
    }
}