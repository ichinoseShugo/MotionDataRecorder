using System;
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
        MainWindow main;

        KinectSensor kinect;

        ColorFrameReader colorFrameReader;
        FrameDescription colorFrameDesc;
        byte[] colorBuffer;

        BodyFrameReader bodyFrameReader;
        Body[] bodies;

        public KinectManager(MainWindow mainWindow)
        {
            main = mainWindow;
            InitializeKinect();
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
                kinect.IsAvailableChanged += kinect_IsAvailableChanged;
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
                colorFrameReader.FrameArrived += colorFrameReader_FrameArrived;
            }

            // ボディーリーダーを開く
            if (bodyFrameReader == null)
            {
                bodies = new Body[kinect.BodyFrameSource.BodyCount];  // Bodyを入れる配列を作る
                bodyFrameReader = kinect.BodyFrameSource.OpenReader();
                bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;
            }
        }

        private void kinect_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
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

        private void colorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
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

        private void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
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

        private void RecordJoints()
        {
            int bodycount = 0;
            foreach(var body in bodies)
            {
                if (body == null)
                {
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
            foreach (var body in bodies)
            {
                foreach (var joint in body.Joints)
                {

                }
            }
        }

        private void DrawBodyFrame()
        {
            main.CanvasBody.Children.Clear();

            var rHand = bodies[0].Joints[JointType.HandRight];
            if (rHand.TrackingState == TrackingState.Tracked)
            {
                DrawEllipse(rHand, 10, Brushes.Blue);
            }
        }

        private void DrawEllipse(Joint joint, int R, Brush brush)
        {
            var ellipse = new Ellipse()
            {
                Width = R,
                Height = R,
                Fill = brush,
            };
            var d = joint.Position;
            // カメラ座標系をDepth座標系に変換する
            var point = kinect.CoordinateMapper.MapCameraPointToColorSpace(joint.Position);
            if ((point.X < 0) || (point.Y < 0))
            {
                return;
            }

            Canvas.SetLeft(ellipse, (point.X - (R / 2)) / 2);
            Canvas.SetTop(ellipse, (point.Y - (R / 2)) / 2);

            main.CanvasBody.Children.Add(ellipse);
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
        }
    }
}
