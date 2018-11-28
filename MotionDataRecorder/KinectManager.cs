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
    public class KinectManager
    {
        private MainWindow main;

        private KinectSensor kinect;

        private ColorFrameReader colorFrameReader;
        private FrameDescription colorFrameDesc;
        private byte[] colorBuffer;
        /// <summary> color frameの高さを実際のimageの高さで割ったサイズの比率 </summary>
        private double imageRate = 1;
        /// <summary> 画像保存用bitmap source </summary>
        //public static BitmapSource bitmapSource = null;
        /// <summary> frame数のカウント </summary>
        //static int frameCount = 0;

        private BodyFrameReader bodyFrameReader;
        private Body[] bodies;

        /// <summary> Kinect座標書き込み用ストリーム( time, x, y, z ) </summary>
        private StreamWriter kinectWriter = null;
        /// <summary> 時間計測用ストップウォッチ </summary>
        private static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        private MidiManager midi = null;
        public KinectManager(MainWindow mainWindow)
        {
            main = mainWindow;
            midi = mainWindow.midiManager;
            InitializeKinect();
            imageRate = colorFrameDesc.Height / main.ImageColor.Height;
        }

        private void InitializeKinect()
        {
            try
            {
                kinect = KinectSensor.GetDefault();
                if (kinect == null)
                {
                    throw new Exception("Kinectを開けません");
                }

                kinect.Open();

                //抜き差しイベントを設定
                kinect.IsAvailableChanged += Kinect_IsAvailableChanged;
                //フレームの準備
                PrepareFrame();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PrepareFrame()
        {
            // カラー画像の情報を作成する(BGRAフォーマット)
            colorFrameDesc = kinect.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // カラーリーダーを開く
            if (colorFrameReader == null)
            {

                colorFrameReader = kinect.ColorFrameSource.OpenReader();
                colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
            }

            // ボディーリーダーを開く
            if (bodyFrameReader == null)
            {
                bodies = new Body[kinect.BodyFrameSource.BodyCount];  // Bodyを入れる配列を作る
                bodyFrameReader = kinect.BodyFrameSource.OpenReader();
                bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;
            }
        }

        private void Kinect_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // Kinectが接続された
            if (e.IsAvailable)
            {
                PrepareFrame();
            }
            // Kinectが外された
            else
            {
                main.ImageColor.Source = new BitmapImage(new Uri("/Resources/GreyBack.png", UriKind.Relative)); // イメージを初期化する
            }
        }

        private void ColorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // カラーフレームを取得する
            using (var colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame == null)
                {
                    return;
                }

                // BGRAデータを取得する
                colorBuffer = new byte[colorFrameDesc.Width * colorFrameDesc.Height * colorFrameDesc.BytesPerPixel];
                colorFrame.CopyConvertedFrameDataToArray(colorBuffer, ColorImageFormat.Bgra);

                // ビットマップにする
                main.ImageColor.Source = BitmapSource.Create(colorFrameDesc.Width, colorFrameDesc.Height, 96, 96,
                    PixelFormats.Bgra32, null, colorBuffer, colorFrameDesc.Width * (int)colorFrameDesc.BytesPerPixel);

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
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            UpdateBodyFrame(e);
            int bodycount = 0;
            foreach (var body in bodies)
            {
                if (body == null)
                {
                    Console.WriteLine(bodies);
                    Console.WriteLine("null body");
                    return;
                }
                if (body.IsTracked) bodycount++;
            }
            if (bodycount > 1)
            {
                Console.WriteLine("Recognize too many people");
                return;
            }
            RecordJoints();
            //DrawBodyFrame();
        }

        private void UpdateBodyFrame(BodyFrameArrivedEventArgs e)
        {
            using (var bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame == null)
                {
                    return;
                }
                // ボディデータを取得する
                bodyFrame.GetAndRefreshBodyData(bodies);
            }
        }

        bool ready = true;
        private void RecordJoints()
        {
            foreach (var body in bodies.Where(b => b.IsTracked))
            {
                //RelativeHandTap(body);
                RelativeBodyTap(body);
            }
        }
      
        private void RelativeHandTap(Body body)
        {
            var HandRight = body.Joints[JointType.HandRight].Position;
            var HandLeft = body.Joints[JointType.HandLeft].Position;
            var ShoulderRight = body.Joints[JointType.ShoulderRight].Position;
            var ShoulderLeft = body.Joints[JointType.ShoulderLeft].Position;

            Point3D LRs = new Point3D(ShoulderRight.X - ShoulderLeft.X, ShoulderRight.Y - ShoulderLeft.Y, ShoulderRight.Z - ShoulderLeft.Z);
            Point3D LRh = new Point3D(HandRight.X - HandLeft.X, HandRight.Y - HandLeft.Y, HandRight.Z - HandLeft.Z);
            double absLRs = Math.Sqrt(Math.Pow(LRs.X, 2) + Math.Pow(LRs.Y, 2) + Math.Pow(LRs.Z, 2));
            double t = (LRs.X * LRh.X + LRs.Y * LRh.Y + LRs.Z * LRh.Z) / absLRs;
            Point3D D = new Point3D(
                HandLeft.X - HandRight.X + t * LRs.X,
                HandLeft.Y - HandRight.Y + t * LRs.Y,
                HandLeft.Z - HandRight.Z + t * LRs.Z);
            double depth = Math.Sqrt(Math.Pow(D.X, 2) + Math.Pow(D.Y, 2) + Math.Pow(D.Z, 2));
            var z = (HandLeft.Z - HandRight.Z) * LRs.X - (HandLeft.X - HandRight.X) * LRs.Z;
            if (z > 0) depth *= -1;
            if (depth < 0)
            {
                if (ready)
                {
                    midi.OnNote(60);
                    ready = false;
                }
            }
            else
            {
                ready = true;
            }
            main.Text1.Text = depth.ToString();
            main.Text2.Text = z.ToString();
            main.CanvasBody.Children.Clear();
            DrawEllipse(HandRight, 10, Brushes.Red);
            DrawEllipse(HandLeft, 10, Brushes.Lavender);
        }

        double maxDist = -1;
        private void RelativeBodyTap(Body body)
        {
            var HandRight = body.Joints[JointType.HandRight].Position;
            var ShoulderRight = body.Joints[JointType.ShoulderRight].Position;
            var distance = Math.Sqrt(
                Math.Pow(HandRight.X - ShoulderRight.X, 2) +
                Math.Pow(HandRight.Y - ShoulderRight.Y, 2) +
                Math.Pow(HandRight.Z - ShoulderRight.Z, 2));
            if (distance > maxDist) maxDist = distance;
            if (distance > maxDist * 0.8)
            {
                if (ready)
                {
                    midi.OnNote(60);
                    ready = false;
                }
            }
            else
            {
                ready = true;
            }
            main.Text1.Text = distance.ToString();
        }

        private Point3D JointToPoint3D(Body body, JointType jointType)
        {
            var p = body.Joints[jointType].Position;
            return new Point3D
            {
                X = p.X,
                Y = p.Y,
                Z = p.Z,
            };
        }

        private void DrawEllipse(CameraSpacePoint position, int R, Brush brush)
        {
            var ellipse = new Ellipse()
            {
                Width = R,
                Height = R,
                Fill = brush,
            };

            var point = kinect.CoordinateMapper.MapCameraPointToColorSpace(position);
            if ((point.X < 0) || (point.Y < 0))
            {
                return;
            }

            Canvas.SetLeft(ellipse, (point.X - (R / 2)) / imageRate);
            Canvas.SetTop(ellipse, (point.Y - (R / 2)) / imageRate);

            main.CanvasBody.Children.Add(ellipse);
        }

        public void StartRecord()
        {
            var dt = DateTime.Now;
            string now = dt.Year + Digits(dt.Month) + Digits(dt.Day) + Digits(dt.Hour) + Digits(dt.Minute) + Digits(dt.Second);
            kinectWriter = new StreamWriter("../../../Data/Kinect/" + now + ".csv", true);
            stopwatch.Start();
            midi.PlayMidi();
            //Console.WriteLine("start record");
        }

        /// <summary> 1桁の場合の桁の補正：1時1分→0101 </summary>
        static public String Digits(int date)
        {
            if (date / 10 == 0) return "0" + date;
            else return date.ToString();
        }

        public void CloseRecord()
        {
            stopwatch.Stop();
            kinectWriter.Close();
            kinectWriter = null;
            Console.WriteLine("stop record");
        }

        public void Close()
        {
            if (colorFrameReader != null)
            {
                colorFrameReader.Dispose();
                colorFrameReader = null;
            }

            if (bodyFrameReader != null)
            {
                bodyFrameReader.Dispose();
                bodyFrameReader = null;
            }

            if (kinect != null)
            {
                kinect.Close();
                kinect = null;
            }
            if (kinectWriter != null)
            {
                kinectWriter.Close();
            }
        }
    }
}
