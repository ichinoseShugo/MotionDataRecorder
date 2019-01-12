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

        public Metronomo metronomo;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Midi.InitMidi();
            metronomo = new Metronomo(this);
            //kinectManager = new KinectManager(this);
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
            if(kinectReplay != null)
            {
                kinectReplay.Close();
            }
        }

        private void KinectButton_Click(object sender, RoutedEventArgs e)
        {
            if(kinectReplay != null)
            {
                kinectReplay.Close();
            }
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
                        metronomo.Start();
                        Console.WriteLine(metronomo.stopwatch.ElapsedMilliseconds - kinectManager.record.recTimer.ElapsedMilliseconds);
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
        
        public void Midi_Click(object sender, RoutedEventArgs e)
        {
            metronomo.Start();
        }

        #endregion

        #region replay
        private void ReplayButton_Click(object sender, RoutedEventArgs e)
        {
            kinectReplay = new KinectReplay(this);
            if (kinectManager != null)
            {
                kinectManager.StopFrameRead();
            }
            kinectReplay.StartReplay();
            StopPlayButton.IsEnabled = true;
        }

        private void StopPlayButton_Checked(object sender, RoutedEventArgs e)
        {
            if (kinectReplay != null)
            {
                kinectReplay.StopReplay();
            }
            StopPlayButton.Content = "▶";
        }

        private void StopPlayButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (kinectReplay != null)
            {
                kinectReplay.StartReplay();
            }
            StopPlayButton.Content = "||";
        }
        #endregion

        private void SkeletonButton_Checked(object sender, RoutedEventArgs e)
        {
            if(kinectManager != null)
            {
                kinectManager.StopFrameRead();
            }
        }

        private void SkeletonButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (kinectManager != null)
            {
                kinectManager.StartFrameRead();
            }
        }

        private void LearnButton_Click(object sender, RoutedEventArgs e)
        {
            string filename = "../../../Data/Kinect/12/20190109184607_taka2_full.csv";
            MotionLearner m = new MotionLearner(this, filename);
        }
    }//class
}//namespace
