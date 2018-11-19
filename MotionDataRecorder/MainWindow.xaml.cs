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
            if(kinectManager == null && realSenseManager == null)
            {
                Console.WriteLine("kinect open");
                kinectManager = new KinectManager(this);
            }
        }

        private void RealSenseButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("realsense button click");
            if (kinectManager == null && realSenseManager == null)
            {
                Console.WriteLine("realsense open");
                realSenseManager = new RealSenseManager(this);
            }
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            kinectManager.StartRecord();
        }

        private void Record_Unchecked(object sender, RoutedEventArgs e)
        {
            kinectManager.CloseRecord();
        }

        int i = 0;
        int s = 0;
        bool flag = false;
        //double border = 49.03325;
        double border = 49.03325;
        Point op = new Point { X = 0, Y = 0 };
        double v = 0;
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            double t = (e.Timestamp - i)*0.001;
            if (t == 0) return;
            Point p = e.GetPosition(this);
            p.X = p.X * 0.345 / 1980;
            p.Y = p.Y * 0.194 / 1080;
            double m = Math.Sqrt(Math.Pow((p.X - op.X),2) + Math.Pow((p.Y - op.Y), 2));
            double ms = m / t;
            double mss = (ms - v) / t;
            //Console.WriteLine(m);
            //Console.WriteLine(ms);
            //Console.WriteLine(mss);
            //Console.WriteLine();
            if (mss > border)
            {
                if (flag == false)
                {
                    s = e.Timestamp;
                    flag = true;
                    //Console.WriteLine(1);
                }
                else
                {
                    //Console.WriteLine(2);
                }
            }
            if (mss < border)
            {
                if (flag == true)
                {
                    //Console.WriteLine(3);
                    int st = e.Timestamp - s;
                    if (st > 15 && st < 25)
                    {
                        Console.Beep();
                        //Console.WriteLine(4);
                    }
                    flag = false;
                }
                else
                {
                    //Console.WriteLine(5);
                }
            }
            Text1.Text = "X=" + p.X + ", Y=" + p.Y;
            Text2.Text = e.Timestamp.ToString();
            Text3.Text = t.ToString();
            Text4.Text = m.ToString();
            Text5.Text = ms.ToString();
            Text6.Text = mss.ToString();
            i = e.Timestamp;
            op = p;
            v = ms;
        }

        public MidiManager midiManager = new MidiManager();
        private void Midi_Click(object sender, RoutedEventArgs e)
        {
            midiManager.PlayMidi();
            //midiManager.OnNote(60);
            //Console.Beep();
        }
    }
}
