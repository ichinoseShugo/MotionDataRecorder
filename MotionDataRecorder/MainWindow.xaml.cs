using System;
using System.Collections.Generic;
using System.IO;
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

using Microsoft.Kinect;


namespace MotionDataRecorder
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectManager kinectManager = null;
        RealSenseManager realSenseManager = null;

        KinectReplay kinectReplay = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Midi.InitMidi();
            kinectManager = new KinectManager(this);
            //realSenseManager = new RealSenseManager(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (kinectManager != null)
            {
                kinectManager.Close();
            }
            if (realSenseManager != null)
            {
                realSenseManager.Close();
            }
        }

        private void KinectButton_Click(object sender, RoutedEventArgs e)
        {
            if (realSenseManager != null)
            {
                Console.WriteLine("realsense is already opened, please stop realsense");
                return;
            }
            if (kinectManager == null)
            {
                Console.WriteLine("try to open kinect");
                kinectManager = new KinectManager(this);
            }
            else
            {
                kinectManager.StartFrameRead();
            }
        }

        private void RealSenseButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("realsense button click");
            if (kinectManager == null && realSenseManager == null)
            {
                Console.WriteLine("try to open realsense");
                realSenseManager = new RealSenseManager(this);
            }
        }

        #region record
        private void Record_Click(object sender, RoutedEventArgs e)
        {
            switch (Constants.deviceSelect)
            {
                case 1:
                    if (kinectManager != null)
                    {
                        kinectManager.record.StartRecord();
                    }
                    break;
                case 2:
                    if (realSenseManager != null)
                    {

                    }
                    break;
                default:
                    ;
                    break;
            }
        }

        private void Record_Unchecked(object sender, RoutedEventArgs e)
        {
            switch (Constants.deviceSelect)
            {
                case 1:
                    if (kinectManager != null)
                    {
                        kinectManager.record.StopRecord();
                    }
                    break;
                case 2:
                    if (realSenseManager != null)
                    {

                    }
                    break;
                default:
                    ;
                    break;
            }
        }
        #endregion

        #region midi

        int[] time = new int[] { 480, 1440, 2400, 2880, 3360, 4320, 4800, 5280, 5760, 6240, 6720, 7200, 7680, 8160, 8640, 9120, 9600, 10080, 10560, 11040, 11520, 12000, 12480, 12960, 13440, 13920, 14400, 14880, 15360, 15840, 16320, 16800, 17280, 17760, 18240, 18720, 19200 };
        private static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        public void Midi_Click(object sender, RoutedEventArgs e)
        {
            stopwatch.Start();
            StartEnsemble();
            Console.WriteLine("click");
        }

        private async void StartMidi()
        {
            await Task.Run(() => 
            {
                Midi.OnNote(60);
            });
        }

        private async void StartEnsemble()
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < time.Length; i++)
                {
                    int waitTime = (int)(time[i] - stopwatch.ElapsedMilliseconds);
                    System.Threading.Thread.Sleep(waitTime);
                    Midi.OnNote(11, 80, 240);
                }
            });
        }

        #endregion

        #region replay
        private void ReplayButton_Click(object sender, RoutedEventArgs e)
        {
            if (kinectManager != null)
            {
                kinectReplay = new KinectReplay(this, kinectManager.kinect);
                if (kinectReplay.index >= 0)
                {
                    kinectManager.StopFrameRead();
                    kinectReplay.StartReplay();
                    StopPlayButton.IsEnabled = true;
                }
            }
        }

        private void StopPlayButton_Click(object sender, RoutedEventArgs e)
        {
            StopPlayButton.IsEnabled = false;
            if (kinectReplay != null)
            {
                kinectReplay.StopReplay();
            }
        }
        #endregion

    }//class
}//namespace
