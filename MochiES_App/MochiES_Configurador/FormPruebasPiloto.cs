using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace MochiES_Configurador
{
    /// <summary>
    /// Panel de pruebas piloto para MochiES
    /// Permite escribir texto al LCD, simular botones, ver datos en tiempo real
    /// </summary>
    public class FormPruebasPiloto : Form
    {
        private ComboBox cmbPuerto;
        private Button btnConectar;
        private Button btnDesconectar;
        private TextBox txtMensaje;
        private Button btnEnviarAlLcd;
        private Button btnSimularBotonArriba;
        private Button btnSimularBotonAbajo;
        private Button btnSimularBotonSelect;
        private RichTextBox txtRespuestaDisplay;
        private Label lblEstado;
        private SerialPort puerto;

        public FormPruebasPiloto()
        {
            ConfigurarUI();
            puerto = new SerialPort();
            puerto.BaudRate = 115200;
            puerto.DataReceived += Puerto_DataReceived;
        }

        private void ConfigurarUI()
        {
            BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            ForeColor = System.Drawing.Color.White;
            Size = new System.Drawing.Size(600, 500);

            // === SECCIÓN 1: Conexión ===
            var lblConexion = new Label();
            lblConexion.Text = "CONEXIÓN";
            lblConexion.Location = new System.Drawing.Point(14, 14);
            lblConexion.Font = new System.Drawing.Font("Consolas", 12, System.Drawing.FontStyle.Bold);

            var lblPuerto = new Label();
            lblPuerto.Text = "Puerto COM:";
            lblPuerto.Location = new System.Drawing.Point(14, 40);
            lblPuerto.AutoSize = true;

            cmbPuerto = new ComboBox();
            cmbPuerto.Location = new System.Drawing.Point(120, 38);
            cmbPuerto.Width = 100;
            cmbPuerto.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            cmbPuerto.ForeColor = System.Drawing.Color.White;
            cmbPuerto.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPuerto.Items.AddRange(SerialPort.GetPortNames());

            btnConectar = new Button();
            btnConectar.Text = "Conectar";
            btnConectar.Location = new System.Drawing.Point(230, 38);
            btnConectar.Click += BtnConectar_Click;

            btnDesconectar = new Button();
            btnDesconectar.Text = "Desconectar";
            btnDesconectar.Location = new System.Drawing.Point(330, 38);
            btnDesconectar.Enabled = false;
            btnDesconectar.Click += BtnDesconectar_Click;

            lblEstado = new Label();
            lblEstado.Text = "Desconectado";
            lblEstado.Location = new System.Drawing.Point(430, 40);
            lblEstado.ForeColor = System.Drawing.Color.Red;

            // === SECCIÓN 2: Pruebas ===
            var lblPruebas = new Label();
            lblPruebas.Text = "PRUEBAS";
            lblPruebas.Location = new System.Drawing.Point(14, 90);
            lblPruebas.Font = new System.Drawing.Font("Consolas", 12, System.Drawing.FontStyle.Bold);

            var lblMensaje = new Label();
            lblMensaje.Text = "Texto para LCD:";
            lblMensaje.Location = new System.Drawing.Point(14, 120);

            txtMensaje = new TextBox();
            txtMensaje.Location = new System.Drawing.Point(14, 145);
            txtMensaje.Width = 350;
            txtMensaje.Height = 50;
            txtMensaje.Multiline = true;
            txtMensaje.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            txtMensaje.ForeColor = System.Drawing.Color.White;

            btnEnviarAlLcd = new Button();
            btnEnviarAlLcd.Text = "Enviar al LCD";
            btnEnviarAlLcd.Location = new System.Drawing.Point(375, 145);
            btnEnviarAlLcd.Height = 50;
            btnEnviarAlLcd.Click += BtnEnviarAlLcd_Click;
            btnEnviarAlLcd.Enabled = false;

            // === SECCIÓN 3: Simular Botones ===
            var lblBotones = new Label();
            lblBotones.Text = "Simular Botones:";
            lblBotones.Location = new System.Drawing.Point(14, 210);

            btnSimularBotonArriba = new Button();
            btnSimularBotonArriba.Text = "↑ Arriba";
            btnSimularBotonArriba.Location = new System.Drawing.Point(14, 240);
            btnSimularBotonArriba.Click += BtnSimularBotonArriba_Click;
            btnSimularBotonArriba.Enabled = false;

            btnSimularBotonAbajo = new Button();
            btnSimularBotonAbajo.Text = "↓ Abajo";
            btnSimularBotonAbajo.Location = new System.Drawing.Point(110, 240);
            btnSimularBotonAbajo.Click += BtnSimularBotonAbajo_Click;
            btnSimularBotonAbajo.Enabled = false;

            btnSimularBotonSelect = new Button();
            btnSimularBotonSelect.Text = "✓ Select";
            btnSimularBotonSelect.Location = new System.Drawing.Point(206, 240);
            btnSimularBotonSelect.Click += BtnSimularBotonSelect_Click;
            btnSimularBotonSelect.Enabled = false;

            // === SECCIÓN 4: Respuesta ===
            var lblRespuesta = new Label();
            lblRespuesta.Text = "Respuesta del Display:";
            lblRespuesta.Location = new System.Drawing.Point(14, 280);

            txtRespuestaDisplay = new RichTextBox();
            txtRespuestaDisplay.Location = new System.Drawing.Point(14, 305);
            txtRespuestaDisplay.Width = 550;
            txtRespuestaDisplay.Height = 150;
            txtRespuestaDisplay.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            txtRespuestaDisplay.ForeColor = System.Drawing.Color.LimeGreen;
            txtRespuestaDisplay.ReadOnly = true;

            Controls.Add(lblConexion);
            Controls.Add(lblPuerto);
            Controls.Add(cmbPuerto);
            Controls.Add(btnConectar);
            Controls.Add(btnDesconectar);
            Controls.Add(lblEstado);

            Controls.Add(lblPruebas);
            Controls.Add(lblMensaje);
            Controls.Add(txtMensaje);
            Controls.Add(btnEnviarAlLcd);

            Controls.Add(lblBotones);
            Controls.Add(btnSimularBotonArriba);
            Controls.Add(btnSimularBotonAbajo);
            Controls.Add(btnSimularBotonSelect);

            Controls.Add(lblRespuesta);
            Controls.Add(txtRespuestaDisplay);
        }

        private void BtnConectar_Click(object sender, EventArgs e)
        {
            try
            {
                puerto.PortName = cmbPuerto.SelectedItem.ToString();
                puerto.Open();
                lblEstado.Text = "Conectado ✓";
                lblEstado.ForeColor = System.Drawing.Color.LimeGreen;
                btnConectar.Enabled = false;
                btnDesconectar.Enabled = true;
                btnEnviarAlLcd.Enabled = true;
                btnSimularBotonArriba.Enabled = true;
                btnSimularBotonAbajo.Enabled = true;
                btnSimularBotonSelect.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar: {ex.Message}");
            }
        }

        private void BtnDesconectar_Click(object sender, EventArgs e)
        {
            if (puerto.IsOpen)
            {
                puerto.Close();
            }
            lblEstado.Text = "Desconectado";
            lblEstado.ForeColor = System.Drawing.Color.Red;
            btnConectar.Enabled = true;
            btnDesconectar.Enabled = false;
            btnEnviarAlLcd.Enabled = false;
            btnSimularBotonArriba.Enabled = false;
            btnSimularBotonAbajo.Enabled = false;
            btnSimularBotonSelect.Enabled = false;
        }

        private void BtnEnviarAlLcd_Click(object sender, EventArgs e)
        {
            if (puerto.IsOpen && !string.IsNullOrWhiteSpace(txtMensaje.Text))
            {
                string mensaje = $"LCD:{txtMensaje.Text}\n";
                puerto.Write(mensaje);
                txtRespuestaDisplay.AppendText($"[ENVIADO] {mensaje}");
            }
        }

        private void BtnSimularBotonArriba_Click(object sender, EventArgs e)
        {
            if (puerto.IsOpen)
            {
                puerto.Write("BOTON:ARRIBA\n");
                txtRespuestaDisplay.AppendText("[BOTON] Arriba\n");
            }
        }

        private void BtnSimularBotonAbajo_Click(object sender, EventArgs e)
        {
            if (puerto.IsOpen)
            {
                puerto.Write("BOTON:ABAJO\n");
                txtRespuestaDisplay.AppendText("[BOTON] Abajo\n");
            }
        }

        private void BtnSimularBotonSelect_Click(object sender, EventArgs e)
        {
            if (puerto.IsOpen)
            {
                puerto.Write("BOTON:SELECT\n");
                txtRespuestaDisplay.AppendText("[BOTON] Select\n");
            }
        }

        private void Puerto_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string datos = puerto.ReadExisting();
                Invoke(new Action(() =>
                {
                    txtRespuestaDisplay.AppendText($"[RECIBIDO] {datos}");
                    txtRespuestaDisplay.ScrollToCaret();
                }));
            }
            catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (puerto != null && puerto.IsOpen)
                {
                    puerto.Close();
                    puerto.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}