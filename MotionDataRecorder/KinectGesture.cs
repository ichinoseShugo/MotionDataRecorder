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
        bool ready = true;
        double maxDist = -1;

        public KinectGesture()
        {

        }

        public void RelativeHandTap(Body body)
        {
            var HandRight = body.Joints[JointType.HandRight].Position;
            var HandLeft = body.Joints[JointType.HandLeft].Position;
            var ShoulderRight = body.Joints[JointType.ShoulderRight].Position;
            var ShoulderLeft = body.Joints[JointType.ShoulderLeft].Position;

            Point3D LRs = new Point3D(ShoulderRight.X - ShoulderLeft.X, ShoulderRight.Y - ShoulderLeft.Y, ShoulderRight.Z - ShoulderLeft.Z);
            Point3D LRh = new Point3D(HandRight.X - HandLeft.X, HandRight.Y - HandLeft.Y, HandRight.Z - HandLeft.Z);

            double absLRs = Math.Sqrt(Math.Pow(LRs.X, 2) + Math.Pow(LRs.Y, 2) + Math.Pow(LRs.Z, 2));
            double t = (LRs.X * LRh.X + LRs.Y * LRh.Y + LRs.Z * LRh.Z) / absLRs;

            Point3D D = new Point3D(
                HandLeft.X - HandRight.X + t * LRs.X,
                HandLeft.Y - HandRight.Y + t * LRs.Y,
                HandLeft.Z - HandRight.Z + t * LRs.Z);
            double depth = Math.Sqrt(Math.Pow(D.X, 2) + Math.Pow(D.Y, 2) + Math.Pow(D.Z, 2));

            var z = (HandLeft.Z - HandRight.Z) * LRs.X - (HandLeft.X - HandRight.X) * LRs.Z;
            if (z > 0) depth *= -1;
            if (depth < 0)
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

        public void RelativeBodyTap(Body body)
        {
            var HandRight = body.Joints[JointType.HandRight].Position;
            var ShoulderRight = body.Joints[JointType.ShoulderRight].Position;

            var distance = Math.Sqrt(
                Math.Pow(HandRight.X - ShoulderRight.X, 2) +
                Math.Pow(HandRight.Y - ShoulderRight.Y, 2) +
                Math.Pow(HandRight.Z - ShoulderRight.Z, 2));

            if (distance > maxDist) maxDist = distance;
            if (distance > maxDist * 0.8)
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

        private Point3D JointToPoint3D(Body body, JointType jointType)
        {
            var p = body.Joints[jointType].Position;
            return new Point3D
            {
                X = p.X,
                Y = p.Y,
                Z = p.Z,
            };
        }

        public void Close()
        {

        }
    }//class
}//namespace
