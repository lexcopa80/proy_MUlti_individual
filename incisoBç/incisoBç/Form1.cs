using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace incisoBç
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Archivos de imagen|*.jpg;*.png;*.bmp";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(openFileDialog1.FileName);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null) return;

            Bitmap bmpOri = new Bitmap(pictureBox1.Image);
            Bitmap bmpRes = new Bitmap(bmpOri.Width, bmpOri.Height);

            for (int x = 1; x < bmpOri.Width - 1; x++)
            {
                for (int y = 1; y < bmpOri.Height - 1; y++)
                {
                    List<int> listaR = new List<int>();
                    List<int> listaG = new List<int>();
                    List<int> listaB = new List<int>();

                    // Llenar la vecindad 3x3
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        for (int ky = -1; ky <= 1; ky++)
                        {
                            Color pixelVecino = bmpOri.GetPixel(x + kx, y + ky);
                            listaR.Add(pixelVecino.R);
                            listaG.Add(pixelVecino.G);
                            listaB.Add(pixelVecino.B);
                        }
                    }

                    // Ordenar de menor a mayor
                    listaR.Sort();
                    listaG.Sort();
                    listaB.Sort();

                    // Tomar el elemento central (la posición 4 es el centro de los 9 elementos)
                    int medianaR = listaR[4];
                    int medianaG = listaG[4];
                    int medianaB = listaB[4];

                    bmpRes.SetPixel(x, y, Color.FromArgb(medianaR, medianaG, medianaB));
                }
            }

            pictureBox2.Image = bmpRes;
            MessageBox.Show("¡Filtro de Mediana aplicado! Ruido eliminado.");
        }
    }
}
