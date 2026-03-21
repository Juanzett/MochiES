using System;
using System.Windows.Forms;

namespace MochiES_Configurador
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormPrincipal());  // ← Cambiar a FormPrincipal
        }
    }
}
