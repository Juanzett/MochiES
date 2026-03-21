using System;
using System.Threading;
using nanoFramework.Hardware.Esp32;
using System.Device.Gpio;

namespace MochiES.UI
{
    /// <summary>
    /// Gestiona los 3 botones pulsadores (↑ ↓ ✓) conectados al ESP32.
    /// Detecta pulsaciones con debounce para evitar falsos positivos.
    /// </summary>
    internal class BotonesManager : IDisposable
    {
        // --- Enumeración de botones ---
        public enum BotonTipo
        {
            Arriba,
            Abajo,
            Select
        }

        // --- Eventos de pulsación ---
        public event EventHandler<BotonEventArgs> BotonPulsado;

        // --- Clase para argumentos del evento ---
        public class BotonEventArgs : EventArgs
        {
            public BotonTipo Boton { get; set; }
            public DateTime FechaPulsacion { get; set; }
        }

        // --- Pines GPIO configurables ---
        private int _pinArriba;
        private int _pinAbajo;
        private int _pinSelect;

        // --- Parámetros de debounce ---
        private const int DEBOUNCE_MS = 50;  // Tiempo mínimo entre pulsaciones válidas
        private long _ultimaPulsacionArriba = 0;
        private long _ultimaPulsacionAbajo = 0;
        private long _ultimaPulsacionSelect = 0;

        // --- Control de polling ---
        private bool _activo = false;
        private Thread _threadPolling;
        private const int POLLING_INTERVAL_MS = 20;

        // --- Controlador GPIO ---
        private GpioController _gpio;

        /// <summary>
        /// Constructor del gestor de botones.
        /// </summary>
        /// <param name="pinArriba">Pin GPIO para botón arriba (↑) — típicamente GPIO 34</param>
        /// <param name="pinAbajo">Pin GPIO para botón abajo (↓) — típicamente GPIO 35</param>
        /// <param name="pinSelect">Pin GPIO para botón seleccionar (✓) — típicamente GPIO 32</param>
        public BotonesManager(int pinArriba = 34, int pinAbajo = 35, int pinSelect = 32)
        {
            _pinArriba = pinArriba;
            _pinAbajo = pinAbajo;
            _pinSelect = pinSelect;

            InicializarGpio();
            IniciarPolling();
        }

        /// <summary>
        /// Inicializa los pines GPIO en modo entrada con pull-up interno.
        /// </summary>
        private void InicializarGpio()
        {
            try
            {
                _gpio = new GpioController();

                // Configurar los 3 pines como entrada con pull-up interno
                ConfigurarPin(_pinArriba);
                ConfigurarPin(_pinAbajo);
                ConfigurarPin(_pinSelect);

                Console.WriteLine("[BotonesManager] GPIO inicializado correctamente");
                Console.WriteLine($"  - Botón Arriba  (↑) → GPIO {_pinArriba}");
                Console.WriteLine($"  - Botón Abajo   (↓) → GPIO {_pinAbajo}");
                Console.WriteLine($"  - Botón Select  (✓) → GPIO {_pinSelect}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] No se pudo inicializar GPIO: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Configura un pin individual como entrada con pull-up.
        /// </summary>
        private void ConfigurarPin(int pin)
        {
            _gpio.OpenPin(pin, PinMode.InputPullUp);
        }

        /// <summary>
        /// Inicia el hilo de polling que monitorea los botones continuamente.
        /// </summary>
        private void IniciarPolling()
        {
            _activo = true;
            _threadPolling = new Thread(ProcesoPolling);
            _threadPolling.Start();
        }

        /// <summary>
        /// Loop de polling que ejecuta en un hilo separado.
        /// Revisa el estado de cada botón periódicamente.
        /// </summary>
        private void ProcesoPolling()
        {
            while (_activo)
            {
                try
                {
                    VerificarBoton(_pinArriba, BotonTipo.Arriba, ref _ultimaPulsacionArriba);
                    VerificarBoton(_pinAbajo, BotonTipo.Abajo, ref _ultimaPulsacionAbajo);
                    VerificarBoton(_pinSelect, BotonTipo.Select, ref _ultimaPulsacionSelect);

                    Thread.Sleep(POLLING_INTERVAL_MS);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Error en polling de botones: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Verifica si un botón fue pulsado (transición HIGH → LOW).
        /// Implementa debounce para evitar falsos positivos.
        /// </summary>
        private void VerificarBoton(int pin, BotonTipo tipo, ref long ultimaPulsacion)
        {
            try
            {
                // En ESP32 con pull-up: LOW = pulsado, HIGH = no pulsado
                PinValue estado = _gpio.Read(pin);

                if (estado == PinValue.Low)
                {
                    long ahora = DateTime.UtcNow.Ticks;
                    long tiempoDesdeUltima = (ahora - ultimaPulsacion) / TimeSpan.TicksPerMillisecond;

                    // Solo procesar si pasó suficiente tiempo (debounce)
                    if (tiempoDesdeUltima > DEBOUNCE_MS)
                    {
                        ultimaPulsacion = ahora;
                        RaiseBotonPulsado(tipo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error leyendo botón {tipo}: {ex.Message}");
            }
        }

        /// <summary>
        /// Dispara el evento BotonPulsado con los parámetros correspondientes.
        /// </summary>
        private void RaiseBotonPulsado(BotonTipo boton)
        {
            EventHandler<BotonEventArgs> handler = BotonPulsado;
            if (handler != null)
            {
                var args = new BotonEventArgs
                {
                    Boton = boton,
                    FechaPulsacion = DateTime.UtcNow
                };

                handler(this, args);
            }
        }

        /// <summary>
        /// Detiene el polling y libera los recursos de GPIO.
        /// </summary>
        public void Dispose()
        {
            _activo = false;

            if (_threadPolling != null)
            {
                _threadPolling.Join(1000);  // Esperar a que termine el hilo
            }

            if (_gpio != null)
            {
                _gpio.Dispose();
            }

            Console.WriteLine("[BotonesManager] Recursos liberados");
        }
    }
}
