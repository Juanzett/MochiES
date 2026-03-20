// ============================================================
//  AnimacionesMochi.cs
//  Frames de animación almacenados como byte arrays
//
//  FORMATO DE CADA FRAME:
//    - 1024 bytes por frame (128×64 pixels / 8 bits por byte)
//    - Organizado en páginas SSD1306:
//      byte[página * 128 + columna]
//      donde cada bit = 1 pixel (1=blanco, 0=negro)
//
//  CÓMO AGREGAR TUS PROPIAS ANIMACIONES:
//    1. Conseguís un GIF animado de 128x64 pixels (o lo redimensionás)
//    2. Usás la herramienta GifConverter (Etapa 2) para convertirlo
//    3. Pegás el byte[][] generado acá como nueva propiedad estática
//
//  NOTA: Los frames de abajo son EJEMPLOS SIMPLIFICADOS con
//  caras dibujadas manualmente. Los frames reales del Mochi
//  se generan con la herramienta GifConverter.
// ============================================================

using System;

namespace MochiES
{
    /// <summary>
    /// Biblioteca de animaciones para MochiES.
    /// Cada animación es un array de frames (byte[][]).
    /// Cada frame es un bitmap 128x64 en formato SSD1306 (byte[1024]).
    /// </summary>
    public static class AnimacionesMochi
    {
        // ==================================================
        //  Animación: Sonrisa
        //  Cara feliz con ojos cerrados en arco
        // ==================================================
        public static byte[][] Sonrisa => new byte[][]
        {
            GenerarCaraSonrisa(fase: 0),
            GenerarCaraSonrisa(fase: 1),
            GenerarCaraSonrisa(fase: 2),
            GenerarCaraSonrisa(fase: 1),
        };

        // ==================================================
        //  Animación: Guiño
        //  Un ojo abierto, uno cerrado
        // ==================================================
        public static byte[][] Guino => new byte[][]
        {
            GenerarCaraGuino(ojoDerechoCerrado: false),
            GenerarCaraGuino(ojoDerechoCerrado: true),
            GenerarCaraGuino(ojoDerechoCerrado: true),
            GenerarCaraGuino(ojoDerechoCerrado: false),
        };

        // ==================================================
        //  Animación: Sorpresa
        //  Ojos grandes abiertos y boca en "O"
        // ==================================================
        public static byte[][] Sorpresa => new byte[][]
        {
            GenerarCaraSorpresa(escala: 0),
            GenerarCaraSorpresa(escala: 1),
            GenerarCaraSorpresa(escala: 2),
            GenerarCaraSorpresa(escala: 2),
            GenerarCaraSorpresa(escala: 1),
        };

        // ==================================================
        //  HELPERS: Generadores de frames de ejemplo
        //  (Reemplazados por datos reales del GifConverter)
        // ==================================================

        static byte[] GenerarCaraSonrisa(int fase)
        {
            byte[] frame = new byte[1024]; // 128 × 64 / 8
            // Dibujar cara circular
            DibujarCirculo(frame, 64, 32, 25);
            // Ojos (arcos según fase de animación)
            DibujarArcoOjo(frame, 45, 24, fase);
            DibujarArcoOjo(frame, 83, 24, fase);
            // Sonrisa
            DibujarSonrisa(frame, 64, 40);
            return frame;
        }

        static byte[] GenerarCaraGuino(bool ojoDerechoCerrado)
        {
            byte[] frame = new byte[1024];
            DibujarCirculo(frame, 64, 32, 25);
            // Ojo izquierdo siempre abierto
            DibujarOjoAbierto(frame, 45, 24);
            // Ojo derecho según estado
            if (ojoDerechoCerrado)
                DibujarLinea(frame, 78, 24, 88, 24); // línea = guiño
            else
                DibujarOjoAbierto(frame, 83, 24);
            DibujarSonrisa(frame, 64, 40);
            return frame;
        }

        static byte[] GenerarCaraSorpresa(int escala)
        {
            byte[] frame = new byte[1024];
            DibujarCirculo(frame, 64, 32, 25);
            int radio = 4 + escala * 2;
            DibujarCirculo(frame, 45, 24, radio); // ojo izq
            DibujarCirculo(frame, 83, 24, radio); // ojo der
            DibujarCirculo(frame, 64, 42, radio); // boca en O
            return frame;
        }

        // ==================================================
        //  PRIMITIVAS DE DIBUJO en buffer SSD1306
        // ==================================================

        /// <summary>
        /// Activa un pixel en el buffer SSD1306.
        /// El buffer está organizado en páginas de 8 filas.
        /// </summary>
        static void SetPixel(byte[] buffer, int x, int y, bool on = true)
        {
            if (x < 0 || x >= 128 || y < 0 || y >= 64) return;
            int page = y / 8;
            int bit = y % 8;
            int index = page * 128 + x;
            if (on)
                buffer[index] |= (byte)(1 << bit);
            else
                buffer[index] &= (byte)~(1 << bit);
        }

        static void DibujarCirculo(byte[] buffer, int cx, int cy, int radio)
        {
            // Algoritmo de Bresenham para círculos
            int x = 0, y = radio;
            int d = 3 - 2 * radio;
            while (y >= x)
            {
                SetPixel(buffer, cx + x, cy + y);
                SetPixel(buffer, cx - x, cy + y);
                SetPixel(buffer, cx + x, cy - y);
                SetPixel(buffer, cx - x, cy - y);
                SetPixel(buffer, cx + y, cy + x);
                SetPixel(buffer, cx - y, cy + x);
                SetPixel(buffer, cx + y, cy - x);
                SetPixel(buffer, cx - y, cy - x);
                if (d < 0) d += 4 * x + 6;
                else { d += 4 * (x - y) + 10; y--; }
                x++;
            }
        }

        static void DibujarLinea(byte[] buffer, int x0, int y0, int x1, int y1)
        {
            // Algoritmo de Bresenham para líneas
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                SetPixel(buffer, x0, y0);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        static void DibujarArcoOjo(byte[] buffer, int cx, int cy, int fase)
        {
            // Ojo como arco (cuanto mayor fase, más cerrado)
            int apertura = 6 - fase * 2;
            for (int x = -5; x <= 5; x++)
            {
                int y = (x * x) / 8 - apertura;
                SetPixel(buffer, cx + x, cy + y);
            }
        }

        static void DibujarOjoAbierto(byte[] buffer, int cx, int cy)
        {
            DibujarCirculo(buffer, cx, cy, 5);
            SetPixel(buffer, cx, cy); // pupila
        }

        static void DibujarSonrisa(byte[] buffer, int cx, int cy)
        {
            // Curva de sonrisa
            for (int x = -10; x <= 10; x++)
            {
                int y = (x * x) / 15;
                SetPixel(buffer, cx + x, cy + y);
            }
        }
    }
}