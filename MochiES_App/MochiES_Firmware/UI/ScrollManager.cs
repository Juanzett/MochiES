using System;
using System.Threading;
using MochiES.Drivers;

namespace MochiES.UI
{
    /// <summary>
    /// Gestor de scroll para mostrar textos largos en pantallas pequeñas.
    /// Soporta scroll horizontal en LCD y renderizado directo en OLED.
    /// </summary>
    internal class ScrollManager
    {
        // --- Propiedades del scroll ---
        private string _textoActual;
        private IDisplayDriver _display;
        private int _posicionScroll;
        private bool _enScroll;
        private Thread _threadScroll;
        private int _velocidadMs;  // Milisegundos entre cambios de scroll

        // --- Constantes ---
        private const int VELOCIDAD_DEFECTO_MS = 300;

        /// <summary>
        /// Constructor del gestor de scroll.
        /// </summary>
        /// <param name="display">Interfaz del display a usar</param>
        /// <param name="velocidadMs">Velocidad del scroll en milisegundos (por defecto 300ms)</param>
        public ScrollManager(IDisplayDriver display, int velocidadMs = VELOCIDAD_DEFECTO_MS)
        {
            _display = display;
            _velocidadMs = velocidadMs;
            _textoActual = "";
            _posicionScroll = 0;
            _enScroll = false;
        }

        /// <summary>
        /// Inicia el scroll horizontal de un texto en la línea especificada.
        /// Para textos cortos (≤ ancho del display), se muestra estático.
        /// Para textos largos, se desplaza horizontalmente.
        /// </summary>
        /// <param name="texto">Texto a mostrar con scroll</param>
        /// <param name="linea">Línea del display donde mostrar (0 o 1 en LCD1602)</param>
        public void IniciarScroll(string texto, int linea = 0)
        {
            _textoActual = texto ?? "";
            _posicionScroll = 0;
            _enScroll = true;

            Console.WriteLine($"[ScrollManager] Iniciando scroll: '{_textoActual}' en línea {linea}");

            // Si el texto cabe en una línea, no es necesario hacer scroll
            if (_textoActual.Length <= _display.AnchoDisplay)
            {
                _display.MostrarTextoPorLinea(linea, _textoActual);
                _enScroll = false;
                return;
            }

            // Iniciar hilo de scroll
            _threadScroll = new Thread(() => ProcesoScroll(linea));
            _threadScroll.Start();
        }

        /// <summary>
        /// Loop de scroll ejecutado en un hilo separado.
        /// </summary>
        private void ProcesoScroll(int linea)
        {
            int anchoDisplay = _display.AnchoDisplay;

            while (_enScroll && !string.IsNullOrEmpty(_textoActual))
            {
                try
                {
                    // Extraer substring para la ventana actual del display
                    string ventanaTexto = ObtenerVentanaScroll(anchoDisplay);

                    // Mostrar en el display
                    _display.MostrarTextoPorLinea(linea, ventanaTexto);

                    // Mover a la siguiente posición
                    _posicionScroll++;

                    // Si llegamos al final, reiniciar
                    if (_posicionScroll >= _textoActual.Length + anchoDisplay)
                    {
                        _posicionScroll = 0;
                    }

                    Thread.Sleep(_velocidadMs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Error en ProcesoScroll: {ex.Message}");
                    _enScroll = false;
                }
            }
        }

        /// <summary>
        /// Obtiene la ventana de texto visible en el display.
        /// Completa con espacios si es necesario para efecto de scroll suave.
        /// </summary>
        private string ObtenerVentanaScroll(int ancho)
        {
            string textoConPadding = _textoActual + new string(' ', ancho);

            if (_posicionScroll + ancho > textoConPadding.Length)
            {
                // Caso especial: cuando el scroll llega al final
                return textoConPadding.Substring(_posicionScroll);
            }

            return textoConPadding.Substring(_posicionScroll, ancho);
        }

        /// <summary>
        /// Detiene el scroll actual.
        /// </summary>
        public void DetenerScroll()
        {
            _enScroll = false;

            if (_threadScroll != null)
            {
                _threadScroll.Join(500);
            }

            Console.WriteLine("[ScrollManager] Scroll detenido");
        }

        /// <summary>
        /// Establece la velocidad del scroll.
        /// </summary>
        /// <param name="velocidadMs">Milisegundos entre cambios</param>
        public void EstablecerVelocidad(int velocidadMs)
        {
            _velocidadMs = velocidadMs;
        }

        /// <summary>
        /// Obtiene el estado actual del scroll.
        /// </summary>
        public bool EnScroll => _enScroll;
    }
}
