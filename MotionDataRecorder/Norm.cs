using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace MotionDataRecorder
{
    class Norm
    {
        static int SpineBase = 0; //1 → (1-1)*3
        static int SpineMid = 3; //2 → (2-1)*3
        static int ShoulderLeft = 12; //5 → (5-1)*3
        static int ShoulderRight = 24; //9 → (9-1)*3

        static public float[] ToModel(float[] joints)
        {
            float[] array = new float[joints.Length];
            float[] spine_b = ToJoint(joints, SpineBase);
            float[] spine_m = ToJoint(joints, SpineMid);
            float[] shoulder_l = ToJoint(joints, ShoulderLeft);
            float[] shoulder_r = ToJoint(joints, ShoulderRight);

            float[] ex = ToVec(shoulder_r, shoulder_l);
            float[] ey = ToVec(spine_b, spine_m);
            float[] ez = ToCross(ex, ey);

            return array;
        }

        static private float[] ToJoint(float[] joints, int jointType)
        {
            return new float[] { joints[jointType], joints[jointType + 1], joints[jointType + 2] };
        }

        static private float[] ToVec(float[] start, float[] end)
        {
            float[] vec = new float[] { end[0] - start[0], end[1] - start[1], end[2] - start[2] };
            float abs = (float)Math.Sqrt(Math.Pow(vec[0], 2) + Math.Pow(vec[1], 2) + Math.Pow(vec[2], 2));
            return new float[] { vec[0] / abs, vec[1] / abs, vec[2] / abs};
        }
        
        static private float[] ToCross(float[] a, float[] b)
        {
            float x = a[2] * b[3] - a[3] * b[2];
            float y = a[3] * b[1] - a[1] * b[3];
            float z = a[1] * b[2] - a[2] * b[1];

            return new float[] { x, y, z };
        }
    }//class
}//namespace
