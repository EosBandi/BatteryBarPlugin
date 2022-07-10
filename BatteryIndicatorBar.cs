using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissionPlanner.BatteryBarPlugin
{
    public partial class BatteryIndicatorBar : UserControl
    {

        private float _batteryCapacity;

        public float BatteryCapacity
        {
            get { return _batteryCapacity; }
            set { _batteryCapacity = value; }
        }

        private float _usedMah;

        public float UsedMah  
        {
            get { return _usedMah; }
            set { _usedMah = value; }
        }

        private float _distFromHome;

        public float DistFromHome
        {
            get { return _distFromHome; }
            set { _distFromHome = value; }
        }

        public float mahPerMinute { get; set; }
        public float avgSpeed { get; set; }


        private readonly Brush _brushbar = new SolidBrush(Color.FromArgb(50, Color.White));
        private readonly Brush _brushGreen = new SolidBrush(Color.FromArgb(255, Color.ForestGreen));
        private readonly Brush _brushOrange = new SolidBrush(Color.FromArgb(255, Color.DarkOrange));
        private readonly Brush _brushRed = new SolidBrush(Color.FromArgb(255, Color.Red));


        //Drawing buffer which will be resized to the control size
        Bitmap buffer = new Bitmap(640, 480);     
        



        public BatteryIndicatorBar()
        {
            this.DoubleBuffered = false;
            InitializeComponent();
        }

        public void DoPaintRemote(PaintEventArgs e)
        {
            var matrix = new System.Drawing.Drawing2D.Matrix();
            matrix.Translate(this.Left, this.Top);
            e.Graphics.Transform = matrix;
            OnPaint(e);
            e.Graphics.ResetTransform();
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            //Calculate Safe home battery
            //
            // Safe flight time with full battery = 65min
            // Mah needed for 1 minute flight = FullCapacity / 65min
            // Estimated speed to home = 10m/s = 600m/min
            // Mah needed for home = distance / 600 * (Mahfor1min)

            float mahToHome = (_distFromHome / (avgSpeed * 60)) * mahPerMinute;

            using (Graphics etemp = Graphics.FromImage(buffer))
            {
                RectangleF bar = new RectangleF(0, 0, this.Width, this.Height);

                etemp.Clear(Color.Transparent);
                etemp.FillRectangle(_brushbar, bar);

                //Validity check
                float battPercent = 0;
                if (_usedMah == 0) battPercent = 1;
                else if (_usedMah < _batteryCapacity) battPercent = 1- (_usedMah/_batteryCapacity);

                if (_batteryCapacity == 0) battPercent = 0;

                RectangleF bartrav = new RectangleF(bar.X, bar.Y, bar.Width * battPercent, bar.Height);




                if (battPercent < .3)
                {
                    etemp.FillRectangle(_brushRed, bartrav);
                }
                else if (mahToHome > (_batteryCapacity - _usedMah))
                {
                    etemp.FillRectangle(_brushOrange, bartrav);
                }
                else
                {
                    etemp.FillRectangle(_brushGreen, bartrav);
                }

                string percent = (battPercent * 100) .ToString("0") + "%";
                var strSize = etemp.MeasureString(percent, this.Font);

                if (battPercent > 0.8f)
                {
                    etemp.DrawString(percent, this.Font, new SolidBrush(this.ForeColor), bartrav.Right-strSize.Width-5,
                        bartrav.Bottom - FontHeight);

                }
                else
                {
                    etemp.DrawString(percent, this.Font, new SolidBrush(this.ForeColor), bartrav.Right + 5,
                        bartrav.Bottom - FontHeight);
                }

                //mahToHome = _batteryCapacity * .5f;
                etemp.FillPie(Brushes.Yellow, (bar.X + bar.Width * (mahToHome / _batteryCapacity)), bar.Top, bar.Height / 2, bar.Height, 0, 360);
                etemp.DrawString("H", new Font(this.Font, FontStyle.Bold), new SolidBrush(Color.Black), (bar.X + bar.Width * (mahToHome / _batteryCapacity)) - 2, bar.Bottom - FontHeight);


                e.Graphics.DrawImageUnscaled(buffer, 0, 0);
            }

        }

        protected override void OnResize(EventArgs e)
        {
            //Resize the buffer to the actual control size
            base.OnResize(e);
            if (this.Width == 0 || this.Height == 0)
                return;
            buffer = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }
    }
}
