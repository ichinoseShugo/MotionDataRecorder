using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Kinect;
using LightBuzz.Vitruvius;
using System.Windows.Media.Media3D;

namespace MotionDataRecorder
{
    public class KinectGesture
    {
        MainWindow main;
        static int IndexHead = 0;
        static int IndexHandRight = 1;
        static int IndexHandLeft = 2;
        static int IndexFootRight = 3;
        static int IndexFootLeft = 4;

        long[] time = new long[] { 0, 0, 0, 0, 0 };
        long waitTime = 200;
        long[] t0 = new long[] { 0, 0, 0, 0, 0 };
        float[][] p0 = new float[][] { new float[] { 0, 0, 0 }, new float[] { 0, 0, 0 }, new float[] { 0, 0, 0 }, new float[] { 0, 0, 0 }, new float[] { 0, 0, 0 } };
        double[] v0 = new double[] { 0, 0, 0, 0, 0 };
        bool[] upValue = new bool[] { false, false, false, false, false}; 
        double[] angle0 = new double[] { 0, 0, 0, 0, 0 };

        long[] preOnTime = { 0, 0 };
        long[] preT = new long[] { 0, 0 };
        float[][] preTipP = new float[][] { new float[] { 0, 0, 0 }, new float[] { 0, 0, 0 } };
        float[][] preP = new float[][] { new float[] { 0, 0, 0 }, new float[] { 0, 0, 0 } };
        float[] preZ = new float[] { 0, 0 };

        static public double[][] Vel = new double[][] { new double[] { 0, 0 }, new double[] { 0, 0 }, new double[] { 0, 0 } };
        static public double[][] Acc = new double[][] { new double[] { 0, 0 }, new double[] { 0, 0 }, new double[] { 0, 0 } };
        static public double[][] AngVel = new double[][] { new double[] { 0, 0 }, new double[] { 0, 0 }, new double[] { 0, 0 } };

        static public string[] GestureList = new string[] { "E1-1", "E1-2", "E2-1-1", "E2-1-2", "E2-2-1", "E2-2-2", "E2-3-1", "E2-3-2" };

        public KinectGesture(MainWindow mainWindow)
        {
            main = mainWindow;
        }

        public void Recog(float[] joints)
        {
            CheckPrevious(joints, 0);
            CheckPrevious(joints, 1);
            //Test(IndexHandRight, joints, JT.HandRight * 3, JT.HandRight * 3, JT.ElbowRight * 3, JT.ShoulderRight * 3);
            //Test(IndexFootRight, joints, JT.KneeRight * 3, JT.AnkleRight * 3, JT.KneeRight * 3, JT.HipRight * 3);
        }

        bool upValueIsUsed = true;

        public void CheckVel(int pIndex, float[] joints, int jointType)
        {
            int tIndex = 0;
            if (pIndex == 1 || pIndex == 2) tIndex = 1;
            if (pIndex == 3 || pIndex == 4) tIndex = 2;

            long t = KinectManager.kinectTimer.ElapsedMilliseconds;
            double sumsq = 0;
            for (int i = 0; i < 3; i++) sumsq += Math.Pow((joints[jointType + i] - p0[pIndex][i]) / (t - t0[pIndex]), 2);
            double velocity = Math.Sqrt(sumsq);

            if (upValueIsUsed)
            {
                if (velocity - v0[pIndex] >= 0) upValue[pIndex] = true;
                if (velocity - v0[pIndex] < 0)
                {
                    if (upValue[pIndex])
                    {
                        if (t - time[pIndex] > waitTime)
                        {
                            if (velocity < Vel[tIndex][0] && Vel[tIndex][1] < velocity)
                            {
                                Midi.OnNote(11, Chord.NoteList[Chord.NoteIndex][Mapping(joints[jointType + 1])], 240);
                                time[pIndex] = t;
                            }
                        }
                    }
                    upValue[pIndex] = false;
                }
            }
            else
            {
                if (t - time[pIndex] > waitTime)
                {
                    if (velocity < Vel[tIndex][0] && Vel[tIndex][1] < velocity)
                    {
                        Midi.OnNote(11, Chord.NoteList[Chord.NoteIndex][Mapping(joints[jointType + 1])], 240);
                        time[pIndex] = t;
                    }
                }
            }

            p0[pIndex] = new float[] { joints[jointType], joints[jointType + 1], joints[jointType + 2] };
            t0[pIndex] = t;
            v0[pIndex] = velocity;
        }

        private void CheckVelAndAngle(int pIndex, float[] joints, int jointType, int start, int mid, int end)
        {
            int tIndex = 0;
            if (pIndex == 1 || pIndex == 2) tIndex = 1;
            if (pIndex == 3 || pIndex == 4) tIndex = 2;

            long t = KinectManager.kinectTimer.ElapsedMilliseconds;
            double sumsq = 0;
            for (int i = 0; i < 3; i++) sumsq += Math.Pow((joints[jointType + i] - p0[pIndex][i]) / (t - t0[pIndex]), 2);
            double velocity = Math.Sqrt(sumsq);

            float[] vec1 = Caliculater.JointsToVecArray(joints, mid, start);
            float[] vec2 = Caliculater.JointsToVecArray(joints, mid, end);
            double cos = Caliculater.VecToCos(vec1, vec2);
            double angle = Math.Acos(cos);
            double angleVelocity = (angle - angle0[pIndex]) / (t - t0[pIndex]);

            if (upValueIsUsed)
            {
                if (velocity - v0[pIndex] >= 0) upValue[pIndex] = true;
                if (velocity - v0[pIndex] < 0)
                {
                    if (upValue[pIndex])
                    {
                        if (t - time[pIndex] > waitTime)
                        {
                            if (velocity < Vel[tIndex][0] && Vel[tIndex][1] < velocity)
                            {
                                if (angleVelocity < AngVel[tIndex][0] && AngVel[tIndex][1] < angleVelocity)
                                {
                                    Midi.OnNote(11, Chord.NoteList[Chord.NoteIndex][Mapping(joints[jointType + 1])], 240);
                                    time[pIndex] = t;
                                }
                            }
                        }
                    }
                    upValue[pIndex] = false;
                }
            }
            else
            {
                if (t - time[pIndex] > waitTime)
                {
                    if (Vel == null) Console.WriteLine("null");
                    if (velocity < Vel[tIndex][0] && Vel[tIndex][1] < velocity)
                    {
                        if (angleVelocity < AngVel[tIndex][0] && AngVel[tIndex][1] < angleVelocity)
                        {
                            Midi.OnNote(11, Chord.NoteList[Chord.NoteIndex][Mapping(joints[jointType + 1])], 240);
                            time[pIndex] = t;
                        }
                    }
                }
            }

            p0[pIndex] = new float[] { joints[jointType], joints[jointType + 1], joints[jointType + 2] };
            t0[pIndex] = t;
            v0[pIndex] = velocity;
            angle0[pIndex] = angle;
        }

        public void E1_1(float[] joints)
        {
            CheckVelAndAngle(IndexHandRight, joints, JT.HandRight * 3, JT.HandRight * 3, JT.ElbowRight * 3, JT.ShoulderRight * 3);
            CheckVelAndAngle(IndexHandLeft, joints, JT.HandLeft * 3, JT.HandLeft * 3, JT.ElbowLeft * 3, JT.ShoulderLeft * 3);
        }

        public void E1_2(float[] joints)
        {
            CheckVel(IndexHandRight, joints, JT.HandRight * 3);
            CheckVel(IndexHandLeft, joints, JT.HandLeft * 3);
        }

        public void E2_1_1(float[] joints)
        {
            CheckVelAndAngle(IndexHandRight, joints, JT.HandRight * 3, JT.HandRight * 3, JT.ElbowRight * 3, JT.ShoulderRight * 3);
            CheckVelAndAngle(IndexHandLeft, joints, JT.HandLeft * 3, JT.HandLeft * 3, JT.ElbowLeft * 3, JT.ShoulderLeft * 3);
        }

        public void E2_1_2(float[] joints)
        {
            CheckVel(IndexHandRight, joints, JT.HandRight * 3);
            CheckVel(IndexHandLeft, joints, JT.HandLeft * 3);
        }


        public void E2_2_1(float[] joints)
        {
            CheckVelAndAngle(IndexHandRight, joints, JT.HandRight * 3, JT.HandRight * 3, JT.ElbowRight * 3, JT.ShoulderRight * 3);
            CheckVelAndAngle(IndexHandLeft, joints, JT.HandLeft * 3, JT.HandLeft * 3, JT.ElbowLeft * 3, JT.ShoulderLeft * 3);
            CheckVelAndAngle(IndexFootRight ,joints, JT.KneeRight * 3, JT.AnkleRight * 3, JT.KneeRight * 3, JT.HipRight * 3);
            CheckVelAndAngle(IndexFootLeft, joints, JT.KneeLeft * 3, JT.AnkleLeft * 3, JT.KneeLeft * 3, JT.HipLeft * 3);
        }

        public void E2_2_2(float[] joints)
        {
            CheckVel(IndexHandRight, joints, JT.HandRight * 3);
            CheckVel(IndexHandLeft, joints, JT.HandLeft * 3);
            CheckVel(IndexFootRight, joints, JT.KneeRight * 3);
            CheckVel(IndexFootLeft, joints, JT.KneeLeft * 3);
        }

        public void E2_3_1(float[] joints)
        {
            CheckVelAndAngle(IndexHead, joints, JT.Head * 3, JT.Head * 3, JT.SpineMid * 3, JT.SpineBase * 3);
            CheckVelAndAngle(IndexHandRight, joints, JT.HandRight * 3, JT.HandRight * 3, JT.ElbowRight * 3, JT.ShoulderRight * 3);
            CheckVelAndAngle(IndexHandLeft, joints, JT.HandLeft * 3, JT.HandLeft * 3, JT.ElbowLeft * 3, JT.ShoulderLeft * 3);
            CheckVelAndAngle(IndexFootRight, joints, JT.KneeRight * 3, JT.AnkleRight * 3, JT.KneeRight * 3, JT.HipRight * 3);
            CheckVelAndAngle(IndexFootLeft, joints, JT.KneeLeft * 3, JT.AnkleLeft * 3, JT.KneeLeft * 3, JT.HipLeft * 3);
        }

        public void E2_3_2(float[] joints)
        {
            CheckVel(IndexHead, joints, JT.Head * 3);
            CheckVel(IndexHandRight, joints, JT.HandRight * 3);
            CheckVel(IndexHandLeft, joints, JT.HandLeft * 3);
            CheckVel(IndexFootRight, joints, JT.KneeRight * 3);
            CheckVel(IndexFootLeft, joints, JT.KneeLeft * 3);
        }

        private void CheckPrevious(float[] joints, int RorL)
        {
            long t = KinectManager.kinectTimer.ElapsedMilliseconds;

            int HandTipIndex = JT.HandTipLeft * 3;
            if (RorL == 0) HandTipIndex = JT.HandTipRight * 3;
            double TipSumsq = 0;
            for (int i = 0; i < 3; i++) TipSumsq += Math.Pow((joints[HandTipIndex + i] - preTipP[RorL][i]) / (t - preT[RorL]), 2);
            double TipVelocity = Math.Sqrt(TipSumsq) * 1000;

            float z = joints[HandTipIndex + 2];
            float deltaZ = z - preZ[RorL];

            int HandIndex = JT.HandLeft * 3;
            if (RorL == 0) HandIndex = JT.HandRight * 3;
            double Sumsq = 0;
            for (int i = 0; i < 3; i++) Sumsq += Math.Pow((joints[HandIndex + i] - preP[RorL][i]) / (t - preT[RorL]), 2);
            double Velocity = Math.Sqrt(Sumsq) * 1000;
            
            if (TipVelocity > 0.03 // 指先の速度が1.8m/s以上 0.03
                && Velocity > 0.01 // 手のひらの速度が0.6m/s以上 0.01
                && deltaZ < -0.02 // 1F(約1/60秒)あたりの深度の変化が0.02m以上 0.02
                && (t - preOnTime[RorL]) > 300) //rsw.ElapsedMilliseconds > 300
            {
                Midi.OnNote(11, Chord.NoteList[Chord.NoteIndex][Mapping(joints[HandIndex + 1])], 240);
                preOnTime[RorL] = t;
            }
            
            preT[RorL] = t;
            preTipP[RorL] = new float[] { joints[HandTipIndex], joints[HandTipIndex + 1], joints[HandTipIndex + 2] };
            preP[RorL] = new float[] { joints[HandIndex], joints[HandIndex + 1], joints[HandIndex + 2] };
            preZ[RorL] = z;
        }

        /// <summary> yの値をmidiに変換 </summary>
        public int Mapping(float y)
        {
            int index = (int)(y * 10);
            if (index > 8) index = 8;
            if (index < 0) index = 0;

            return index;
        }

        public void Close()
        {

        }

        #region Recog
        /*
        public void Recog(float[] joints)
        {
            //CheckVelAndAngle(joints, JT.Head * 3, JT.Head * 3, JT.SpineMid * 3, JT.SpineBase * 3); // 0
            CheckVelAndAngle(joints, JT.HandRight * 3, JT.HandRight * 3, JT.ElbowRight * 3, JT.ShoulderRight * 3); // 1
            CheckVelAndAngle(joints, JT.KneeRight * 3, JT.AnkleRight * 3, JT.KneeRight * 3, JT.HipRight * 3); // 2

            //CheckPrevious(joints, 0);
            //CheckPrevious(joints, 1);

            //CheckVel(joints, JT.HandRight * 3);
            //CheckAccAndAngle(joints, JT.HandRight * 3, JT.HandRight * 3, JT.ElbowRight * 3, JT.ShoulderRight * 3);
            //CheckVelAndAngle(joints, JT.KneeRight * 3, JT.KneeRight * 3, JT.SpineBase * 3, JT.SpineMid * 3);
            //CheckAccAndAngle(joints, JT.KneeRight * 3, JT.KneeRight * 3, JT.SpineBase * 3, JT.SpineMid * 3);
        }
        */
        #endregion

        #region Vel
        /*
        private void CheckVel(float[] joints, int jointType)
        {
            int pIndex = 0;
            if (jointType == JT.Head * 3) pIndex = 0;
            if (jointType == JT.HandRight * 3 || jointType == JT.HandLeft) pIndex = 1;
            if (jointType == JT.KneeRight * 3 || jointType == JT.KneeLeft) pIndex = 2;

            long t = KinectManager.kinectTimer.ElapsedMilliseconds;
            double sumsq = 0;
            for (int i = 0; i < 3; i++) sumsq += Math.Pow((joints[jointType + i] - p0[pIndex][i]) / (t - t0[pIndex]), 2);
            double velocity = Math.Sqrt(sumsq);

            if (t - time > waitTime) ready = true;
            if (ready)
            {
                if (velocity < Vel[pIndex][0] && Vel[pIndex][1] < velocity)
                {
                    Midi.OnNote(11, Chord.NoteList[Chord.NoteIndex][Mapping(joints[jointType + 1])], 240);
                    //Console.WriteLine(t - time);
                    ready = false;
                    time = t;
                }
            }

            p0[pIndex] = new float[] { joints[jointType], joints[jointType + 1], joints[jointType + 2] };
            t0[pIndex] = t;
            v0[pIndex] = velocity;
        }
        */
        #endregion

        #region Judge
        /*
        public bool Judge_time(double p1, double oldp1, double p1max, double p1min)
        {
            if (ready)
            {
                if (p1min < p1 && p1 < p1max)
                {
                    ready = false;
                    return true;
                }
            }
            return false;
        }

        public bool Judge(double p1, double oldp1, double p1max, double p1min)
        {
            if (p1 - oldp1 >= 0)
            {
                up = true;
            }
            else
            {
                if (up)
                {
                    if (p1min < p1 && p1 < p1max)
                    {
                        ready = false;
                        return true;
                    }
                }
                up = false;
            }
            return false;
        }

        public bool Judge(double p1, double oldp1, double p1max, double p1min, double p2, double p2max, double p2min)
        {
            if (p1 - oldp1 >= 0)
            {
                up = true;
                //Console.WriteLine(1);
            }
            else
            {
                //Console.WriteLine(2);
                if (up)
                {
                    //Console.WriteLine(3);
                    if (p1min < p1 && p1 < p1max)
                    {
                        //Console.WriteLine(4);
                        if (p2min < p2 && p2 < p2max)
                        {
                            ready = false;
                            return true;
                        }
                    }
                }
                up = false;
            }
            return false;
        }
        */
        #endregion

        #region acc

        /*
        private void CheckAccAndAngle(float[] joints, int jointType, int start, int mid, int end)
        {
            int pIndex = 0;
            if (jointType == JT.Head * 3) pIndex = 0;
            if (jointType == JT.HandRight * 3 || jointType == JT.HandLeft) pIndex = 1;
            if (jointType == JT.FootRight * 3 || jointType == JT.HandLeft) pIndex = 2;

            long t = KinectManager.kinectTimer.ElapsedMilliseconds;
            double sumsq = 0;
            for (int i = 0; i < 3; i++) sumsq += Math.Pow((joints[jointType + i] - p0[pIndex][i]) / (t - t0[pIndex]), 2);
            double velocity = Math.Sqrt(sumsq);
            double acc = (velocity - v0[pIndex]) / (t - t0[pIndex]);

            float[] vec1 = Caliculater.JointsToVecArray(joints, mid, start);
            float[] vec2 = Caliculater.JointsToVecArray(joints, mid, end);
            double cos = Caliculater.VecToCos(vec1, vec2);
            double angle = Math.Acos(cos);
            double angleVelocity = (angle - angle0[pIndex]) / (t - t0[pIndex]);

            if (t - time > waitTime) ready = true;
            if (ready)
            {
                if (acc < Acc[pIndex][0] && Acc[pIndex][1] < acc)
                {
                    if (angleVelocity < AngVel[pIndex][0] && AngVel[pIndex][1] < angleVelocity)
                    {
                        Midi.OnNote(11,Chord.NoteList[Chord.NoteIndex][Mapping(joints[jointType + 1])],240);
                        Console.WriteLine(joints[jointType + 1]);
                        ready = false;
                        time = t;
                    }
                }
            }
            
            //Console.WriteLine(acc + "," + Acc[pIndex][0] + "," + Acc[pIndex][1]);
            //if (Judge(acc, acc0[pIndex], Acc[pIndex][0], Acc[pIndex][1], angleVelocity, AngVel[pIndex][0], AngVel[pIndex][1]))
            //{
            //    Midi.OnNote(11,Chord.NoteList[Chord.NoteIndex][Mapping(joints[jointType + 1])],240);
            //    Console.WriteLine(joints[jointType + 1]);
            //}
            p0[pIndex] = new float[] { joints[jointType], joints[jointType + 1], joints[jointType + 2] };
            t0[pIndex] = t;
            v0[pIndex] = velocity;
            acc0[pIndex] = acc;
            angle0[pIndex] = angle;
        }
        */
        #endregion

        #region temporary
        /*
            double accThrethold = 0.0016;
            double angThrethold = 0.002;
            main.Text2.Text = velocity.ToString();
            if (velocity > accThrethold)
            {
                if(angleVelocity > angThrethold)
                Midi.OnNote(60);
            }
            */
        /*
        if (velocity - v0[pIndex] >= 0)
        {
            up = true;
        }
        else
        {
            if (up)
            {
                if (t - time > waitTime) ready = true;
                if (ready)
                {
                    if (acc < Acc[pIndex][0] && Acc[pIndex][1] < acc)
                    {
                        if (angleVelocity < AngVel[pIndex][0] && AngVel[pIndex][1] < angleVelocity)
                        {
                            Midi.OnNote(Chord.NoteList[Chord.NoteIndex][Mapping(joints[jointType + 1])]);
                            Console.WriteLine(joints[jointType + 1]);
                            ready = false;
                            time = t;
                        }
                    }
                }
            }
            up = false;
        }
        */
        #endregion

        #region param
        /*

                            public void Recog(float[] joints)
                {
                    switch (gestureKind)
                    {
                        case 0:
                            Distance(joints);
                            break;
                        case 1:
                            Angle(joints);
                            break;
                        case 2:
                            Y(joints);
                            break;
                        case 3:
                            Velocity(joints);
                            break;
                        case 4:
                            E(joints);
                            break;
                        case 5:
                            Accel(joints);
                            break;
                        case 6:
                            CheckVelAndAngle(joints);
                            break;
                        case 7:
                            CheckAccAndAngle(joints);
                            break;
                        default:
                            break;
                    }
    }

    private void Distance(float[] joints)
{
double distance = GetDistance(joints);
if (distance > minThrethold)
{
if (ready)
{
    Midi.OnNote(60);
    ready = false;
}
}
else
{
ready = true;
}
}

private double GetDistance(float[] joints)
{
float x = joints[jointStart] - joints[jointEnd];
float y = joints[jointStart + 1] - joints[jointEnd + 1];
float z = joints[jointStart + 2] - joints[jointEnd + 2];
return Math.Sqrt(Math.Pow(x,2) + Math.Pow(y,2) + Math.Pow(z,2));
}

double angle = 0;
double angle0 = 0;
double anglePerSec = 0;
static public bool angleFrag = true;
private void Angle(float[] joints)
{
long s = KinectManager.kinectTimer2.ElapsedMilliseconds;
long t = s - s0;
float[] ElbowToSholder = Caliculater.JointsToVecArray(joints, Norm.ElbowRight, Norm.ShoulderRight);
float[] ElbowToHand = Caliculater.JointsToVecArray(joints, Norm.ElbowRight, Norm.HandRight);
double cos = Caliculater.VecToCos(ElbowToSholder, ElbowToHand);
angle = Math.Acos(cos);
double ans = angle;
Console.WriteLine("B:" + angle + "," + maxThrethold);
anglePerSec = (angle - angle0) / t;
s0 = s;
if (minThrethold < ans && ans < maxThrethold)
{
if (ready && angleFrag)
{
    Midi.OnNote(60);
    ready = false;
}
}
else
{
ready = true;
}
}

private void Y(float[] joints)
{
double y = joints[Norm.HandRight + 2];

if (y > minThrethold)
{
if (ready)
{
    Midi.OnNote(60);
    ready = false;
}
}
else
{
ready = true;
}
}

float[] old = new float[] { 0, 0, 0 };
long oldTime = 0;
double oldY = -1;
private void Velocity(float[] joints)
{
long t = KinectManager.kinectTimer.ElapsedMilliseconds;
double y;
if (oldTime == 0)
{
y = 0;
}
else
{
y = Math.Sqrt(
    Math.Pow(joints[Norm.HandRight] - old[0], 2) +
    Math.Pow(joints[Norm.HandRight + 1] - old[1], 2) +
    Math.Pow(joints[Norm.HandRight + 1] - old[2], 2)) / (t - oldTime);
}

if (oldY <= y)
{
ready = true;
}
else
{
if (ready)
{
    Midi.OnNote(60);
    ready = false;
}
}

old = new float[] { joints[Norm.HandRight], joints[Norm.HandRight + 1], joints[Norm.HandRight + 2] };
oldTime = t;
oldY = y;

}

private void E(float[] joints)
{
long s = KinectManager.kinectTimer2.ElapsedMilliseconds;
long t = s - s0;
Point3D p = new Point3D { X = joints[Norm.HandRight], Y = joints[Norm.HandRight + 1], Z = joints[Norm.HandRight + 2] };
double v = Math.Sqrt((p.X - P0[0].X) + (p.Y - P0[0].Y) + (p.Z - P0[0].Z)) / t;
double a = (v - V0[0]) / t;

if (max < p.Y) max = p.Y;
if (min > p.Y) min = p.Y;

if (KinectManager.kinectTimer.ElapsedMilliseconds > 250)
{
main.Text7.Text = p.Y.ToString();
byte note = (byte)(100 * p.Y);
main.Text6.Text = note.ToString();
Midi.OnNote(63);
KinectManager.kinectTimer.Restart();
}
s0 = s;
//Fifo(t, p, v);
}


double max = -99;
double min = 99;
long s0 = 0;
long[] T0 = new long[] { 0, 0, 0 };
Point3D[] P0 = new Point3D[]         
{
new Point3D() { X=0,Y=0,Z=0},
new Point3D() { X = 0, Y = 0, Z = 0 },
new Point3D() { X = 0, Y = 0, Z = 0 },
};
double[] V0 = new double[] { 0, 0, 0, };
double[] acc0 = new double[] { 0, 0, 0, };
double acc = 0;
private void Accel(float[] joints)
{
long s = KinectManager.kinectTimer2.ElapsedMilliseconds;
long t = s - s0;
Point3D p = new Point3D { X = joints[Norm.HandRight], Y = joints[Norm.HandRight + 1], Z = joints[Norm.HandRight + 2] };
double v = Math.Sqrt(Math.Pow((p.X - P0[0].X), 2) + Math.Pow((p.Y - P0[0].Y), 2) + Math.Pow((p.Z - P0[0].Z), 2)) / t;
double delV = v - V0[0];
long delT = t;

acc = delV / delT;
s0 = s;
//double ans = (acc - acc0[0]) / t;
double ans = acc;

Fifo(t, p, v, acc);

if (max < p.Y) max = p.Y;
if (min > p.Y) min = p.Y;
}

private void Fifo(long t, Point3D p, double v, double acc)
{
for (int i = T0.Length-1; i > 0; i--)
{
T0[i] = T0[i - 1];
V0[i] = V0[i - 1];
p0[i] = p0[i - 1];
acc0[i] = acc0[i - 1];
}
T0[0] = t;
V0[0] = v;
P0[0] = p;
acc0[0] = acc;
}
*/

        /*
         *
                public void CheckAcc()
                {
                    if(acc > minThrethold)
                    {
                        Midi.OnNote(11, 80, 120);
                    }
                    Console.WriteLine(acc +","+ minThrethold);
                }

                public void CheckAngle()
                {
                    if (angle > minThrethold)
                    {
                        Midi.OnNote(11, 80, 120);
                    }
                    Console.WriteLine("CheckAngle:" + angle + "," + minThrethold);
                }

                public void CheckAnglePerSec()
                {
                    if (anglePerSec > minThrethold)
                    {
                        Midi.OnNote(11, 80, 120);
                    }
                    Console.WriteLine(angle + "," + minThrethold);
                }

         */
        #endregion

    }//class
}//namespace
