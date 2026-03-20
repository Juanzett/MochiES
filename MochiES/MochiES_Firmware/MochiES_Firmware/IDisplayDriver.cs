// ============================================================
//  IDisplayDriver.cs
//  Interfaz abstracta para drivers de pantalla
//  Permite intercambiar LCD1602, SSD1306, etc. fácilmente
// ============================================================

using System;

namespace MochiES
{
    /// <summary>
    /// Interfaz común para todos los drivers de pantalla.
    /// Esto permite cambiar entre LCD1602 y SSD1306 sin modificar Program.cs
    /// </summary>
    public interface IDisplayDriver : IDisposable
    {
        /// <summary>
        /// Inicializa la pantalla
        /// </summary>
        void Inicializar();

        /// <summary>
        /// Limpia la pantalla
        /// </summary>
        void Limpiar();

        /// <summary>
        /// Muestra texto en la pantalla
        /// </summary>
        void MostrarTexto(string texto);

        /// <summary>
        /// Muestra texto en una línea específica (para LCD)
        /// </summary>
        void MostrarTextoPorLinea(int linea, string texto);

        /// <summary>
        /// Muestra un frame bitmap (para OLED)
        /// </summary>
        void MostrarFrame(byte[] frameData);

        /// <summary>
        /// Obtiene el ancho de la pantalla en caracteres (LCD) o píxeles (OLED)
        /// </summary>
        int AnchoDisplay { get; }

        /// <summary>
        /// Obtiene el alto de la pantalla en filas (LCD) o píxeles (OLED)
        /// </summary>
        int AltoDisplay { get; }

        /// <summary>
        /// Obtiene el tipo de pantalla ("LCD1602", "SSD1306", etc)
        /// </summary>
        string TipoDisplay { get; }
    }
}