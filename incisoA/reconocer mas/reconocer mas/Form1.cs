using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace reconocer_mas
{
    public partial class Form1 : Form
    {
        string cadenaConexion = @"Data Source=DESKTOP-1OUMH4H;Initial Catalog=ReconocimientoMapas;Integrated Security=True";
        Color colorMuestra = Color.White;
        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image == null) return;
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            int x = e.X * bmp.Width / pictureBox1.Width;
            int y = e.Y * bmp.Height / pictureBox1.Height;
            colorMuestra = bmp.GetPixel(x, y);
            pictureBox1.BackColor = colorMuestra;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(openFileDialog1.FileName);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(cadenaConexion))
            {
                string query = "INSERT INTO MuestrasColor (NombreZona, R, G, B) VALUES (@nom, @r, @g, @b)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@nom", textBox1.Text);
                cmd.Parameters.AddWithValue("@r", colorMuestra.R);
                cmd.Parameters.AddWithValue("@g", colorMuestra.G);
                cmd.Parameters.AddWithValue("@b", colorMuestra.B);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            MessageBox.Show("Muestra guardada.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null) return;

            Bitmap bmpOri = new Bitmap(pictureBox1.Image);
            Bitmap bmpRes = new Bitmap(bmpOri.Width, bmpOri.Height);

            // Cargar muestras de la BD a una lista para mayor velocidad
            List<Muestra> listaMuestras = new List<Muestra>();
            using (SqlConnection con = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand("SELECT NombreZona, R, G, B FROM MuestrasColor", con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    listaMuestras.Add(new Muestra
                    {
                        Nombre = dr["NombreZona"].ToString().ToLower(),
                        R = (int)dr["R"],
                        G = (int)dr["G"],
                        B = (int)dr["B"]
                    });
                }
            }

            // Recorrer por bloques de 10x10
            for (int i = 0; i < bmpOri.Width; i += 10)
            {
                for (int j = 0; j < bmpOri.Height; j += 10)
                {
                    // Obtener promedio de color del bloque
                    Color promedio = ObtenerPromedioBloque(bmpOri, i, j);

                    string zonaDetectada = "ninguna";
                    int distMin = 50; // Umbral de coincidencia

                    foreach (var m in listaMuestras)
                    {
                        int d = (int)Math.Sqrt(Math.Pow(m.R - promedio.R, 2) + Math.Pow(m.G - promedio.G, 2) + Math.Pow(m.B - promedio.B, 2));
                        if (d < distMin)
                        {
                            distMin = d;
                            zonaDetectada = m.Nombre;
                        }
                    }

                    // Pintar el bloque 10x10 en la imagen de resultado
                    Color colorFinal;
                    if (zonaDetectada == "vegetacion" || zonaDetectada == "pasto" || zonaDetectada == "cesped")
                    {
                        colorFinal = Color.FromArgb(0, 180, 0); // Verde para Césped
                    }
                    else if (zonaDetectada == "tierra")
                    {
                        colorFinal = Color.FromArgb(139, 90, 43); // Café/Marrón para Tierra
                    }
                    else if (zonaDetectada == "asfalto")
                    {
                        colorFinal = Color.FromArgb(180, 50, 250); // Gris Oscuro para Asfalto
                    }
                    else if (zonaDetectada == "cemento")
                    {
                        colorFinal = Color.FromArgb(170, 170, 170); // Gris Claro para Cemento
                    }
                    else if (zonaDetectada == "agua")
                    {
                        colorFinal = Color.FromArgb(0, 120, 255); // Azul por si usas mapas con agua
                    }
                    else
                    {
                        // Escala de grises si no coincide
                        int g = (promedio.R + promedio.G + promedio.B) / 3;
                        colorFinal = Color.FromArgb(g, g, g);
                    }

                    PintarBloque(bmpRes, i, j, colorFinal);
                }
            }
            pictureBox2.Image = bmpRes;
            MessageBox.Show("Detección finalizada.");
        }

        private Color ObtenerPromedioBloque(Bitmap b, int x, int y)
        {
            int r = 0, g = 0, bl = 0, count = 0;
            for (int i = x; i < x + 10 && i < b.Width; i++)
            {
                for (int j = y; j < y + 10 && j < b.Height; j++)
                {
                    Color c = b.GetPixel(i, j);
                    r += c.R; g += c.G; bl += c.B;
                    count++;
                }
            }
            return Color.FromArgb(r / count, g / count, bl / count);
        }

        private void PintarBloque(Bitmap b, int x, int y, Color c)
        {
            for (int i = x; i < x + 10 && i < b.Width; i++)
                for (int j = y; j < y + 10 && j < b.Height; j++)
                    b.SetPixel(i, j, c);
        }


    }

    public class Muestra
    {
        public string Nombre { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }
}
