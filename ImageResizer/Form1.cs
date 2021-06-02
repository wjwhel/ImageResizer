using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//
using System.IO;
using System.Collections;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageResizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            progressBar1.Value = 0;
            InitializeBackgroundWorker();
        }

        private void InitializeBackgroundWorker()
        {
            backgroundWorker1.WorkerReportsProgress = true;

            backgroundWorker1.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += 
                new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            btnSubmit.Enabled = false;
            status.Visible = true;

            string[] parentDir = Directory.GetDirectories(Properties.Settings.Default.InputFolder);

            string[] subDir = Directory.GetDirectories(Properties.Settings.Default.InputFolder, "*", SearchOption.AllDirectories);

            ArrayList allDir = new ArrayList(subDir);

            for (int i = 0; i < parentDir.Length; i++)
            {
                if (allDir.Contains(parentDir[i]))
                {
                    allDir.Remove(parentDir[i]);
                }
            }

            if (backgroundWorker1.IsBusy != true)
            {
                backgroundWorker1.RunWorkerAsync(allDir);
            }
        }
        public static Bitmap resizeImage(Image image)
        {
            var destRect = new Rectangle(0, 0, 1024, 768);
            var destImage = new Bitmap(1024, 768);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            Properties.Settings.Default.InputFolder = folderBrowserDialog1.SelectedPath;
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            Properties.Settings.Default.OutputFolder = folderBrowserDialog1.SelectedPath;
            Properties.Settings.Default.Save();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            ArrayList allDir = (ArrayList)e.Argument;
            int count = 0;
            foreach (string dir in allDir)
            {
                count++;
                int percent = (count * 100) / allDir.Count;
                string RMA_SN = dir.Replace(Properties.Settings.Default.InputFolder + "\\", "");

                string[] subs = RMA_SN.Split("\\");

                string RMA = subs[0];

                string SN = subs[1];

                string[] outfolder = Directory.GetDirectories(Properties.Settings.Default.OutputFolder, RMA + "*");

                DirectoryInfo di = Directory.CreateDirectory(outfolder[0] + "\\Inspection Photos\\" + SN);

                string[] files = Directory.GetFiles(dir);

                for (int i = 0; i < files.Length; i++)
                {
                    Image newImage = Image.FromFile(files[i]);

                    Image resizedImage = resizeImage(newImage);

                    resizedImage.Save(di.FullName + "\\" + RMA + "_" + SN + "_" + Path.GetFileName(files[i]), ImageFormat.Jpeg);
                }
                worker.ReportProgress(percent, RMA + "/" + SN);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            status.Text = "Resizing... " + e.UserState.ToString();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                status.Text = "Complete.";
                progressBar1.Value = 100;
                btnSubmit.Enabled = true;
            }
        }
    }
}
