using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MotionDataRecorder
{
    class Norm
    {
        static public int SpineBase = 0; //1 → (1-1)*3
        static public int SpineMid = 3; //2 → (2-1)*3
        static public int ShoulderLeft = 12; //5 → (5-1)*3
        static public int ShoulderRight = 24; //9 → (9-1)*3
        static public int HipLeft = 36; //13
        static public int HipRight = 48; //17

        static public float[] ToModel(float[] joints)
        {
            float[] array = new float[joints.Length];
            float[] spine_b = ToJoint(joints, SpineBase);
            float[] spine_m = ToJoint(joints, SpineMid);
            float[] shoulder_l = ToJoint(joints, ShoulderLeft);
            float[] shoulder_r = ToJoint(joints, ShoulderRight);
            float[] hip_l = ToJoint(joints, HipLeft);
            float[] hip_r = ToJoint(joints, HipRight);

            //float[] ex = ToVec(shoulder_r, shoulder_l);
            float[] ex = ToVec(hip_r, hip_l);
            float[] ey = ToVec(spine_b, spine_m);
            float[] ez = ToCross(ex, ey);

            Matrix<double> InvA = Matrix.Build.DenseOfArray(new double[,] { 
                { ex[0], ex[1], ex[2] },
                { ey[0], ey[1], ey[2] },
                { ez[0], ez[1], ez[2] } }).Inverse();

            for (int i = 0; i < joints.Length / 3; i++)
            {
                var x = i * 3;
                var y = i * 3 + 1;
                var z = i * 3 + 2;

                Vector<double> v = Vector.Build.DenseOfArray(new double[] { joints[x], joints[y], joints[z] });
                var p = InvA * v;

                array[x] = (float)p[0];
                array[y] = (float)p[1];
                array[z] = (float)p[2];
            }

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
            float x = a[1] * b[2] - a[2] * b[1];
            float y = a[2] * b[0] - a[0] * b[2];
            float z = a[0] * b[1] - a[1] * b[0];
            float abs = (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            return new float[] { x / abs, y / abs, z / abs };
        }
    }//class
}//namespace
