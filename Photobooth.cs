/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Threading;

namespace SnapShot
{
	/// <summary>
	/// Summary description for Photobooth.
	/// </summary>
    public class Photobooth : System.Windows.Forms.Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private PictureBox cameraPreview;
        private Label countdownTime;
        private Capture cam;

        public void Countdown(int from)
        {
            countdownTime.Invoke(new ThreadStart(delegate{
                countdownTime.Visible = true;
            }));
            for (int i = from; i > 0; i--)
            {
                countdownTime.Invoke(new ThreadStart(delegate{
                    countdownTime.Text = i.ToString();
                }));
                Thread.Sleep(1000);
            }
            countdownTime.Invoke(new ThreadStart(delegate{
                countdownTime.Visible = false;
            }));
        }

        string watermarkText;
        int watermarkSize;
        public Photobooth()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();


            PrinterPicker picker = new PrinterPicker();
            picker.ShowDialog();
            printerName = picker.PrinterName;

            Prompt p = new Prompt("What text would you like to display as a watermark?");
            p.ShowDialog();
            watermarkText = p.Result;

            Prompt s = new Prompt("What size would you like the text?");
            s.ShowDialog();
            try
            {
                watermarkSize = int.Parse(s.Result);
            }
            catch
            {
                watermarkSize = 0;
            }

            const int VIDEODEVICE = 0; // zero based index of video capture device to use
            const int VIDEOWIDTH = 640; // Depends on video device caps
            const int VIDEOHEIGHT = 480; // Depends on video device caps
            const int VIDEOBITSPERPIXEL = 24; // BitsPerPixel values determined by device

            cam = new Capture(VIDEODEVICE, VIDEOWIDTH, VIDEOHEIGHT, VIDEOBITSPERPIXEL);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                updateVideoThread.Abort();
                if (components != null) 
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );

            if (m_ip != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(m_ip);
                m_ip = IntPtr.Zero;
            }
        }

		#region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cameraPreview = new System.Windows.Forms.PictureBox();
            this.countdownTime = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.cameraPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // cameraPreview
            // 
            this.cameraPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cameraPreview.Location = new System.Drawing.Point(228, 277);
            this.cameraPreview.Name = "cameraPreview";
            this.cameraPreview.Size = new System.Drawing.Size(320, 240);
            this.cameraPreview.TabIndex = 3;
            this.cameraPreview.TabStop = false;
            // 
            // countdownTime
            // 
            this.countdownTime.AutoSize = true;
            this.countdownTime.BackColor = System.Drawing.Color.Black;
            this.countdownTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.countdownTime.ForeColor = System.Drawing.Color.White;
            this.countdownTime.Location = new System.Drawing.Point(12, 9);
            this.countdownTime.Name = "countdownTime";
            this.countdownTime.Size = new System.Drawing.Size(0, 108);
            this.countdownTime.TabIndex = 4;
            this.countdownTime.Visible = false;
            // 
            // Photobooth
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(776, 794);
            this.Controls.Add(this.countdownTime);
            this.Controls.Add(this.cameraPreview);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Photobooth";
            this.Text = "Photobooth";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Photobooth_FormClosed);
            this.Load += new System.EventHandler(this.Photobooth_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Photobooth_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.cameraPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
		#endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() 
        {
            Application.Run(new Photobooth());
        }

        IntPtr m_ip = IntPtr.Zero;

        Bitmap i1;
        Bitmap i2;
        Bitmap i3;
        Bitmap i4;

        private Bitmap CaptureFromWebcam()
        {
            // Release any previous buffer
            if (m_ip != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(m_ip);
                m_ip = IntPtr.Zero;
            }

            // capture image
            m_ip = cam.Click();
            Bitmap b = new Bitmap(cam.Width, cam.Height, cam.Stride, PixelFormat.Format24bppRgb, m_ip);

            // If the image is upsidedown
            b.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return b;
        }

        public Bitmap Lighten(Bitmap bitmap, int amount)
        {
            if (amount < -255 || amount > 255)
                return bitmap;

            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            int nVal = 0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - bitmap.Width * 3;
                int nWidth = bitmap.Width * 3;

                for (int y = 0; y < bitmap.Height; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        nVal = (int)(p[0] + amount);

                        if (nVal < 0) nVal = 0;
                        if (nVal > 255) nVal = 255;

                        p[0] = (byte)nVal;

                        ++p;
                    }
                    p += nOffset;
                }
            }

            bitmap.UnlockBits(bmData);

            return bitmap;
        }

        private void FlashScreen()
        {
            (new Thread(new ThreadStart(delegate
            {
                doUpdate = false;

                int steps = 3;
                int halfSteps = steps / 2;

                for (int i = 0; i < steps; i++)
                {
                    int amount = 255;

                    if (i < halfSteps)
                    {
                        amount = (int)(amount * ((float)i / steps));
                    }
                    else
                    {
                        amount = (int)(amount * ((float)(steps - i) / steps));
                    }

                    cameraPreview.Image = ResizeImage(Lighten(CaptureFromWebcam(), amount), this.Width, this.Height);

                }

                doUpdate = true;
            }))).Start();
            
        }

        private Bitmap ResizeImage(Bitmap i, int newWidth, int newHeight)
        {
            Bitmap newImage = new Bitmap(newWidth, newHeight);
            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(i, new Rectangle(0, 0, newWidth, newHeight));
            }

            return newImage;
        }

        private void PrintImage(object o, PrintPageEventArgs e)
        {
            var width = e.PageBounds.Width;
            var height = e.PageBounds.Height;

            i1 = ResizeImage(i1, width / 2, height / 2);
            i2 = ResizeImage(i2, width / 2, height / 2);
            i3 = ResizeImage(i3, width / 2, height / 2);
            i4 = ResizeImage(i4, width / 2, height / 2);

            var l1 = new Point(0, 0);
            var l2 = new Point(0, height / 2);
            var l3 = new Point(width / 2, 0);
            var l4 = new Point(width / 2, height / 2);
            
            e.Graphics.DrawImage(i1, l1);
            e.Graphics.DrawImage(i2, l2);
            e.Graphics.DrawImage(i3, l3);
            e.Graphics.DrawImage(i4, l4);

            var font = new System.Drawing.Font("Arial", watermarkSize);

            e.Graphics.DrawString(watermarkText, font, Brushes.White, new PointF(1, 1));
            e.Graphics.DrawString(watermarkText, font, Brushes.Black, new PointF(0, 0));
        }

        private void Photobooth_FormClosed(object sender, FormClosedEventArgs e)
        {
            cam.Dispose();

            if (m_ip != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(m_ip);
                m_ip = IntPtr.Zero;
            }
        }

        Thread updateVideoThread;
        private bool doUpdate = true;
        private void UpdateVideoThread()
        {
            while (true)
            {
                if (doUpdate)
                {
                    cameraPreview.Image = ResizeImage(CaptureFromWebcam(), cameraPreview.Width, cameraPreview.Height);
                }
                Thread.Sleep(30);
            }
        }

        private string printerName = null;
        private void Photobooth_Load(object sender, EventArgs e)
        {
            updateVideoThread = new Thread(new ThreadStart(UpdateVideoThread));
            updateVideoThread.Start();

            Bounds = Screen.PrimaryScreen.Bounds;
            Width = Width + 100;
            Height = Height + 100;
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;

            cameraPreview.Location = new Point(0, 0);
            cameraPreview.Width = this.Width;
            cameraPreview.Height = this.Height;
        }

        private void Photobooth_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'q')
            {
                Application.Exit();
            }
            else if (e.KeyChar == ' ')
            {
                (new Thread(new ThreadStart(delegate
                {
                    Countdown(5);
                    i1 = CaptureFromWebcam();
                    FlashScreen();

                    Countdown(3);
                    i2 = CaptureFromWebcam();
                    FlashScreen();

                    Countdown(3);
                    i3 = CaptureFromWebcam();
                    FlashScreen();

                    Countdown(3);
                    i4 = CaptureFromWebcam();
                    FlashScreen();

                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += PrintImage;
                    pd.PrinterSettings.PrinterName = printerName;
                    pd.Print();
                }))).Start();
            }
        }
    }
}
