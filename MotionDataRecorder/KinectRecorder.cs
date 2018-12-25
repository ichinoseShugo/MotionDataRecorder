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
        private static System.Diagnostics.Stopwatch recTimer = new System.Diagnostics.Stopwatch();

        public KinectRecorder()
        {
        }

        public void StartRecord()
        {
            var dt = DateTime.Now;
            string now = dt.Year + Digits(dt.Month) + Digits(dt.Day) + Digits(dt.Hour) + Digits(dt.Minute) + Digits(dt.Second);
            kinectWriter = new StreamWriter("../../../Data/Kinect/" + now + ".csv", true);
            recTimer.Start();
            //Midi.PlayMidi();
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
            if(kinectWriter != null)
            {
                kinectWriter.Write(recTimer.ElapsedMilliseconds);
                foreach (var joint in body.Joints)
                {
                    var p = joint.Value.Position;
                    kinectWriter.Write("," + p.X + "," + p.Y + "," + p.Z);
                }
                kinectWriter.WriteLine();
            }
        }

        public void StopRecord()
        {
            recTimer.Stop();
            kinectWriter.Close();
            kinectWriter = null;
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
