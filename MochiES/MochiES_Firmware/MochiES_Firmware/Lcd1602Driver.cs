// ============================================================
//  Lcd1602Driver.cs
//  Driver I2C para pantalla LCD1602 con módulo PCF8574
//
//  Conexión:
//    VCC → 3.3V | GND → GND | SCL → GPIO21 | SDA → GPIO20
//    Dirección I2C: 0x27 (típico) o 0x3F
// ============================================================

using System;
using System.Device.I2c;
using System.Threading;

namespace MochiES
{
    public class Lcd1602Driver : IDisplayDriver
    {
        private I2cDevice _i2cDevice;
        private int _i2cAddress;
        private const int DISPLAY_WIDTH = 16;
        private const int DISPLAY_HEIGHT = 2;

        // Comandos LCD1602
        private const byte CMD_CLEAR = 0x01;
        private const byte CMD_HOME = 0x02;
        private const byte CMD_ENTRY_SET = 0x04;
        private const byte CMD_DISPLAY_CONTROL = 0x08;
        private const byte CMD_CURSOR_SHIFT = 0x10;
        private const byte CMD_FUNCTION_SET = 0x20;
        private const byte CMD_CGRAM_ADDR = 0x40;
        private const byte CMD_DDRAM_ADDR = 0x80;

        // Bits del módulo PCF8574
        private const byte BIT_RS = 0x01;   // Register Select
        private const byte BIT_RW = 0x02;   // Read/Write
        private const byte BIT_E = 0x04;    // Enable
        private const byte BIT_BL = 0x08;   // Backlight

        public int AnchoDisplay => DISPLAY_WIDTH;
        public int AltoDisplay => DISPLAY_HEIGHT;
        public string TipoDisplay => "LCD1602";

        public Lcd1602Driver(I2cDevice i2cDevice, int i2cAddress = 0x27)
        {
            _i2cDevice = i2cDevice;
            _i2cAddress = i2cAddress;
            Inicializar();
        }

        public void Inicializar()
        {
            Thread.Sleep(50);

            // Secuencia de inicialización (8-bit mode)
            EnviarComando(0x33);
            EnviarComando(0x32);
            EnviarComando(0x28); // 2 líneas, font 5x8
            EnviarComando(0x0C); // Display ON, cursor OFF
            EnviarComando(0x06); // Entry mode: incrementar cursor
            
            Limpiar();
        }

        public void Limpiar()
        {
            EnviarComando(CMD_CLEAR);
            Thread.Sleep(2);
        }

        public void MostrarTexto(string texto)
        {
            MostrarTextoPorLinea(0, texto);
        }

        public void MostrarTextoPorLinea(int linea, string texto)
        {
            if (linea < 0 || linea >= DISPLAY_HEIGHT)
                return;

            if (texto.Length > DISPLAY_WIDTH)
                texto = texto.Substring(0, DISPLAY_WIDTH);

            // Dirección DDRAM para cada línea
            byte[] direcciones = { 0x00, 0x40 };
            EnviarComando((byte)(CMD_DDRAM_ADDR | direcciones[linea]));

            // Enviar caracteres
            foreach (char c in texto)
            {
                EnviarDato((byte)c);
            }

            // Rellenar con espacios si es necesario
            int espacios = DISPLAY_WIDTH - texto.Length;
            for (int i = 0; i < espacios; i++)
            {
                EnviarDato((byte)' ');
            }
        }

        public void MostrarFrame(byte[] frameData)
        {
            // LCD1602 no soporta gráficos bitmap directamente
            Console.WriteLine("Advertencia: LCD1602 no soporta frames bitmap. Usa MostrarTexto()");
        }

        private void EnviarComando(byte comando)
        {
            EnviarByte(comando, isComando: true);
        }

        private void EnviarDato(byte dato)
        {
            EnviarByte(dato, isComando: false);
        }

        private void EnviarByte(byte valor, bool isComando)
        {
            // RS = 0 para comando, RS = 1 para dato
            byte rs = isComando ? (byte)0 : BIT_RS;
            byte bl = BIT_BL; // Backlight siempre encendido

            // Nibble alto
            byte nibbleAlto = (byte)((valor & 0xF0) | rs | bl);
            EnviarAlPcf8574(nibbleAlto);
            Pulsar(nibbleAlto);

            // Nibble bajo
            byte nibbleBajo = (byte)(((valor << 4) & 0xF0) | rs | bl);
            EnviarAlPcf8574(nibbleBajo);
            Pulsar(nibbleBajo);

            Thread.Sleep(1);
        }

        private void EnviarAlPcf8574(byte valor)
        {
            byte[] buffer = { valor };
            _i2cDevice.Write(buffer);
        }

        private void Pulsar(byte valor)
        {
            byte conEnable = (byte)(valor | BIT_E);
            EnviarAlPcf8574(conEnable);
            Thread.Sleep(1);

            byte sinEnable = (byte)(valor & ~BIT_E);
            EnviarAlPcf8574(sinEnable);
            Thread.Sleep(1);
        }

        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                _i2cDevice.Dispose();
            }
        }
    }
}