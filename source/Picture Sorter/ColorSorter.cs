using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using System.IO;

namespace Picture_Sorter
{
    public partial class ColorSorter : Form
    {
        public ColorSorter(String s)
        {
            InitializeComponent();
            //set working dir
            textBox1.Text = s;
        }

        private bool stop = true;

        public static Color getDominantColor(FastBitmap bmp, Size w)
        {
            //Used for tally
            long r = 0;
            long g = 0;
            long b = 0;

            long total = 0;

            for (int x = 0; x < w.Width; x++)
            {
                for (int y = 0; y < w.Height; y++)
                {
                    Color clr = bmp.GetPixel(x, y);

                    r += clr.R;
                    g += clr.G;
                    b += clr.B;

                    total++;
                }
            }

            //Calculate average
            r /= total;
            g /= total;
            b /= total;

            return Color.FromArgb((int)Math.Abs(r), (int)Math.Abs(g), (int)Math.Abs(b));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog i = new FolderBrowserDialog();
            if (i.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = i.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!stop)
            {
                stop = true;
                button2.Text = "Sort";
                return;
            }

            stop = false;
            button2.Text = "Stop";


            List<FileInfo> q = new List<FileInfo>();

            string[] fileTypes = { "jpg", "png", "jpeg", "bmp", "gif" };

            for (int i = 0; i < fileTypes.Length; i++)
            {
                q.AddRange(FileSystem.GetDirectoryInfo(textBox1.Text).GetFiles("*." + fileTypes[i]));
            }

            progressBar1.Maximum = q.Count;
            progressBar1.Value = 0;

            DateTime be;
            DateTime af;

            int skippedFiles = 0;
            int erroredFiles = 0;

            for (int i = 0; i < q.Count; i++)
            {
                if (stop) { break; }

                be = DateTime.Now;

                FileInfo x = q[i];

                Image w = null;

                try
                {
                    w = Image.FromFile(x.FullName);

                    if (checkBox1.Checked)
                    {
                        pictureBox1.Image = w.GetThumbnailImage(250, 250, null, IntPtr.Zero);
                        Application.DoEvents();
                    }


                    FastBitmap g = new FastBitmap((Bitmap)w);

                    g.LockImage();


                    Color a = getDominantColor(g, w.Size);

                    string s = ColorManagement.GetNearestWebColor(a).ToString();

                    g.UnlockImage();

                    w.Dispose();

                    if (pictureBox1.Image != null)
                    {
                        pictureBox1.Image.Dispose();
                        pictureBox1.Image = null;
                    }

                    if (File.Exists(Path.Combine(Path.Combine(textBox1.Text, s), x.Name)))
                    {
                        if (checkBox2.Checked)
                        {
                            String fileType = x.Name.Substring(x.Name.IndexOf("."), x.Name.Length - (x.Name.IndexOf(".")));
                            String fileName = x.Name.Substring(0, x.Name.IndexOf("."));
                            FileSystem.MoveFile(x.FullName, Path.Combine(Path.Combine(textBox1.Text, s), fileName + " - 1" + fileType));
                        }
                        else
                        {
                            //skip file
                            skippedFiles++;
                        }
                    }
                    else
                    {
                        FileSystem.MoveFile(x.FullName, Path.Combine(Path.Combine(textBox1.Text, s), x.Name));
                    }

                    af = DateTime.Now;

                    TimeSpan ss = (af - be);

                    label1.Text = (i + 1).ToString() + "/" + q.Count.ToString() + " , " + ((int)(1 / ss.TotalSeconds)).ToString() + " file/sec.";

                    progressBar1.Value = (i + 1);
                    Application.DoEvents();

                }
                catch (OutOfMemoryException ome)
                {

                    Console.WriteLine(ome);
                    Console.WriteLine(x.Name + "may be corrupted");
                    erroredFiles++;
                }
            }
            
            label1.Text = "Finished"+((erroredFiles>0)?" - "+erroredFiles+" failed":"")+ ((skippedFiles > 0) ? " - " + skippedFiles + " skipped" : "");

            stop = true;
            button2.Text = "Sort";
        }

    }
}
