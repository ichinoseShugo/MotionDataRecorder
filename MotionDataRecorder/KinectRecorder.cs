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
        public string fileName = "";
        /// <summary> 時間計測用ストップウォッチ </summary>
        public System.Diagnostics.Stopwatch recTimer = new System.Diagnostics.Stopwatch();

        public static int stoptime;

        private MainWindow main;

        public static bool writable = false;

        public KinectRecorder(MainWindow mainWindow)
        {
            stoptime = Midi.stoptime;
            main = mainWindow;
            Console.WriteLine("record length = " + stoptime);
        }

        public void StartRecord()
        {
            var dt = DateTime.Now;
            string now = dt.Year + Digits(dt.Month) + Digits(dt.Day) + Digits(dt.Hour) + Digits(dt.Minute) + Digits(dt.Second);
            fileName = "../../../Data/Kinect/" + now + "_" + main.NameBox.Text + ".csv";
            kinectWriter = new StreamWriter(fileName , true);
            //recTimer.Start();
            Console.WriteLine("start record");
        }

        public void StartRecordExperiment()
        {
            var dt = DateTime.Now;
            string now = dt.Year + Digits(dt.Month) + Digits(dt.Day) + Digits(dt.Hour) + Digits(dt.Minute) + Digits(dt.Second);
            fileName = "../../../Data/Kinect/experiment/" + now + "_" + main.NameBox.Text + "_ex"+ main.GestureBox.SelectedIndex+".csv";
            kinectWriter = new StreamWriter(fileName, true);
            Console.WriteLine("start record");
        }

        /// <summary> 1桁の場合の桁の補正：1時1分→0101 </summary>
        private String Digits(int date)
        {
            if (date / 10 == 0) return "0" + date;
            else return date.ToString();
        }

        public void Write(float[] joints)
        {
            if (Metronomo.stopwatch.ElapsedMilliseconds > stoptime)
            {
                StopRecord();
            }
            if (!writable) return;
            kinectWriter.Write(Metronomo.stopwatch.ElapsedMilliseconds);
            for (int i = 0; i < joints.Length / 3; i++)
            {
                kinectWriter.Write("," + joints[i * 3] + "," + joints[i * 3 + 1] + "," + joints[i * 3 + 2]);
            }
            kinectWriter.WriteLine();
        }

        public void Write(float[] joints,int jtype)
        {
            if (Metronomo.stopwatch.ElapsedMilliseconds > stoptime)
            {
                StopRecord();
            }
            if (!writable) return;
            kinectWriter.Write(Metronomo.stopwatch.ElapsedMilliseconds);
            kinectWriter.Write("," + joints[jtype] + "," + joints[jtype + 1] + "," + joints[jtype + 2]);
            kinectWriter.WriteLine();
        }

        public void StopRecord()
        {
            writable = false;
            if (kinectWriter != null)
            {
                kinectWriter.Close();
                kinectWriter = null;
                Console.WriteLine("stop record");
            }
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

        #region use body
        public void Write(Body body)
        {
            if (Metronomo.stopwatch.ElapsedMilliseconds > stoptime)
            {
                //StopRecord();
                main.RecordButton.IsChecked = false;
            }
            if (!writable) return;
            kinectWriter.Write(Metronomo.stopwatch.ElapsedMilliseconds);
            foreach (var joint in body.Joints)
            {
                var p = joint.Value.Position;
                kinectWriter.Write("," + p.X + "," + p.Y + "," + p.Z);
            }
            kinectWriter.WriteLine();
        }

        public void Write(Body body, JointType jtype)
        {
            if (Metronomo.stopwatch.ElapsedMilliseconds > stoptime)
            {
                main.RecordButton.IsChecked = false;
            }
            if (!writable) return;
            kinectWriter.Write(Metronomo.stopwatch.ElapsedMilliseconds);
            var joint = body.Joints[jtype].Position;
            kinectWriter.Write("," + joint.X + "," + joint.Y + "," + joint.Z);
            kinectWriter.WriteLine();
        }
        #endregion
    }//class
}//namespace
