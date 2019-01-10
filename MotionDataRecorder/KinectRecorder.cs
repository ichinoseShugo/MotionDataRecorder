using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Kinect;
using LightBuzz.Vitruvius;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace MotionDataRecorder
{
    public class KinectRecorder
    {
        /// <summary> 画像保存用bitmap source </summary>
        //public static BitmapSource bitmapSource = null;
        /// <summary> frame数のカウント </summary>
        //static int frameCount = 0;

        /// <summary> Kinect座標書き込み用ストリーム( time, x, y, z ) </summary>
        private StreamWriter kinectWriter = null;
        /// <summary> 時間計測用ストップウォッチ </summary>
        public System.Diagnostics.Stopwatch recTimer = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch m = null;

        private int stoptime;

        private MainWindow main;

        public KinectRecorder(MainWindow mainWindow)
        {
            stoptime = Midi.mill[Midi.mill.Length-1] + Midi.resolution;
            main = mainWindow;
            m = main.metronomo.stopwatch;
            Console.WriteLine("record length = " + stoptime);
        }

        public void StartRecord()
        {
            var dt = DateTime.Now;
            string now = dt.Year + Digits(dt.Month) + Digits(dt.Day) + Digits(dt.Hour) + Digits(dt.Minute) + Digits(dt.Second);
            string version = "";
            if (main.BodyCheck.IsChecked == true) version = "full";
            if (main.HandCheck.IsChecked == true) version = "hand";
            kinectWriter = new StreamWriter("../../../Data/Kinect/" + now + "_" + main.NameBox.Text + "_" + version + ".csv", true);
            recTimer.Start();
            Console.WriteLine("start record");
        }

        /// <summary> 1桁の場合の桁の補正：1時1分→0101 </summary>
        private String Digits(int date)
        {
            if (date / 10 == 0) return "0" + date;
            else return date.ToString();
        }

        public void Write(Body body)
        {
            if (recTimer.ElapsedMilliseconds > stoptime)
            {
                StopRecord();
            }
            if (kinectWriter == null) return;
            kinectWriter.Write(recTimer.ElapsedMilliseconds);
            foreach (var joint in body.Joints)
            {
                var p = joint.Value.Position;
                kinectWriter.Write("," + p.X + "," + p.Y + "," + p.Z);
            }
            kinectWriter.WriteLine();
        }

        public void Write(Body body, JointType jtype)
        {
            if (m.ElapsedMilliseconds > stoptime)
            {
                //StopRecord();
                main.RecordButton.IsChecked = false;
            }
            if (kinectWriter == null) return;
            //kinectWriter.Write(recTimer.ElapsedMilliseconds);
            kinectWriter.Write(m.ElapsedMilliseconds);
            var joint = body.Joints[jtype].Position;
            kinectWriter.Write("," + joint.X + "," + joint.Y + "," + joint.Z);
            kinectWriter.WriteLine();
        }

        public void StopRecord()
        {
            //recTimer.Stop();
            m.Stop();
            if (kinectWriter != null)
            {
                kinectWriter.Close();
                kinectWriter = null;
            }
            Console.WriteLine("stop record");
        }


        public void Close()
        {
            if (kinectWriter != null)
            {
                kinectWriter.Close();
                kinectWriter = null;
            }

            if(recTimer != null)
            {
                recTimer.Stop();
                recTimer = null;
            }
        }
    }//class
}//namespace
