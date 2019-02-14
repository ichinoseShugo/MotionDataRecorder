using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using OxyPlot;

namespace MotionDataRecorder
{
    public static class Caliculater
    {
        /// <summary> 3点の座標から角度 </summary>
        static public double GetAngle(float[] joint, int start, int mid, int end)
        {
            float[] vec1 = JointsToVecArray(joint, mid, start);
            float[] vec2 = JointsToVecArray(joint, mid, end);

            double cos = VecToCos(vec1, vec2);
            double angle = Math.Acos(cos);

            return angle;
        }

        /// <summary> 時系列順の2つの座標から速度 </summary>
        static public double GetVelocity(float[] joint, int jt, float[] p0, int time)
        {
            double vx = (joint[jt] - p0[0]) / time;
            double vy = (joint[jt + 1] - p0[1]) / time;
            double vz = (joint[jt + 2] - p0[2]) / time;
            double velocity = Math.Sqrt(vx * vx + vy * vy + vz * vz);

            return velocity;
        }

        public static float[] JointsToVecArray(float[] joint, int start, int end)
        {
            return new float[] { joint[start] - joint[end], joint[start + 1] - joint[end + 1], joint[start + 2] - joint[end + 2],};
        }

        public static double VecToCos(float[] v1, float[] v2)
        {
            float inner = v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
            double abs = Math.Sqrt((v1[0] * v1[0] + v1[1] * v1[1] + v1[2] * v1[2]) * (v2[0] * v2[0] + v2[1] * v2[1] + v2[2] * v2[2]));
            return inner / abs;
        }
    }
}
