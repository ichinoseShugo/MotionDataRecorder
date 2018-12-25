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

        public KinectSensor kinect;
        public KinectRecorder record;
        private KinectGesture gesture;

        private ColorFrameReader colorFrameReader;
        private FrameDescription colorFrameDesc;
        private byte[] colorBuffer;

        private BodyFrameReader bodyFrameReader;
        private Body[] bodies;
        private Body user;

        public KinectManager(MainWindow mainWindow)
        {
            main = mainWindow;
            InitializeKinect();
            Constants.kinectImageRate = colorFrameDesc.Height / main.ImageColor.Height;
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

                //選択デバイス情報を更新
                Constants.deviceSelect = Constants.SET_KINECT;
                record = new KinectRecorder();
                gesture = new KinectGesture();

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

        /// <summary> kinectの接続状態が変化した時のイベント </summary>
        private void Kinect_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            if (e.IsAvailable) // Kinectが接続された
            {
                PrepareFrame();
            }
            else // Kinectが外された
            {
                //main.ImageColor.Source = new BitmapImage(new Uri("/Resources/GreyBack.png", UriKind.Relative)); // イメージを初期化する
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
            //なぜかbodiesやbodyがnullのまま処理に入ることがあるため、「bodies(body)がnullでない」かつ「配列に要素が1つでない」時だけ処理
            int bodycount = 0;
            foreach (var body in bodies)
            {
                if (body == null)
                {
                    Console.WriteLine(bodies);
                    Console.WriteLine("null body");
                    return;
                }
                if (body.IsTracked)
                {
                    bodycount++;
                    user = body;
                }
            }
            if (bodycount > 1)
            {
                Console.WriteLine("Recognize too many people");
                return;
            }
            //処理を記述
            if(user != null)
            {
                //RecogGesture();
                RecordJoints();

                var right = user.Joints[JointType.HandRight].Position;
                var left = user.Joints[JointType.HandLeft].Position;

                main.CanvasBody.Children.Clear();
                DrawEllipse(right, 10, Brushes.Red);
                DrawEllipse(left, 10, Brushes.Blue);
            }
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
            record.Write(user);
        }

        private void RecogGesture()
        {
            gesture.RelativeHandTap(user);
        }

        /// <summary> 画面に円を表示 </summary>
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

            Canvas.SetLeft(ellipse, (point.X - (R / 2)) / Constants.kinectImageRate);
            Canvas.SetTop(ellipse, (point.Y - (R / 2)) / Constants.kinectImageRate);

            main.CanvasBody.Children.Add(ellipse);
        }

        public void StartFrameRead()
        {
            PrepareFrame();
        }

        public void StopFrameRead()
        {
            //kinect.Close();

            if (colorFrameReader != null)
            {
                colorFrameReader.FrameArrived -= ColorFrameReader_FrameArrived;
            }
            if (bodyFrameReader != null)
            {
                bodyFrameReader.FrameArrived -= BodyFrameReader_FrameArrived;
            }

            main.CanvasBody.Children.Clear();
            main.ImageColor.Source = null;
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

            if (record != null)
            {
                record.Close();
            }

            if (gesture != null)
            {
                gesture.Close();
            }
        }
    }
}
