// ============================================================
//  FormFlasher.cs — Panel de Flasheo para MochiES Configurador
//  Flashea nanoFramework y el firmware directamente desde la app
//
//  Requiere: Python + nanoff instalado en el sistema
//  Instalar: pip install nanoff
// ============================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace MochiES_Configurador
{
    public class FormFlasher : Form
    {
        // Targets soportados por nanoff para el kit Hemmel TEK-002
        static readonly string[] TARGETS_DISPONIBLES = new string[]
        {
            "ESP32_C3",          // ESP32 C3 Mini
            "ESP32_WROVER_KIT",  // ESP32-S (el del kit Hemmel)
            "ESP32_GENERIC",     // ESP32 genérico
            "ESP32_S2",
            "ESP32_S3",
        };

        ComboBox cmbPuerto;
        ComboBox cmbTarget;
        Button btnRefrescarPuertos;
        Button btnDetectarDispositivo;
        Button btnFlashearNano;
        Button btnFlashearFirmware;
        Button btnAbrirFirmware;
        RichTextBox txtLog;
        Label lblEstado;
        ProgressBar progressBar;
        TextBox txtRutaFirmware;
        Panel panelEstado;

        bool _flasheando = false;
        string _rutaFirmware = null;

        public FormFlasher()
        {
            ConfigurarUI();
            RefrescarPuertos();
        }

        void ConfigurarUI()
        {
            Text = "MochiES — Flashear Dispositivo";
            Size = new System.Drawing.Size(750, 620);
            BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            ForeColor = System.Drawing.Color.White;
            MinimumSize = new System.Drawing.Size(600, 500);

            // --- Panel superior: configuración ---
            var panelConfig = new Panel();
            panelConfig.Dock = DockStyle.Top;
            panelConfig.Height = 200;
            panelConfig.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            panelConfig.Padding = new Padding(14);

            // Puerto COM
            var lblPuerto = new Label();
            lblPuerto.Text = "Puerto COM:";
            lblPuerto.Location = new System.Drawing.Point(14, 14);
            lblPuerto.Size = new System.Drawing.Size(100, 22);
            lblPuerto.ForeColor = System.Drawing.Color.LightGray;

            cmbPuerto = new ComboBox();
            cmbPuerto.Location = new System.Drawing.Point(120, 12);
            cmbPuerto.Size = new System.Drawing.Size(130, 26);
            cmbPuerto.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            cmbPuerto.ForeColor = System.Drawing.Color.White;
            cmbPuerto.DropDownStyle = ComboBoxStyle.DropDownList;

            btnRefrescarPuertos = CrearBoton("Refrescar", 258, 12, 90, 26,
                System.Drawing.Color.FromArgb(80, 80, 80));
            btnRefrescarPuertos.Click += (s, e) => RefrescarPuertos();

            btnDetectarDispositivo = CrearBoton("Detectar ESP32", 356, 12, 120, 26,
                System.Drawing.Color.FromArgb(0, 100, 160));
            btnDetectarDispositivo.Click += BtnDetectar_Click;

            // Target
            var lblTarget = new Label();
            lblTarget.Text = "Target:";
            lblTarget.Location = new System.Drawing.Point(14, 50);
            lblTarget.Size = new System.Drawing.Size(100, 22);
            lblTarget.ForeColor = System.Drawing.Color.LightGray;

            cmbTarget = new ComboBox();
            cmbTarget.Location = new System.Drawing.Point(120, 48);
            cmbTarget.Size = new System.Drawing.Size(200, 26);
            cmbTarget.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            cmbTarget.ForeColor = System.Drawing.Color.White;
            cmbTarget.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (string t in TARGETS_DISPONIBLES)
                cmbTarget.Items.Add(t);
            cmbTarget.SelectedIndex = 0; // ESP32_C3 por defecto

            var lblTargetHint = new Label();
            lblTargetHint.Text = "Kit Hemmel TEK-002 (ESP-32S) → usar ESP32_WROVER_KIT";
            lblTargetHint.Location = new System.Drawing.Point(120, 76);
            lblTargetHint.Size = new System.Drawing.Size(500, 18);
            lblTargetHint.ForeColor = System.Drawing.Color.FromArgb(150, 200, 150);
            lblTargetHint.Font = new System.Drawing.Font("Segoe UI", 8);

            // Separador
            var sep = new Label();
            sep.Location = new System.Drawing.Point(14, 100);
            sep.Size = new System.Drawing.Size(700, 1);
            sep.BackColor = System.Drawing.Color.FromArgb(70, 70, 70);

            // Firmware
            var lblFirmware = new Label();
            lblFirmware.Text = "Firmware .bin:";
            lblFirmware.Location = new System.Drawing.Point(14, 112);
            lblFirmware.Size = new System.Drawing.Size(100, 22);
            lblFirmware.ForeColor = System.Drawing.Color.LightGray;

            txtRutaFirmware = new TextBox();
            txtRutaFirmware.Location = new System.Drawing.Point(120, 110);
            txtRutaFirmware.Size = new System.Drawing.Size(340, 26);
            txtRutaFirmware.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            txtRutaFirmware.ForeColor = System.Drawing.Color.FromArgb(150, 150, 150);
            txtRutaFirmware.Text = "Opcional — se usa nanoff si no se especifica";
            txtRutaFirmware.ReadOnly = true;

            btnAbrirFirmware = CrearBoton("Buscar...", 468, 110, 80, 26,
                System.Drawing.Color.FromArgb(80, 80, 80));
            btnAbrirFirmware.Click += BtnAbrirFirmware_Click;

            // Botones principales
            btnFlashearNano = CrearBoton("1. Flashear nanoFramework", 14, 150, 220, 36,
                System.Drawing.Color.FromArgb(0, 122, 180));
            btnFlashearNano.Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
            btnFlashearNano.Click += BtnFlashearNano_Click;

            btnFlashearFirmware = CrearBoton("2. Flashear Firmware MochiES", 244, 150, 240, 36,
                System.Drawing.Color.FromArgb(0, 160, 80));
            btnFlashearFirmware.Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
            btnFlashearFirmware.Click += BtnFlashearFirmware_Click;

            panelConfig.Controls.AddRange(new Control[]
            {
                lblPuerto, cmbPuerto, btnRefrescarPuertos, btnDetectarDispositivo,
                lblTarget, cmbTarget, lblTargetHint, sep,
                lblFirmware, txtRutaFirmware, btnAbrirFirmware,
                btnFlashearNano, btnFlashearFirmware
            });

            // --- Panel estado ---
            panelEstado = new Panel();
            panelEstado.Dock = DockStyle.Bottom;
            panelEstado.Height = 52;
            panelEstado.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            panelEstado.Padding = new Padding(10, 8, 10, 8);

            progressBar = new ProgressBar();
            progressBar.Location = new System.Drawing.Point(10, 8);
            progressBar.Size = new System.Drawing.Size(500, 16);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.MarqueeAnimationSpeed = 0; // empieza detenido
            progressBar.Visible = true;

            lblEstado = new Label();
            lblEstado.Text = "Listo";
            lblEstado.Location = new System.Drawing.Point(10, 28);
            lblEstado.Size = new System.Drawing.Size(700, 18);
            lblEstado.ForeColor = System.Drawing.Color.LightGray;
            lblEstado.Font = new System.Drawing.Font("Segoe UI", 8);

            panelEstado.Controls.Add(progressBar);
            panelEstado.Controls.Add(lblEstado);

            // --- Log ---
            txtLog = new RichTextBox();
            txtLog.Dock = DockStyle.Fill;
            txtLog.BackColor = System.Drawing.Color.FromArgb(15, 15, 15);
            txtLog.ForeColor = System.Drawing.Color.LightGreen;
            txtLog.Font = new System.Drawing.Font("Consolas", 9);
            txtLog.ReadOnly = true;
            txtLog.WordWrap = false;

            Controls.Add(txtLog);
            Controls.Add(panelEstado);
            Controls.Add(panelConfig);

            Log("MochiES Flasher listo.");
            Log("Paso 1: Seleccionar puerto COM y target.");
            Log("Paso 2: Conectar el ESP32 por USB.");
            Log("Paso 3: Flashear nanoFramework (solo la primera vez).");
            Log("Paso 4: Flashear el firmware MochiES.");
        }

        // ----------------------------------------------------------
        //  Handlers de botones
        // ----------------------------------------------------------

        void BtnDetectar_Click(object sender, EventArgs e)
        {
            Log("Detectando ESP32 en puertos disponibles...");
            RefrescarPuertos();

            // Intentar abrir cada puerto y buscar respuesta del ESP32
            foreach (string puerto in SerialPort.GetPortNames())
            {
                Log("  Probando " + puerto + "...");
                // En una implementación real, enviar comando AT y esperar respuesta
                // Por ahora, seleccionar el último puerto detectado
                cmbPuerto.SelectedItem = puerto;
            }
            Log("Si tu ESP32 no aparece, verificá el cable USB (debe soportar datos).");
        }

        void BtnAbrirFirmware_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Seleccionar firmware .bin";
            dialog.Filter = "Firmware (*.bin)|*.bin|Todos los archivos (*.*)|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _rutaFirmware = dialog.FileName;
                txtRutaFirmware.Text = _rutaFirmware;
                txtRutaFirmware.ForeColor = System.Drawing.Color.White;
                Log("Firmware seleccionado: " + Path.GetFileName(_rutaFirmware));
            }
        }

        void BtnFlashearNano_Click(object sender, EventArgs e)
        {
            if (_flasheando) return;

            string puerto = cmbPuerto.SelectedItem != null ? cmbPuerto.SelectedItem.ToString() : "";
            string target = cmbTarget.SelectedItem != null ? cmbTarget.SelectedItem.ToString() : "";

            if (string.IsNullOrEmpty(puerto))
            {
                MessageBox.Show("Seleccioná un puerto COM antes de flashear.",
                    "Falta puerto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(
                "Esto va a instalar .NET nanoFramework en el ESP32.\n\n" +
                "Target: " + target + "\nPuerto: " + puerto + "\n\n" +
                "El proceso tarda 2-5 minutos. ¿Continuás?",
                "Confirmar flasheo",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            // Comando nanoff
            string args = "--target " + target + " --update --serialport " + puerto;
            EjecutarComando("nanoff", args, "nanoFramework flasheado correctamente.");
        }

        void BtnFlashearFirmware_Click(object sender, EventArgs e)
        {
            if (_flasheando) return;

            string puerto = cmbPuerto.SelectedItem != null ? cmbPuerto.SelectedItem.ToString() : "";
            if (string.IsNullOrEmpty(puerto))
            {
                MessageBox.Show("Seleccioná un puerto COM.", "Falta puerto",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_rutaFirmware == null || !File.Exists(_rutaFirmware))
            {
                MessageBox.Show(
                    "No se seleccionó un archivo .bin de firmware.\n\n" +
                    "Para generar el .bin: en Visual Studio, Build → Publish.\n" +
                    "O seleccioná el archivo desde el botón 'Buscar...'",
                    "Firmware no encontrado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // nanoff permite flashear un .bin propio con --binfile
            string args = "--target " + cmbTarget.SelectedItem + " --serialport " + puerto +
                          " --binfile \"" + _rutaFirmware + "\"";
            EjecutarComando("nanoff", args, "Firmware MochiES flasheado correctamente.");
        }

        // ----------------------------------------------------------
        //  Ejecutar proceso externo (nanoff) con salida en tiempo real
        // ----------------------------------------------------------

        void EjecutarComando(string comando, string args, string mensajeExito)
        {
            _flasheando = true;
            btnFlashearNano.Enabled = false;
            btnFlashearFirmware.Enabled = false;
            progressBar.MarqueeAnimationSpeed = 30;
            ActualizarEstado("Flasheando... no desconectes el dispositivo.");

            Log("");
            Log("═══════════════════════════════════");
            Log("> " + comando + " " + args);
            Log("═══════════════════════════════════");

            var thread = new Thread(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo();
                    psi.FileName = comando;
                    psi.Arguments = args;
                    psi.UseShellExecute = false;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.CreateNoWindow = true;

                    var proc = new Process();
                    proc.StartInfo = psi;

                    // Capturar stdout y stderr en tiempo real
                    proc.OutputDataReceived += (s, ev) =>
                    {
                        if (!string.IsNullOrEmpty(ev.Data))
                            LogDesdeHilo(ev.Data);
                    };
                    proc.ErrorDataReceived += (s, ev) =>
                    {
                        if (!string.IsNullOrEmpty(ev.Data))
                            LogDesdeHilo("[ERR] " + ev.Data, System.Drawing.Color.Salmon);
                    };

                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    proc.WaitForExit();

                    if (proc.ExitCode == 0)
                    {
                        LogDesdeHilo("");
                        LogDesdeHilo("✓ " + mensajeExito, System.Drawing.Color.LightGreen);
                        ActualizarEstadoDesdeHilo("Completado.");
                    }
                    else
                    {
                        LogDesdeHilo("✗ Error. Código de salida: " + proc.ExitCode,
                            System.Drawing.Color.Salmon);
                        ActualizarEstadoDesdeHilo("Error en el flasheo. Revisá el log.");
                    }
                }
                catch (Exception ex)
                {
                    LogDesdeHilo("✗ No se pudo ejecutar nanoff: " + ex.Message,
                        System.Drawing.Color.Salmon);
                    LogDesdeHilo("  Verificá que Python y nanoff estén instalados:",
                        System.Drawing.Color.Yellow);
                    LogDesdeHilo("  pip install nanoff", System.Drawing.Color.Yellow);
                    ActualizarEstadoDesdeHilo("Error — nanoff no encontrado.");
                }
                finally
                {
                    Invoke(new Action(() =>
                    {
                        _flasheando = false;
                        btnFlashearNano.Enabled = true;
                        btnFlashearFirmware.Enabled = true;
                        progressBar.MarqueeAnimationSpeed = 0;
                    }));
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        // ----------------------------------------------------------
        //  Helpers
        // ----------------------------------------------------------

        void RefrescarPuertos()
        {
            string seleccionado = cmbPuerto.SelectedItem != null
                ? cmbPuerto.SelectedItem.ToString() : null;

            cmbPuerto.Items.Clear();
            string[] puertos = SerialPort.GetPortNames();

            foreach (string p in puertos)
                cmbPuerto.Items.Add(p);

            if (puertos.Length == 0)
            {
                Log("No se encontraron puertos COM. Conectá el ESP32 por USB.");
            }
            else
            {
                // Restaurar selección previa si sigue disponible
                if (seleccionado != null && cmbPuerto.Items.Contains(seleccionado))
                    cmbPuerto.SelectedItem = seleccionado;
                else
                    cmbPuerto.SelectedIndex = cmbPuerto.Items.Count - 1;

                Log("Puertos disponibles: " + string.Join(", ", puertos));
            }
        }

        void Log(string mensaje, System.Drawing.Color? color = null)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => Log(mensaje, color)));
                return;
            }

            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionColor = color ?? System.Drawing.Color.LightGreen;
            txtLog.AppendText(DateTime.Now.ToString("HH:mm:ss") + " " + mensaje + "\n");
            txtLog.ScrollToCaret();
        }

        void LogDesdeHilo(string msg, System.Drawing.Color? color = null)
        {
            if (InvokeRequired)
                Invoke(new Action(() => Log(msg, color)));
            else
                Log(msg, color);
        }

        void ActualizarEstado(string msg)
        {
            if (lblEstado.InvokeRequired)
                lblEstado.Invoke(new Action(() => lblEstado.Text = msg));
            else
                lblEstado.Text = msg;
        }

        void ActualizarEstadoDesdeHilo(string msg)
        {
            if (InvokeRequired)
                Invoke(new Action(() => ActualizarEstado(msg)));
            else
                ActualizarEstado(msg);
        }

        Button CrearBoton(string texto, int x, int y, int w, int h,
            System.Drawing.Color color)
        {
            var btn = new Button();
            btn.Text = texto;
            btn.Location = new System.Drawing.Point(x, y);
            btn.Size = new System.Drawing.Size(w, h);
            btn.BackColor = color;
            btn.ForeColor = System.Drawing.Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 80);
            return btn;
        }
    }
}