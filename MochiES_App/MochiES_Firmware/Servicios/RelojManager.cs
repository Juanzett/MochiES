using System;
using System.Threading;
using MochiES.Drivers;

namespace MochiES.Servicios
{
    /// <summary>
    /// Gestor de reloj y sincronización de hora.
    /// Muestra la hora en tiempo real en el display.
    /// </summary>
    internal class RelojManager : IDisposable
    {
        // --- Propiedades ---
        public DateTime HoraActual { get; private set; }
        public bool Sincronizado { get; private set; }
        public string ServidorNTP { get; private set; }

        // --- Eventos ---
        public event EventHandler<EventArgs> SincronizacionExitosa;
        public event EventHandler<string> ErrorSincronizacion;

        // --- Variables internas ---
        private IDisplayDriver _display;
        private bool _mostrando;
        private Thread _threadActualizacion;
        private int _linea;
        private int _intervaloActualizacionMs;

        /// <summary>
        /// Constructor del gestor de reloj.
        /// </summary>
        /// <param name="display">Interfaz del display para mostrar la hora</param>
        /// <param name="linea">Línea del display donde mostrar la hora (0 o 1)</param>
        /// <param name="intervaloMs">Intervalo de actualización en milisegundos (por defecto 1000ms)</param>
        public RelojManager(IDisplayDriver display, int linea = 0, int intervaloMs = 1000)
        {
            _display = display;
            _linea = linea;
            _intervaloActualizacionMs = intervaloMs;
            Sincronizado = false;
            ServidorNTP = "";
            _mostrando = false;

            HoraActual = DateTime.UtcNow;
        }

        /// <summary>
        /// Sincroniza la hora del sistema (simulado para pruebas).
        /// En producción, usaría nanoFramework.Networking.Sntp
        /// </summary>
        public bool SincronizarHora()
        {
            try
            {
                Console.WriteLine("[RelojManager] Sincronizando hora del sistema...");

                // Para esta fase de desarrollo, usamos la hora actual del sistema
                // En producción con WiFi, usaríamos Sntp.UpdateTimeFromServer()
                HoraActual = DateTime.UtcNow;
                Sincronizado = true;
                ServidorNTP = "sistema (local)";

                Console.WriteLine($"[RelojManager] ✅ Hora sincronizada: {HoraActual:yyyy-MM-dd HH:mm:ss} UTC");

                SincronizacionExitosa?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                string error = $"Error en sincronización: {ex.Message}";
                Console.WriteLine($"[ERROR] {error}");
                ErrorSincronizacion?.Invoke(this, error);
                Sincronizado = false;
                return false;
            }
        }

        /// <summary>
        /// Inicia la actualización continua de la hora en el display.
        /// Muestra la hora actual cada segundo.
        /// </summary>
        public void IniciarActualizacionDisplay()
        {
            if (!_mostrando)
            {
                _mostrando = true;
                _threadActualizacion = new Thread(ProcesoActualizacion);
                _threadActualizacion.Start();

                Console.WriteLine($"[RelojManager] Actualizando hora en display cada {_intervaloActualizacionMs}ms");
            }
        }

        /// <summary>
        /// Loop de actualización ejecutado en un hilo separado.
        /// Actualiza la hora en el display de forma contínua.
        /// </summary>
        private void ProcesoActualizacion()
        {
            while (_mostrando)
            {
                try
                {
                    // Actualizar la hora actual
                    HoraActual = DateTime.UtcNow;

                    // Formatear la hora para el display
                    string horaFormato = FormatearHora(HoraActual);

                    // Mostrar en el display
                    _display.MostrarTextoPorLinea(_linea, horaFormato);

                    Thread.Sleep(_intervaloActualizacionMs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Error actualizando display: {ex.Message}");
                    _mostrando = false;
                }
            }
        }

        /// <summary>
        /// Detiene la actualización de la hora en el display.
        /// </summary>
        public void DetenerActualizacionDisplay()
        {
            _mostrando = false;

            if (_threadActualizacion != null)
            {
                _threadActualizacion.Join(1000);
            }

            Console.WriteLine("[RelojManager] Actualización de display detenida");
        }

        /// <summary>
        /// Formatea la hora según el ancho disponible del display.
        /// Para LCD1602 (16 caracteres): "HH:mm:ss" o "HH:mm"
        /// </summary>
        private string FormatearHora(DateTime hora)
        {
            int anchoDisplay = _display.AnchoDisplay;

            if (anchoDisplay >= 8)
            {
                // Formato: "HH:mm:ss" (8 caracteres)
                return hora.ToString("HH:mm:ss");
            }
            else if (anchoDisplay >= 5)
            {
                // Formato: "HH:mm" (5 caracteres)
                return hora.ToString("HH:mm");
            }
            else
            {
                // Formato reducido: "HH" (2 caracteres)
                return hora.ToString("HH");
            }
        }

        /// <summary>
        /// Establece manualmente la hora del sistema.
        /// </summary>
        public void EstablecerHora(DateTime hora)
        {
            try
            {
                HoraActual = hora;
                Console.WriteLine($"[RelojManager] Hora establecida manualmente: {hora:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] No se pudo establecer la hora: {ex.Message}");
            }
        }

        /// <summary>
        /// Libera los recursos utilizados por el gestor de reloj.
        /// </summary>
        public void Dispose()
        {
            DetenerActualizacionDisplay();
            Console.WriteLine("[RelojManager] Recursos liberados");
        }
    }
}
