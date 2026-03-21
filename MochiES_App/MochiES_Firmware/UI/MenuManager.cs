using System;
using MochiES.Drivers;

namespace MochiES.UI
{
    /// <summary>
    /// Gestor de menús del asistente MochiES.
    /// Maneja la navegación entre los diferentes modos y pantallas.
    /// Modos disponibles: Reloj | Clima | IA | Notificaciones
    /// </summary>
    internal class MenuManager
    {
        // --- Enumeración de modos de operación ---
        public enum ModoOperacion
        {
            Reloj,
            Clima,
            ConsultaIA,
            Notificaciones,
            Config
        }

        // --- Propiedades ---
        public ModoOperacion ModoActual { get; private set; }
        public IDisplayDriver Display { get; private set; }

        // --- Lista de modos disponibles (orden de navegación) ---
        private ModoOperacion[] _modos;
        private int _indiceModoCurrent;

        /// <summary>
        /// Constructor del gestor de menús.
        /// </summary>
        /// <param name="display">Interfaz del display a usar</param>
        public MenuManager(IDisplayDriver display)
        {
            Display = display;

            // Inicializar array de modos en orden de navegación
            _modos = new ModoOperacion[]
            {
                ModoOperacion.Reloj,
                ModoOperacion.Clima,
                ModoOperacion.ConsultaIA,
                ModoOperacion.Notificaciones
            };

            _indiceModoCurrent = 0;
            ModoActual = _modos[_indiceModoCurrent];

            MostrarModoActual();
        }

        /// <summary>
        /// Navega al siguiente modo (botón abajo).
        /// </summary>
        public void NavigarAbajo()
        {
            _indiceModoCurrent++;
            if (_indiceModoCurrent >= _modos.Length)
            {
                _indiceModoCurrent = 0;  // Volver al principio
            }

            ModoActual = _modos[_indiceModoCurrent];
            MostrarModoActual();

            Console.WriteLine($"[MenuManager] Navegación abajo → Modo: {ModoActual}");
        }

        /// <summary>
        /// Navega al modo anterior (botón arriba).
        /// </summary>
        public void NavigarArriba()
        {
            _indiceModoCurrent--;
            if (_indiceModoCurrent < 0)
            {
                _indiceModoCurrent = _modos.Length - 1;  // Ir al final
            }

            ModoActual = _modos[_indiceModoCurrent];
            MostrarModoActual();

            Console.WriteLine($"[MenuManager] Navegación arriba → Modo: {ModoActual}");
        }

        /// <summary>
        /// Confirma la selección del modo actual (botón select).
        /// Este evento puede usarse para entrar en submodos o acciones específicas.
        /// </summary>
        public event EventHandler<EventArgs> ModoSeleccionado;

        /// <summary>
        /// Ejecuta la acción del botón select en el modo actual.
        /// </summary>
        public void ConfirmarSeleccion()
        {
            Console.WriteLine($"[MenuManager] Modo seleccionado: {ModoActual}");

            ModoSeleccionado?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Muestra el modo actual en el display.
        /// Formato: primera línea = nombre del modo, segunda línea = instrucciones.
        /// </summary>
        public void MostrarModoActual()
        {
            Display.Limpiar();

            string linea1 = "";
            string linea2 = "";

            switch (ModoActual)
            {
                case ModoOperacion.Reloj:
                    linea1 = "RELOJ";
                    linea2 = "Presiona OK";
                    break;

                case ModoOperacion.Clima:
                    linea1 = "CLIMA";
                    linea2 = "Presiona OK";
                    break;

                case ModoOperacion.ConsultaIA:
                    linea1 = "CONSULTA IA";
                    linea2 = "Presiona OK";
                    break;

                case ModoOperacion.Notificaciones:
                    linea1 = "NOTIFICACIONES";
                    linea2 = "Presiona OK";
                    break;

                case ModoOperacion.Config:
                    linea1 = "CONFIGURACION";
                    linea2 = "Presiona OK";
                    break;
            }

            Display.MostrarTextoPorLinea(0, linea1);
            Display.MostrarTextoPorLinea(1, linea2);
        }

        /// <summary>
        /// Obtiene el nombre del modo actual como string.
        /// </summary>
        public string ObtenerNombreModoActual()
        {
            return ModoActual.ToString();
        }

        /// <summary>
        /// Obtiene la lista de todos los modos disponibles.
        /// </summary>
        public ModoOperacion[] ObtenerModos()
        {
            return _modos;
        }
    }
}
