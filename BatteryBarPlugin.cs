using System;
using MissionPlanner.Utilities;
using System.IO;
using System.Windows.Forms;
using GMap.NET.WindowsForms;
using GMap.NET;
using System.Drawing;
using System.Drawing.Drawing2D;
using MissionPlanner.Maps;
using System.Collections.Generic;
using System.Net;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using MissionPlanner.Controls;

namespace MissionPlanner.Maps
{

    public class GMapMarkerWeather : GMapMarker
    {
        Bitmap weather;
        private RectLatLng rect;

        public GMapMarkerWeather(String imageName, RectLatLng rect, PointLatLng currentloc)
        : base(currentloc)
        {
            this.rect = rect;
            weather?.Dispose();
            weather = new Bitmap(imageName);
            weather.MakeTransparent();
        }


        public override void OnRender(IGraphics g)
        {
            base.OnRender(g);

            if (weather != null)
            {

                var tlll = Overlay.Control.FromLatLngToLocal(rect.LocationTopLeft);
                var brll = Overlay.Control.FromLatLngToLocal(rect.LocationRightBottom);

                var old = g.Transform;
                g.ResetTransform();
                g.CompositingMode = CompositingMode.SourceOver;
                g.DrawImage(weather, tlll.X, tlll.Y, brll.X - tlll.X, brll.Y - tlll.Y);
                g.Transform = old;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            weather.Dispose();

        }


    }

}



namespace MissionPlanner.BatteryBarPlugin
{


    public class BatteryBarPlugin : MissionPlanner.Plugin.Plugin
    {


        BatteryIndicatorBar bBar;
        float mahPerMin;
        float avgSpeed;

        float battCap;



        public override string Name
        {
            get { return "BatteryBar"; }
        }

        public override string Version
        {
            get { return "0.1"; }
        }

        public override string Author
        {
            get { return "Andras Schaffer"; }
        }

        //[DebuggerHidden]
        public override bool Init()
        {
            loopratehz = 1f;

            ////weather

            MainV2.instance.BeginInvoke((MethodInvoker)(() =>
            {

               var FDRightSide = Host.MainForm.FlightData.Controls.Find("splitContainer1", true).FirstOrDefault() as SplitContainer;
                bBar = new BatteryIndicatorBar();
                bBar.Name = "batteryBar1";
                bBar.Location = new Point(66, 24);
                bBar.Size = new Size(Host.FDGMapControl.Width - 66 -6, 15);
                bBar.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
                bBar.BringToFront();

                FDRightSide.Panel2.Controls.Add(bBar);
                FDRightSide.Panel2.Controls.SetChildIndex(bBar, 1);

            }));

            //Get parameters
            mahPerMin = Host.config.GetFloat("BBAR_MAHPERMIN", 700);
            Host.config["BBAR_MAHPERMIN"] = mahPerMin.ToString();

            avgSpeed = Host.config.GetFloat("BBAR_AVGSPEED", 10);
            Host.config["BBAR_AVGSPEED"] = avgSpeed.ToString();

            return true;
        }


        public override bool Loaded()
        {
            return true;
        }

        public override bool Loop()
        {
            if (Host.cs.connected)
            {

                if (battCap == 0)
                {
                    if (MainV2.comPort.MAV.param.ContainsKey("BATT_CAPACITY"))
                    {
                        battCap = (float)MainV2.comPort.MAV.param["BATT_CAPACITY"].Value;
                        bBar.BatteryCapacity = battCap;
                    }
                }
                //MahPerMinute flight 
                bBar.mahPerMinute = mahPerMin;
                //avgSpeed when coming home (m/s)
                bBar.avgSpeed = avgSpeed;
                bBar.UsedMah = (float)Host.cs.battery_usedmah;
                bBar.DistFromHome = Host.cs.DistToHome;
            }

            bBar?.Invalidate();
            return true;
        }

        public override bool Exit()
        {

            return true;
        }


        //public void setTimeLabel(string s)
        //{
        //    MainV2.instance.BeginInvoke((MethodInvoker)(() =>
        //    {

        //        weatherImageDate.Text = s;
        //    }));
        //}




    }

}