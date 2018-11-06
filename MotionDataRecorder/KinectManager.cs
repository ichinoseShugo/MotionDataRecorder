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

        private BodyFrameReader bodyFrameReader;
        private Body[] bodies;

        /// <summary> Kinect座標書き込み用ストリーム( time, x, y, z ) </summary>
        private StreamWriter kinectWriter = null;
        /// <summary> 時間計測用ストップウォッチ </summary>
        private static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        public KinectManager(MainWindow mainWindow)
        {
            main = mainWindow;
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
            }
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            UpdateBodyFrame(e);
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

        CameraSpacePoint op;
        bool firstrecog = true;
        /// <summary> 5000mG = 49.03325 (m/s^2) </summary>
        double border = 0;
        bool over = false;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        private void RecordJoints()
        {
            int bodycount = 0;
            foreach(var body in bodies)
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

            main.CanvasBody.Children.Clear();
            foreach (var body in bodies.Where(b => b.IsTracked))
            {
                if (firstrecog)
                {
                    op = body.Joints[JointType.HandRight].Position;
                    firstrecog = false;
                    return;
                }
                //var o = body.JointOrientations[JointType.HandRight].Orientation;
                //main.Text1.Text = o.W.ToString();
                //main.Text2.Text = o.X.ToString();
                //main.Text3.Text = o.Y.ToString();
                //main.Text4.Text = o.Z.ToString();
                //var s = body.Joints[JointType.HandRight];
                var p = body.Joints[JointType.HandRight].Position;
                //double m = Math.Sqrt( Math.Pow(p.X - op.X, 2) + Math.Pow(p.Y - op.Y, 2) + Math.Pow(p.Z - op.Z, 2) );
                double m = p.Z - op.Z;
                double ms = m / 0.035;
                double mss = ms / 0.035;
                //main.Text1.Text = ms.ToString();
                op = p;
                if (kinectWriter != null)
                {
                    //var position = body.Joints[JointType.HandRight].Position;
                    var mill = stopwatch.ElapsedMilliseconds;
                    //kinectWriter.WriteLine(mill + "," + position.X + "," + position.Y + "," + position.Z);
                    kinectWriter.WriteLine(mill + "," + m + "," + ms + "," + mss);
                }
                foreach (var joint in body.Joints)
                {
                    DrawEllipse(joint.Value.Position, 10, Brushes.Blue);
                }
            }
        }

        private void DrawBodyFrame()
        {
            main.CanvasBody.Children.Clear();

            var rHand = bodies[0].Joints[JointType.HandRight];
            if (rHand.TrackingState == TrackingState.Tracked)
            {
                //DrawEllipse(rHand, 10, Brushes.Blue);
            }
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
            Console.WriteLine("start record");
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
