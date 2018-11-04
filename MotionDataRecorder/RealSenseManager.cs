using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace MotionDataRecorder
{
    public class RealSenseManager
    {
        MainWindow main;

        PXCMSenseManager senseManager;
        PXCMProjection projection;
        PXCMCapture.Device device;
        PXCMHandModule handAnalyzer;
        PXCMHandData handData;
        PXCMHandConfiguration config;

        public RealSenseManager(MainWindow mainWindow)
        {
            main = mainWindow;
            InitializeRealSense();
        }

        private void InitializeRealSense()
        {
            try
            {
                //SenseManagerを生成
                senseManager = PXCMSenseManager.CreateInstance();

                //カラーストリームの有効
                var sts = senseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR,
                    Constants.COLOR_WIDTH, Constants.COLOR_HEIGHT, Constants.COLOR_FPS);
                if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    throw new Exception("Colorストリームの有効化に失敗しました");
                }

                // Depthストリームを有効にする
                sts = senseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_DEPTH,
                    Constants.DEPTH_WIDTH, Constants.DEPTH_HEIGHT, Constants.DEPTH_FPS);
                if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    throw new Exception("Depthストリームの有効化に失敗しました");
                }

                // 手の検出を有効にする
                sts = senseManager.EnableHand();
                if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    throw new Exception("手の検出の有効化に失敗しました");
                }

                //パイプラインを初期化する
                //(インスタンスはInit()が正常終了した後作成されるので，機能に対する各種設定はInit()呼び出し後となる)
                sts = senseManager.Init();
                if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) throw new Exception("パイプラインの初期化に失敗しました");

                //ミラー表示にする
                senseManager.QueryCaptureManager().QueryDevice().SetMirrorMode(PXCMCapture.Device.MirrorMode.MIRROR_MODE_HORIZONTAL);

                //デバイスを取得する
                device = senseManager.captureManager.device;

                //座標変換オブジェクトを作成
                projection = device.CreateProjection();

                // 手の検出の初期化
                InitializeHandTracking();

                CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary> 手の検出の初期化 </summary>
        private void InitializeHandTracking()
        {
            // 手の検出器を取得する
            handAnalyzer = senseManager.QueryHand();

            // 手のデータを作成する
            handData = handAnalyzer.CreateOutput();

            // RealSense カメラであれば、プロパティを設定する
            var device = senseManager.QueryCaptureManager().QueryDevice();
            PXCMCapture.DeviceInfo dinfo;
            device.QueryDeviceInfo(out dinfo);
            if (dinfo.model == PXCMCapture.DeviceModel.DEVICE_MODEL_IVCAM)
            {
                device.SetDepthConfidenceThreshold(1);
                //device.SetMirrorMode( PXCMCapture.Device.MirrorMode.MIRROR_MODE_DISABLED );
                device.SetIVCAMFilterOption(6);
            }

            // 手の検出の設定
            config = handAnalyzer.CreateActiveConfiguration();
            config.EnableJointSpeed(PXCMHandData.JointType.JOINT_MIDDLE_TIP, PXCMHandData.JointSpeedType.JOINT_SPEED_AVERAGE, 100);
            //config.EnableGesture("v_sign");
            //config.EnableGesture("thumb_up");
            //config.EnableGesture("thumb_down");
            //config.EnableGesture("tap");
            //config.EnableGesture("fist");
            config.ApplyChanges();
            config.Update();
        }
        
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            //フレームを取得する
            //AcquireFrame()の引数はすべての機能の更新が終るまで待つかどうかを指定
            //ColorやDepthによって更新間隔が異なるので設定によって値を変更
            var ret = senseManager.AcquireFrame(true);
            if (ret < pxcmStatus.PXCM_STATUS_NO_ERROR) return;

            //フレームデータを取得する
            PXCMCapture.Sample sample = senseManager.QuerySample();
            if (sample != null)
            {
                //カラー画像の表示
                UpdateColorImage(sample.color);
            }

            //手のデータを更新
            UpdateHandFrame();

            //フレームを解放する
            senseManager.ReleaseFrame();
        }

        /// <summary> カラーイメージが更新された時の処理 </summary>
        private void UpdateColorImage(PXCMImage colorFrame)
        {
            if (colorFrame == null) return;
            //データの取得
            PXCMImage.ImageData data;

            //アクセス権の取得
            pxcmStatus ret = colorFrame.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB32, out data);
            if (ret < pxcmStatus.PXCM_STATUS_NO_ERROR) throw new Exception("カラー画像の取得に失敗");

            //ビットマップに変換する
            //画像の幅と高さ，フォーマットを取得
            var info = colorFrame.QueryInfo();

            //1ライン当たりのバイト数を取得し(pitches[0]) 高さをかける　(1pxel 3byte)
            var length = data.pitches[0] * info.height;

            //画素の色データの取得
            //ToByteArrayでは色データのバイト列を取得する．
            var buffer = data.ToByteArray(0, length);
            //バイト列をビットマップに変換
            main.ImageColor.Source = BitmapSource.Create(info.width, info.height, 96, 96, PixelFormats.Bgr32, null, buffer, data.pitches[0]);

            //データを解放する
            colorFrame.ReleaseAccess(data);
        }

        int[] side2id = new int[2];
        /// <summary> 手のデータを更新する </summary>
        private void UpdateHandFrame()
        {
            // 手のデータを更新する
            handData.Update();

            // データを初期化する
            main.CanvasBody.Children.Clear();

            // 検出した手の数を取得する
            var numOfHands = handData.QueryNumberOfHands();
            for (int i = 0; i < numOfHands; i++)
            {
                // 手を取得する
                PXCMHandData.IHand hand;
                var sts = handData.QueryHandData(
                    PXCMHandData.AccessOrderType.ACCESS_ORDER_BY_ID, i, out hand);
                if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) continue;

                //GetFingerData(hand, PXCMHandData.JointType.JOINT_MIDDLE_TIP);
            }
        }

        /// <summary> 指のデータを取得する </summary>
        private void GetFingerData(PXCMHandData.IHand hand, PXCMHandData.JointType jointType)
        {
            PXCMHandData.JointData jointData; //image 原点:左上 単位:ピクセル, world 原点: 単位:m カメラ側から見て右に+x,上に+y,奥に+z
            var sts = hand.QueryTrackedJoint(jointType, out jointData);
            if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) return;

            // Depth座標系をカラー座標系に変換する
            var cameraPoint = new PXCMPoint3DF32[1];
            var depthPoint = new PXCMPoint3DF32[1];
            var colorPoint = new PXCMPointF32[1];
            
            depthPoint[0].x = jointData.positionImage.x; //depth座標系(pixels)
            depthPoint[0].y = jointData.positionImage.y; //depth座標系(pixels)
            depthPoint[0].z = jointData.positionWorld.z * 1000; //depth座標(pixels)
            /*
            cameraPoint[0].x = jointData.positionWorld.z;
            cameraPoint[0].y = jointData.positionWorld.z;
            cameraPoint[0].z = jointData.positionWorld.z;
            */

            projection.MapDepthToColor(depthPoint, colorPoint);
            //projection.ProjectCameraToColor(cameraPoint, colorPoint);

            /*
            main.Text1.Text = jointData.positionImage.x.ToString();
            main.Text2.Text = jointData.positionImage.y.ToString();
            main.Text3.Text = jointData.positionImage.z.ToString();
            main.Text4.Text = jointData.positionWorld.x.ToString();
            main.Text5.Text = jointData.positionWorld.y.ToString();
            main.Text6.Text = jointData.positionWorld.z.ToString();
            main.Text7.Text = colorPoint[0].x.ToString();
            main.Text8.Text = colorPoint[0].y.ToString();
            main.Text9.Text = (jointData.positionImage.x - colorPoint[0].x).ToString();
            */

            AddEllipse(new Point(colorPoint[0].x, colorPoint[0].y), 10, Brushes.Red, 1);
        }

        /// <summary> 円を表示する </summary>
        private void AddEllipse(Point point, int radius, Brush color, int thickness)
        {
            var ellipse = new Ellipse()
            {
                Width = radius,
                Height = radius,
            };
            if (thickness <= 0)
            {
                ellipse.Fill = color;
            }
            else
            {
                ellipse.Stroke = Brushes.Black;
                ellipse.StrokeThickness = thickness;
                ellipse.Fill = color;
            }
            Canvas.SetLeft(ellipse, point.X);
            Canvas.SetTop(ellipse, point.Y);
            main.CanvasBody.Children.Add(ellipse);
        }

        public void Close()
        {
            if (senseManager != null)
            {
                senseManager.Dispose();
                senseManager = null;
            }
            if (projection != null)
            {
                projection.Dispose();
                projection = null;
            }
            if (handData != null)
            {
                handData.Dispose();
                handData = null;
            }

            if (handAnalyzer != null)
            {
                handAnalyzer.Dispose();
                handAnalyzer = null;
            }
            if (config != null)
            {
                config.Dispose();
            }
        }
    }
}
