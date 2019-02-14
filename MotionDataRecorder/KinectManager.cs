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
using System.Threading.Tasks;

namespace MotionDataRecorder
{
    public class KinectManager
    {
        private MainWindow main;

        public KinectSensor kinect;
        public KinectRecorder record;
        public KinectGesture gesture;
        private KinectJointFilter filter;

        private ColorFrameReader colorFrameReader;
        private FrameDescription colorFrameDesc;
        private byte[] colorBuffer;

        private BodyFrameReader bodyFrameReader;
        private Body[] bodies;
        private Body user;
        private float[] joints;

        EventHandler<ColorFrameArrivedEventArgs> handler = null;

        static public System.Diagnostics.Stopwatch kinectTimer = new System.Diagnostics.Stopwatch();

        public KinectManager(MainWindow mainWindow)
        {
            main = mainWindow;
            InitializeKinect();

            kinect.Open();
        }

        private void InitializeKinect()
        {
            kinect = KinectSensor.GetDefault();
            filter = new KinectJointFilter();
            filter.Init();
            if (kinect == null)
            {
                throw new Exception("Kinectを開けません");
            }

            //選択デバイス情報を更新
            Constants.deviceSelect = Constants.SET_KINECT;
            record = new KinectRecorder(main);
            gesture = new KinectGesture(main);

            //抜き差しイベントを設定
            kinect.IsAvailableChanged += Kinect_IsAvailableChanged;

            handler = (s, e) =>
            {
                ColorFrameReader_FrameArrived(s, e);
            };

            //フレームの準備
            PrepareFrame();

            Constants.kinectImageRate = colorFrameDesc.Height / main.ImageColor.Height;
            kinectTimer.Start();
        }

        //int[] time = new int[] { 0, 960, 1920, 2400, 2880, 3840, 4320, 4800, 5280, 5760, 6240, 6720, 7200, 7680, 8160, 8640, 9120, 9600, 10080, 10560, 11040, 11520, 12000, 12480, 12960, 13440, 13920, 14400, 14880, 15360, 15840, 16320, 16800, 17280, 17760, 18240, 18720 };
        int[] time = Midi.mill;

        private void PrepareFrame()
        {
            // カラー画像の情報を作成する(BGRAフォーマット)
            colorFrameDesc = kinect.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // カラーリーダーを開く
            if (colorFrameReader == null)
            {
                colorFrameReader = kinect.ColorFrameSource.OpenReader();
                //colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
                colorFrameReader.FrameArrived += handler;
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
            Console.WriteLine(e.ToString() + "," + e.IsAvailable);
            if (e.IsAvailable) // Kinectが接続された
            {
                Console.WriteLine("available > prepare frame");
                //PrepareFrame();
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
                    filter.UpdateFilter(user);
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
                Processing();
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

        #region Processing

        public static bool GestureIsRecognized = false;
        private void Processing()
        {
            CameraSpacePoint[] p = filter.GetFilteredJoints();
            joints = Norm.ToModel(p);

            if (GestureIsRecognized) RecogGesture(joints);
            RecordJoints(joints);

            DrawSkeleton(p);
        }

        private void RecordJoints(float[] joints)
        {
            record.Write(joints);
        }

        private void RecogGesture(float[] joints)
        {
            /*
             int index = 0;
                    foreach (var j in user.Joints)
                    {
                        joints[index++] = j.Value.Position.X;
                        joints[index++] = j.Value.Position.Y;
                        joints[index++] = j.Value.Position.Y;
                    }
             */
            switch (main.GestureBox.SelectedIndex)
            {
                case 0:
                    gesture.E1_1(joints);
                    break;
                case 1:
                    gesture.E1_2(joints);
                    break;
                case 2:
                    gesture.E2_1_1(joints);
                    break;
                case 3:
                    gesture.E2_1_2(joints);
                    break;
                case 4:
                    gesture.E2_2_1(joints);
                    break;
                case 5:
                    gesture.E2_2_2(joints);
                    break;
                case 6:
                    gesture.E2_3_1(joints);
                    break;
                case 7:
                    gesture.E2_3_2(joints);
                    break;
                default:
                    gesture.Recog(joints);
                    break;
            }
        }
        
        private void DrawSkeleton(CameraSpacePoint[] joints)
        {
            main.CanvasBody.Children.Clear();
            foreach (var joint in joints)
            {
                DrawEllipse(joint, 10, Brushes.White);
            }
        }

        #endregion

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
            //kinect.Open();
        }

        public void StopFrameRead()
        {
            //kinect.Close();
            if (colorFrameReader != null)
            {
                //colorFrameReader.FrameArrived -= ColorFrameReader_FrameArrived;
                //colorFrameReader.FrameArrived -= handler;
                //colorFrameReader.FrameArrived += Skeleton_Mode;
            }
            if (bodyFrameReader != null)
            {
                bodyFrameReader.FrameArrived -= BodyFrameReader_FrameArrived;
            }

            main.CanvasBody.Children.Clear();
            main.ImageColor.Source = main.ImageColor.Source = new BitmapImage(new Uri("/Resources/GreyBack.png", UriKind.Relative));
        }

        private void Skeleton_Mode(object sender, ColorFrameArrivedEventArgs e)
        {
            main.Text5.Text = e.FrameReference.RelativeTime.TotalMilliseconds.ToString();
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
