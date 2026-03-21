// ============================================================
//  Ssd1306Driver.cs
//  Driver I2C directo para OLED SSD1306
//  Implementa IDisplayDriver para portabilidad
// ============================================================

using System;
using System.Device.I2c;
using System.Threading;

namespace MochiES.Drivers
{
    public class Ssd1306Driver : IDisplayDriver
    {
        private I2cDevice _i2cDevice;
        private byte[] _buffer = new byte[1024]; // 128×64 / 8 = 1024 bytes
        private const byte COMMAND_MODE = 0x00;
        private const byte DATA_MODE = 0x40;
        private const int DISPLAY_WIDTH = 128;
        private const int DISPLAY_HEIGHT = 64;

        // Comandos SSD1306
        private const byte CMD_DISPLAY_OFF = 0xAE;
        private const byte CMD_DISPLAY_ON = 0xAF;
        private const byte CMD_SET_CONTRAST = 0x81;
        private const byte CMD_SET_NORMAL_DISPLAY = 0xA6;
        private const byte CMD_SET_COLUMN_ADDRESS = 0x21;
        private const byte CMD_SET_PAGE_ADDRESS = 0x22;
        private const byte CMD_SET_MEMORY_MODE = 0x20;
        private const byte CMD_COM_SCAN_INC = 0xC0;
        private const byte CMD_COM_SCAN_DEC = 0xC8;
        private const byte CMD_SET_SEGMENT_REMAP = 0xA1;
        private const byte CMD_MULTIPLEX_RATIO = 0xA8;
        private const byte CMD_SET_DISPLAY_OFFSET = 0xD3;
        private const byte CMD_SET_CLOCK_DIV = 0xD5;
        private const byte CMD_SET_PRE_CHARGE = 0xD9;
        private const byte CMD_SET_VCOMH = 0xDB;
        private const byte CMD_CHARGE_PUMP = 0x8D;

        public int AnchoDisplay => DISPLAY_WIDTH;
        public int AltoDisplay => DISPLAY_HEIGHT;
        public string TipoDisplay => "SSD1306";

        public Ssd1306Driver(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            Inicializar();
        }

        public void Inicializar()
        {
            EnviarComando(CMD_DISPLAY_OFF);
            EnviarComando(CMD_SET_CLOCK_DIV);
            EnviarComando(0x80);
            EnviarComando(CMD_MULTIPLEX_RATIO);
            EnviarComando(0x3F);
            EnviarComando(CMD_SET_DISPLAY_OFFSET);
            EnviarComando(0x00);
            EnviarComando(0x40);
            EnviarComando(CMD_CHARGE_PUMP);
            EnviarComando(0x14);
            EnviarComando(CMD_SET_MEMORY_MODE);
            EnviarComando(0x02);
            EnviarComando(CMD_SET_SEGMENT_REMAP);
            EnviarComando(CMD_COM_SCAN_DEC);
            EnviarComando(CMD_SET_CONTRAST);
            EnviarComando(0xFF);
            EnviarComando(CMD_SET_PRE_CHARGE);
            EnviarComando(0xF1);
            EnviarComando(CMD_SET_VCOMH);
            EnviarComando(0x40);
            EnviarComando(CMD_SET_NORMAL_DISPLAY);

            Limpiar();
            EnviarComando(CMD_DISPLAY_ON);

            Thread.Sleep(100);
        }

        public void Limpiar()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            MostrarFrame(_buffer);
        }

        public void MostrarTexto(string texto)
        {
            // SSD1306 no tiene renderizado de texto incorporado
            // Aquí irían funciones de dibujo de caracteres si las necesitaras
            Console.WriteLine("SSD1306: MostrarTexto() requiere una fuente de caracteres personalizada");
        }

        public void MostrarTextoPorLinea(int linea, string texto)
        {
            MostrarTexto(texto);
        }

        public void MostrarFrame(byte[] frameData)
        {
            if (frameData == null || frameData.Length != 1024)
                return;

            Array.Copy(frameData, _buffer, 1024);

            for (int pagina = 0; pagina < 8; pagina++)
            {
                byte[] comandosPagina = new byte[]
                {
                    (byte)(0xB0 + pagina),
                    0x00,
                    0x10
                };
                EnviarComandos(comandosPagina);

                byte[] datosPagina = new byte[128];
                Array.Copy(frameData, pagina * 128, datosPagina, 0, 128);

                EnviarDatos(datosPagina);
                Thread.Sleep(10);
            }
        }

        private void EnviarComando(byte comando)
        {
            byte[] buffer = new byte[] { COMMAND_MODE, comando };
            _i2cDevice.Write(buffer);
        }

        private void EnviarComandos(byte[] comandos)
        {
            byte[] buffer = new byte[comandos.Length + 1];
            buffer[0] = COMMAND_MODE;
            Array.Copy(comandos, 0, buffer, 1, comandos.Length);
            _i2cDevice.Write(buffer);
        }

        private void EnviarDatos(byte[] datos)
        {
            byte[] buffer = new byte[datos.Length + 1];
            buffer[0] = DATA_MODE;
            Array.Copy(datos, 0, buffer, 1, datos.Length);
            _i2cDevice.Write(buffer);
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