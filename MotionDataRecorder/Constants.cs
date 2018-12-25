using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MotionDataRecorder
{
    public class Constants
    {
        public const int COLOR_WIDTH = 640;
        public const int COLOR_HEIGHT = 480;
        public const int COLOR_FPS = 30;

        public const int DEPTH_WIDTH = 640;
        public const int DEPTH_HEIGHT = 480;
        public const int DEPTH_FPS = 30;

        public static bool RealSenseIsConnect = false;

        public static int deviceSelect = -1;
        public const int SET_KINECT = 1;
        public const int SET_REALSENSE = 2;

        /// <summary> color frameの高さを実際のimageの高さで割ったサイズの比率(座標の表示に利用) </summary>
        public static double kinectImageRate = 1;

        public static Brush[] brushes = new Brush[]
        {
            Brushes.Red,
            Brushes.OrangeRed,
            Brushes.Orange,
            Brushes.Yellow,
            Brushes.YellowGreen,
            Brushes.Green,
            Brushes.LightBlue,
            Brushes.Blue,
            Brushes.Navy,
            Brushes.Purple
        };
        public static Color[] colors = new Color[]
        {
            Colors.Red,
            Colors.OrangeRed,
            Colors.Orange,
            Colors.Yellow,
            Colors.YellowGreen,
            Colors.Green,
            Colors.LightBlue,
            Colors.Blue,
            Colors.Navy,
            Colors.Purple
        };

    }
}