using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionDataRecorder
{
    public static class JT
    {
        static public int SpineBase = 0;
        static public int SpineMid = 1;
        static public int Neck = 2;
        static public int Head = 3;
        static public int ShoulderLeft = 4;
        static public int ElbowLeft = 5;
        static public int WristLeft = 6;
        static public int HandLeft = 7;
        static public int ShoulderRight = 8;
        static public int ElbowRight = 9;
        static public int WristRight = 10;
        static public int HandRight = 11;
        static public int HipLeft = 12;
        static public int KneeLeft = 13;
        static public int AnkleLeft = 14;
        static public int FootLeft = 15;
        static public int HipRight = 16;
        static public int KneeRight = 17;
        static public int AnkleRight = 18;
        static public int FootRight = 19;
        static public int SpineShoulder = 20;
        static public int HandTipLeft = 21;
        static public int ThumbLeft = 22;
        static public int HandTipRight = 23;
        static public int ThumbRight = 24;

        static public int[] ArmRArray = { ShoulderRight, ElbowRight, WristRight, HandRight, HandTipRight };
        static public int[] ArmLArray = { ShoulderLeft, ElbowLeft, WristLeft, HandLeft, HandTipLeft };
        static public int[] LegRArray = { HipRight, KneeRight, AnkleRight, FootRight };
        static public int[] LegLArray = { HipLeft, KneeLeft, AnkleLeft, FootLeft };


        static public int[] ArmRArrayContainThumb = { ShoulderRight, ElbowRight, WristRight, HandRight, HandTipRight, ThumbRight };
        static public int[] ArmLArrayContainThumb = { ShoulderLeft, ElbowLeft, WristLeft, HandLeft, HandTipLeft, ThumbLeft };

        static public int Index(int jointType)
        {
            return jointType * 3;
        }
    }
}
