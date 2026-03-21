// ============================================================
//  FormConverter.cs  —  Etapa 2: Herramienta de conversión
//  Convierte GIFs animados a byte arrays para MochiES
//
//  CÓMO USAR:
//    1. Crear proyecto WinForms en Visual Studio
//    2. Instalar NuGet: AnimatedGif
//    3. Pegar este archivo como Form1.cs
//    4. Compilar y ejecutar
//    5. Cargar un GIF → convertir → copiar el C# generado
//       al archivo AnimacionesMochi.cs del firmware
//
//  FORMATO DE SALIDA:
//    El código C# generado tiene el formato:
//    public static byte[][] MiAnimacion { get { return new byte[][] { ... }; } }
//
//    Listo para pegar en AnimacionesMochi.cs
// ============================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MochiES_GifConverter
{
    public class FormConverter : Form
    {
        // Dimensiones target del SSD1306
        const int OLED_WIDTH = 128;
        const int OLED_HEIGHT = 64;

        private string _gifPath = null;
        private List<byte[]> _frames = new List<byte[]>();

        // Controles UI (declarados acá para simplificar, normalmente van en el Designer)
        private Button btnCargar, btnConvertir, btnCopiar;
        private Label lblStatus;
        private TextBox txtNombre;
        private PictureBox picPreview;
        private RichTextBox txtOutput;

        public FormConverter()
        {
            // NO llamar a InitializeComponent() aquí - lo construimos manualmente
            this.ClientSize = new System.Drawing.Size(700, 600);
            ConfigurarUI();
        }

        void ConfigurarUI()
        {
            this.Text = "MochiES — Conversor de GIF";
            this.Size = new Size(700, 600);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            // Panel superior — controles
            Panel panelTop = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(10) };
            panelTop.BackColor = Color.FromArgb(45, 45, 48);

            btnCargar = new Button
            {
                Text = "📂 Cargar GIF",
                Location = new Point(10, 10),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCargar.Click += BtnCargar_Click;

            btnConvertir = new Button
            {
                Text = "⚡ Convertir",
                Location = new Point(170, 10),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(0, 180, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnConvertir.Click += BtnConvertir_Click;

            btnCopiar = new Button
            {
                Text = "📋 Copiar C#",
                Location = new Point(330, 10),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(180, 100, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnCopiar.Click += BtnCopiar_Click;

            lblStatus = new Label
            {
                Text = "Cargá un GIF para empezar",
                Location = new Point(10, 60),
                Size = new Size(660, 50),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10)
            };

            txtNombre = new TextBox
            {
                Text = "MiAnimacion",
                Location = new Point(500, 10),
                Size = new Size(170, 40),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12)
            };

            panelTop.Controls.AddRange(new Control[] { btnCargar, btnConvertir, btnCopiar, txtNombre, lblStatus });

            // Preview del GIF
            picPreview = new PictureBox
            {
                Dock = DockStyle.Left,
                Width = 256,
                BackColor = Color.Black,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            // Caja de texto con el código generado
            txtOutput = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                Text = "// El código C# aparecerá acá después de convertir...\n\n" +
                       "// Copialo y pegalo en AnimacionesMochi.cs del firmware."
            };

            Panel panelMain = new Panel { Dock = DockStyle.Fill };
            panelMain.Controls.Add(txtOutput);
            panelMain.Controls.Add(picPreview);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        void BtnCargar_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Seleccionar GIF animado",
                Filter = "GIF animados (*.gif)|*.gif",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            try
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _gifPath = dialog.FileName;
                    lblStatus.Text = "✅ Cargado: " + Path.GetFileName(_gifPath);
                    btnConvertir.Enabled = true;

                    // Mostrar preview
                    picPreview.Image = Image.FromFile(_gifPath);
                }
            }
            finally
            {
                dialog.Dispose();
            }
        }

        void BtnConvertir_Click(object sender, EventArgs e)
        {
            if (_gifPath == null) return;

            try
            {
                _frames.Clear();
                lblStatus.Text = "⏳ Convirtiendo...";
                Application.DoEvents();

                ConvertirGif(_gifPath);

                lblStatus.Text = "✅ Convertido: " + _frames.Count + " frames de " + OLED_WIDTH + "×" + OLED_HEIGHT + "px";
                btnCopiar.Enabled = true;

                // Mostrar el código generado
                txtOutput.Text = GenerarCodigoCSharp(txtNombre.Text, _frames);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ Error: " + ex.Message;
                MessageBox.Show("Error al convertir:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void BtnCopiar_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtOutput.Text);
            lblStatus.Text = "✅ Código copiado al portapapeles. Pegalo en AnimacionesMochi.cs";
        }

        /// <summary>
        /// Extrae todos los frames del GIF, los redimensiona a 128x64
        /// y los convierte al formato de buffer SSD1306.
        /// </summary>
        void ConvertirGif(string path)
        {
            Image gif = Image.FromFile(path);
            FrameDimension dimension = new FrameDimension(gif.FrameDimensionsList[0]);
            int totalFrames = gif.GetFrameCount(dimension);

            try
            {
                for (int i = 0; i < totalFrames; i++)
                {
                    gif.SelectActiveFrame(dimension, i);

                    // Redimensionar a 128x64 (tamaño OLED)
                    Bitmap bmp = new Bitmap(OLED_WIDTH, OLED_HEIGHT);
                    Graphics g = Graphics.FromImage(bmp);

                    try
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(gif, 0, 0, OLED_WIDTH, OLED_HEIGHT);

                        // Convertir a bitmap monocromático (umbral = 128)
                        byte[] frameBuffer = BitmapABuffer(bmp);
                        _frames.Add(frameBuffer);
                    }
                    finally
                    {
                        g.Dispose();
                        bmp.Dispose();
                    }
                }
            }
            finally
            {
                gif.Dispose();
            }
        }

        /// <summary>
        /// Convierte un Bitmap a buffer SSD1306.
        /// Formato: 1024 bytes, organizado en páginas de 8 filas.
        /// Cada bit = 1 pixel (1=blanco, 0=negro).
        /// </summary>
        byte[] BitmapABuffer(Bitmap bmp)
        {
            byte[] buffer = new byte[OLED_WIDTH * OLED_HEIGHT / 8]; // 1024 bytes

            for (int y = 0; y < OLED_HEIGHT; y++)
            {
                int page = y / 8;
                int bit = y % 8;

                for (int x = 0; x < OLED_WIDTH; x++)
                {
                    Color pixel = bmp.GetPixel(x, y);

                    // Convertir a escala de grises y aplicar umbral (128)
                    int gris = (pixel.R + pixel.G + pixel.B) / 3;
                    bool blanco = gris > 128;

                    if (blanco)
                    {
                        int index = page * OLED_WIDTH + x;
                        buffer[index] |= (byte)(1 << bit);
                    }
                }
            }

            return buffer;
        }

        /// <summary>
        /// Genera el código C# con los frames como byte arrays,
        /// listo para pegar en AnimacionesMochi.cs
        /// </summary>
        string GenerarCodigoCSharp(string nombre, List<byte[]> frames)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// Animación: " + nombre);
            sb.AppendLine("// " + frames.Count + " frames, " + OLED_WIDTH + "×" + OLED_HEIGHT + "px, formato SSD1306");
            sb.AppendLine("// Generado con MochiES GifConverter");
            sb.AppendLine();
            sb.AppendLine("public static byte[][] " + nombre);
            sb.AppendLine("{");
            sb.AppendLine("    get");
            sb.AppendLine("    {");
            sb.AppendLine("        return new byte[][]");
            sb.AppendLine("        {");

            for (int f = 0; f < frames.Count; f++)
            {
                sb.Append("            new byte[] { ");

                byte[] frame = frames[f];
                for (int i = 0; i < frame.Length; i++)
                {
                    sb.Append("0x");
                    sb.Append(frame[i].ToString("X2"));
                    if (i < frame.Length - 1) sb.Append(", ");

                    // Salto de línea cada 16 bytes para legibilidad
                    if ((i + 1) % 16 == 0 && i < frame.Length - 1)
                    {
                        sb.AppendLine();
                        sb.Append("                        ");
                    }
                }

                sb.Append(" }");
                if (f < frames.Count - 1) sb.Append(",");
                sb.AppendLine("  // frame " + f);
            }

            sb.AppendLine("        };");
            sb.AppendLine("    }");
            sb.AppendLine("}");


            // Estadísticas
            sb.AppendLine();
            sb.AppendLine("// Tamaño total en flash: ~" + (frames.Count * frames[0].Length / 1024) + " KB");

            return sb.ToString();
        }
    }
}