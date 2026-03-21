using System;
using System.Windows.Forms;

namespace MochiES_Configurador
{
    /// <summary>
    /// Formulario principal del Configurador MochiES
    /// Tabs: Flasher | Pruebas Piloto | Monitor de Logs
    /// </summary>
    public class FormPrincipal : Form
    {
        private TabControl tabsMain;
        private FormFlasher panelFlasher;
        private FormPruebasPiloto panelPruebas;
        private FormMonitorLogs panelMonitor;

        public FormPrincipal()
        {
            ConfigurarUI();
            CargarPaneles();
        }

        private void ConfigurarUI()
        {
            Text = "MochiES Control Panel v1.0";
            Size = new System.Drawing.Size(900, 700);
            BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            ForeColor = System.Drawing.Color.White;
            StartPosition = FormStartPosition.CenterScreen;
            Icon = null;  // TODO: Agregar icono del mochies

            // TabControl
            tabsMain = new TabControl();
            tabsMain.Dock = DockStyle.Fill;
            tabsMain.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            tabsMain.ForeColor = System.Drawing.Color.White;

            // Tabs
            var tabFlasher = new TabPage("🔧 Flasher");
            var tabPruebas = new TabPage("⚡ Pruebas Piloto");
            var tabMonitor = new TabPage("📊 Monitor de Logs");

            tabsMain.TabPages.Add(tabFlasher);
            tabsMain.TabPages.Add(tabPruebas);
            tabsMain.TabPages.Add(tabMonitor);

            Controls.Add(tabsMain);
        }

        private void CargarPaneles()
        {
            // Tab 1: Flasher
            panelFlasher = new FormFlasher();
            panelFlasher.Dock = DockStyle.Fill;
            tabsMain.TabPages[0].Controls.Add(panelFlasher);

            // Tab 2: Pruebas Piloto
            panelPruebas = new FormPruebasPiloto();
            panelPruebas.Dock = DockStyle.Fill;
            tabsMain.TabPages[1].Controls.Add(panelPruebas);

            // Tab 3: Monitor de Logs
            panelMonitor = new FormMonitorLogs();
            panelMonitor.Dock = DockStyle.Fill;
            tabsMain.TabPages[2].Controls.Add(panelMonitor);
        }
    }
}