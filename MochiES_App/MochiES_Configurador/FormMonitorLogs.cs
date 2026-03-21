using System;
using System.IO;
using System.Windows.Forms;

namespace MochiES_Configurador
{
    /// <summary>
    /// Panel de monitoreo de logs del ESP32
    /// Captura, filtra y guarda logs en tiempo real
    /// </summary>
    public class FormMonitorLogs : Form
    {
        private RichTextBox txtLogs;
        private Button btnLimpiar;
        private Button btnGuardarLogs;
        private Button btnFiltrar;
        private ComboBox cmbFiltro;
        private Label lblContador;
        private int _contadorLineas = 0;

        public FormMonitorLogs()
        {
            ConfigurarUI();
        }

        private void ConfigurarUI()
        {
            BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            ForeColor = System.Drawing.Color.White;
            Size = new System.Drawing.Size(700, 600);

            // === TOOLBAR ===
            var panelToolbar = new Panel();
            panelToolbar.Dock = DockStyle.Top;
            panelToolbar.Height = 50;
            panelToolbar.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            panelToolbar.Padding = new Padding(10);

            btnLimpiar = new Button();
            btnLimpiar.Text = "Limpiar";
            btnLimpiar.Location = new System.Drawing.Point(10, 10);
            btnLimpiar.Click += BtnLimpiar_Click;

            btnGuardarLogs = new Button();
            btnGuardarLogs.Text = "Guardar Logs";
            btnGuardarLogs.Location = new System.Drawing.Point(100, 10);
            btnGuardarLogs.Click += BtnGuardarLogs_Click;

            var lblFiltro = new Label();
            lblFiltro.Text = "Filtrar por módulo:";
            lblFiltro.Location = new System.Drawing.Point(230, 15);
            lblFiltro.AutoSize = true;

            cmbFiltro = new ComboBox();
            cmbFiltro.Location = new System.Drawing.Point(350, 12);
            cmbFiltro.Width = 100;
            cmbFiltro.Items.AddRange(new[] { "Todos", "[M1]", "[M2]", "[M3]", "[M4]", "[ERROR]" });
            cmbFiltro.SelectedIndex = 0;
            cmbFiltro.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            cmbFiltro.ForeColor = System.Drawing.Color.White;

            lblContador = new Label();
            lblContador.Text = "Líneas: 0";
            lblContador.Location = new System.Drawing.Point(500, 15);
            lblContador.AutoSize = true;

            panelToolbar.Controls.Add(btnLimpiar);
            panelToolbar.Controls.Add(btnGuardarLogs);
            panelToolbar.Controls.Add(lblFiltro);
            panelToolbar.Controls.Add(cmbFiltro);
            panelToolbar.Controls.Add(lblContador);

            // === ÁREA DE LOGS ===
            txtLogs = new RichTextBox();
            txtLogs.Dock = DockStyle.Fill;
            txtLogs.ReadOnly = true;
            txtLogs.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            txtLogs.ForeColor = System.Drawing.Color.LimeGreen;
            txtLogs.Font = new System.Drawing.Font("Consolas", 9);

            Controls.Add(panelToolbar);
            Controls.Add(txtLogs);
        }

        public void AgregarLog(string mensaje)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AgregarLog(mensaje)));
                return;
            }

            string filtroSeleccionado = cmbFiltro.SelectedItem?.ToString() ?? "Todos";

            // Aplicar filtro
            if (filtroSeleccionado != "Todos" && !mensaje.Contains(filtroSeleccionado))
            {
                return;
            }

            txtLogs.AppendText($"[{DateTime.Now:HH:mm:ss}] {mensaje}\n");
            _contadorLineas++;
            lblContador.Text = $"Líneas: {_contadorLineas}";
            txtLogs.ScrollToCaret();
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            txtLogs.Clear();
            _contadorLineas = 0;
            lblContador.Text = "Líneas: 0";
        }

        private void BtnGuardarLogs_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Archivos de texto|*.txt|Todos|*.*";
            sfd.FileName = $"MochiES_Logs_{DateTime.Now:yyyy-MM-dd_HHmmss}.txt";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, txtLogs.Text);
                MessageBox.Show($"Logs guardados en: {sfd.FileName}");
            }
        }
    }
}