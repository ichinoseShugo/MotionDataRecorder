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
    class KinectRecorder
    {
        /// <summary> 画像保存用bitmap source </summary>
        //public static BitmapSource bitmapSource = null;
        /// <summary> frame数のカウント </summary>
        //static int frameCount = 0;

        /// <summary> Kinect座標書き込み用ストリーム( time, x, y, z ) </summary>
        private StreamWriter kinectWriter = null;
        /// <summary> 時間計測用ストップウォッチ </summary>
        private static System.Diagnostics.Stopwatch recordTimer = new System.Diagnostics.Stopwatch();

        public KinectRecorder()
        {

        }

        public void StartRecord()
        {
            var dt = DateTime.Now;
            string now = dt.Year + Digits(dt.Month) + Digits(dt.Day) + Digits(dt.Hour) + Digits(dt.Minute) + Digits(dt.Second);
            kinectWriter = new StreamWriter("../../../Data/Kinect/" + now + ".csv", true);
            recordTimer.Start();
            Midi.PlayMidi();
            Console.WriteLine("start record");
        }

        /// <summary> 1桁の場合の桁の補正：1時1分→0101 </summary>
        private String Digits(int date)
        {
            if (date / 10 == 0) return "0" + date;
            else return date.ToString();
        }

        public void Write()
        {
            if(kinectWriter != null)
            {
                kinectWriter.WriteLine();
            }
        }

        public void StopRecord()
        {
            recordTimer.Stop();
            kinectWriter.Close();
            kinectWriter = null;
            Console.WriteLine("stop record");
        }

        public void RecordImage()
        {

            //bitmapSource = BitmapSource.Create(colorFrameDesc.Width, colorFrameDesc.Height, 96, 96,
            //    PixelFormats.Bgra32, null, colorBuffer, colorFrameDesc.Width * (int)colorFrameDesc.BytesPerPixel);

            //main.ImageColor.Source = bitmapSource;
            //ImageColor.SetCurrentValue(Image.SourceProperty, bitmapSource);
            /*
            if (RecordPoints.IsChecked == true && frameCount % 3 == 0)
            {
                using (Stream stream =
                new FileStream(pathSaveFolder + "image/" + StopWatch.ElapsedMilliseconds + ".jpg", FileMode.Create))
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(stream);
                    stream.Close();
                }
            }
            frameCount++;
            */
        }

        public void Close()
        {
            if (kinectWriter != null)
            {
                kinectWriter.Close();
                kinectWriter = null;
            }

            if(recordTimer != null)
            {
                recordTimer.Stop();
                recordTimer = null;
            }
        }
    }//class
}//namespace
