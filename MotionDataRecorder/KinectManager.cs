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
                frameTimer.Start();
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

        private void RecordJoints()
        {
            foreach (var body in bodies.Where(b => b.IsTracked))
            {
                Tap(body);
                //var hand = body.Joints[JointType.HandRight].Position;
                var p = body.Joints[JointType.HandRight].Position;
                //s = stopwatch.ElapsedMilliseconds;
                //var elbow = body.Joints[JointType.ElbowRight].Position;
                //var shoulder = body.Joints[JointType.ShoulderRight].Position;
                //var arm1 = Math.Sqrt(Math.Pow(hand.X - elbow.X, 2) + Math.Pow(hand.Y - elbow.Y, 2) + Math.Pow(hand.Z - elbow.Z, 2));
                //var arm2 = Math.Sqrt(Math.Pow(elbow.X - shoulder.X, 2) + Math.Pow(elbow.Y - shoulder.Y, 2) + Math.Pow(elbow.Z - shoulder.Z, 2));
                //var arm = arm1 + arm2;
                //var arm = Math.Sqrt(Math.Pow(hand.X - shoulder.X, 2) + Math.Pow(hand.Y - shoulder.Y, 2) + Math.Pow(hand.Z - shoulder.Z, 2));
                //main.Text1.Text = arm.ToString();
                if (kinectWriter != null)
                {
                    //kinectWriter.WriteLine(s + "," + comAbs);
                    //s = stopwatch.ElapsedMilliseconds;
                    //kinectWriter.WriteLine(s + "," + p.X + "," + p.Y + "," + p.Z);
                    //kinectWriter.WriteLine(s + "," + m + "," + v + "," + a);
                }

                main.CanvasBody.Children.Clear();
                DrawEllipse(p, 10, Brushes.Blue);
                //DrawEllipse(shoulder, 10, Brushes.Blue);
            }
        }

        bool firstrecog = true;
        CameraSpacePoint o;
        double v0 = 0;
        double a0 = 0;
        double t0 = 0;
        double angle0 = 0;
        System.Diagnostics.Stopwatch frameTimer = new System.Diagnostics.Stopwatch();

        double border = 20;
        //double border = 49.03325;
        bool over = false;
        System.Diagnostics.Stopwatch transitTimer = new System.Diagnostics.Stopwatch();
        private void Tap(Body body)
        {
            if (firstrecog)
            {
                o = body.Joints[JointType.HandRight].Position;
                firstrecog = false;
                t0 = frameTimer.ElapsedMilliseconds;
                return;
            }
            var p = body.Joints[JointType.HandRight].Position;
            var wrist = body.Joints[JointType.WristLeft].Position;
            var elbow = body.Joints[JointType.ElbowRight].Position;
            double angle = wrist.Angle(elbow, p);
            var t = frameTimer.ElapsedMilliseconds;
            double mill = (t - t0)/1000;
            double v = Math.Sqrt(Math.Pow(p.X - o.X, 2) + Math.Pow(p.Y - o.Y, 2) + Math.Pow(p.Z - o.Z, 2))/mill;
            double a = (v - v0) / mill;
            double dangle = angle - angle0;
            Console.WriteLine(a);
            if (a > border)
            {
                if (over)
                {
                }
                else
                {
                    transitTimer.Start();
                    over = true;
                }
            }
            else
            {
                if (over)
                {
                    var transit = transitTimer.ElapsedMilliseconds;
                    if(30 <= transit && transit <= 60)
                    {
                        midi.OnNote(60);
                    }
                    transitTimer.Reset();
                    over = false;
                }
                else
                {

                }
            }

            /*
            if (a < 0 && (a0 - a) > 40)
            {
                midi.OnNote(60);
            }
            */
            if (kinectWriter != null)
            {
                var s = stopwatch.ElapsedMilliseconds;
                kinectWriter.WriteLine(s + "," + t + "," + v + "," + a + "," + angle + "," + dangle);
            }
            o = p;
            v0 = v;
            a0 = a;
            t0 = t;
            angle0 = angle;
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
