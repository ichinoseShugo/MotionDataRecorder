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
            if(kinectManager != null)
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
            Console.WriteLine("kinect button click");
            if (kinectManager == null && realSenseManager == null)
            {
                Console.WriteLine("try to open kinect");
                kinectManager = new KinectManager(this);
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

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            switch (Constants.deviceSelect)
            {
                case 1:
                    if (kinectManager != null)
                    {

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
        
        private void Midi_Click(object sender, RoutedEventArgs e)
        {
            Midi.OnNote(60);
            //midiManager.PlayMidi();
            //midiManager.OnNote(60);
            //Console.Beep();
        }
    }
}
