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

        // --- Configuración de animación ---
        const int FRAME_DELAY_MS = 80;
        const int PAUSE_BETWEEN_MSGS_MS = 3000;

        // --- Pantalla (interfaz genérica) ---
        static IDisplayDriver _display;

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

                // Loop principal
                Console.WriteLine("Iniciando loop de animación...");
                int contador = 0;

                while (true)
                {
                    MostrarMensaje("Linea 1: " + contador, "Linea 2: MochiES");
                    contador++;
                    Thread.Sleep(PAUSE_BETWEEN_MSGS_MS);

                    MostrarMensaje("Contador:", contador.ToString());
                    Thread.Sleep(PAUSE_BETWEEN_MSGS_MS);

                    MostrarMensaje("LCD1602 OK!", "¡Funcionando!");
                    Thread.Sleep(PAUSE_BETWEEN_MSGS_MS);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }

        static void MostrarIntro()
        {
            _display.Limpiar();

            if (_display.TipoDisplay == "LCD1602")
            {
                _display.MostrarTextoPorLinea(0, "Bienvenido!");
                _display.MostrarTextoPorLinea(1, "MochiES v1.0");
            }
            else
            {
                // Para OLED, aquí iría dibujo de bitmap
                _display.MostrarTexto("OLED Ready");
            }
        }

        static void MostrarMensaje(string linea1, string linea2)
        {
            Console.WriteLine(">>> " + linea1 + " | " + linea2);

            if (_display.TipoDisplay == "LCD1602")
            {
                _display.MostrarTextoPorLinea(0, linea1);
                _display.MostrarTextoPorLinea(1, linea2);
            }
            else
            {
                _display.MostrarTexto(linea1 + "\n" + linea2);
            }
        }

        static void ReproducirAnimacion(byte[][] frames, string nombre)
        {
            // Esto solo funciona con SSD1306
            if (_display.TipoDisplay != "SSD1306")
            {
                Console.WriteLine("Animaciones bitmap solo en SSD1306");
                return;
            }

            Console.WriteLine("Reproduciendo: " + nombre);

            for (int i = 0; i < frames.Length; i++)
            {
                _display.MostrarFrame(frames[i]);
                Thread.Sleep(FRAME_DELAY_MS);
            }
        }
    }
}