using OxyPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Windows.Media.Media3D;

namespace MotionDataRecorder
{
    class MotionLearner
    {
        private List<int> timeTable = new List<int>();
        private List<float[]> jointsList = new List<float[]>();

        public DataViewer viewer;
        MainWindow main;

        private int JointType = 33; // hand right
        private int[] anglePoint = new int[] { JT.ShoulderRight, JT.ElbowRight, JT.HandRight};

        DataPoint[] datapointList = null;
        DataPoint[][] theretholdList = new DataPoint[2][];
        
        /// <summary> 0:max 1:min </summary>
        double[] sliderInit = new double[] { 0, 0 };
        /// <summary> 0:max 1:min </summary>
        double[] MaxMin = new double[] { -9999, 9999};
        /// <summary> 0:max 1:min </summary>
        System.Windows.Controls.Slider[] sliders = new System.Windows.Controls.Slider[2];

        public MotionLearner(MainWindow mainWindow, DataViewer dataViewer, string filename)
        {
            main = mainWindow;
            viewer = dataViewer;
            sliders[0] = main.MaxSlider;
            sliders[1] = main.MinSlider;
            FileToList(filename);
        }

        private void FileToList(string filename)
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                String line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    string[] tokens = line.Split(',');
                    //タイムテーブルに時間を追加
                    timeTable.Add(int.Parse(tokens[0]));
                    float[] joints = new float[tokens.Length - 1];
                    for (int i = 0; i < tokens.Length - 1; i++)
                    {
                        joints[i] = float.Parse(tokens[i + 1]);
                    }
                    jointsList.Add(joints);
                }
            }
        }

        public void SelectList(int index)
        {
            CheckJoint();
            switch (index)
            {
                case 0:
                    GetPList(0);
                    break;
                case 1:
                    GetPList(1);
                    break;
                case 2:
                    GetPList(2);
                    break;
                case 3:
                    GetDistanceList();
                    break;
                case 4:
                    GetVelocityList();
                    break;
                case 5:
                    GetAccelerationList();
                    break;
                case 6:
                    GetDeltaAccList();
                    break;
                case 7:
                    GetAngleList();
                    break;
                case 8:
                    GetAngularVelocityList();
                    break;
                case 9:
                    GetGravityVelocityList();
                    break;
                case 10:
                    GetGravityAccList();
                    break;
                case 11:
                    GetDeltaVelocityList();
                    break;
            }
        }

        private void CheckJoint()
        {
            if (main.HeadCheck.IsChecked == true)
            {
                JointType = JT.Head * 3;
                anglePoint = new int[] { JT.SpineBase * 3, JT.SpineMid * 3, JT.Head * 3 };
            }
            if (main.HandCheck.IsChecked == true)
            {
                JointType = JT.HandRight * 3;
                anglePoint = new int[] { JT.ShoulderRight * 3, JT.ElbowRight * 3, JT.HandRight *3 };
            }
            if (main.FootCheck.IsChecked == true)
            {
                //JointType = JT.AnkleRight * 3;
                anglePoint = new int[] { JT.HipRight * 3, JT.KneeRight * 3, JT.AnkleRight * 3 };
                JointType = JT.KneeRight * 3;
                //anglePoint = new int[] { JT.SpineMid * 3, JT.SpineBase * 3, JT.KneeRight * 3 };
            }
        }

        private void InitParameter()
        {
            datapointList = new DataPoint[timeTable.Count];
            MaxMin = new double[] { -9999, 9999 };
            for (int i = 0; i < 2; i++) theretholdList[i] = new DataPoint[2];
        }

        private void UpdateView()
        {
            viewer.Update(0, datapointList);
            for (int i = 0; i < 2; i++)
            {
                sliderInit[i] = MaxMin[i];
                if (MaxMin[0] > 9999) MaxMin[0] = 0;
                if (MaxMin[1] < -9999) MaxMin[0] = 0;
                sliders[i].Maximum = MaxMin[0];
                sliders[i].Minimum = MaxMin[1];
                theretholdList[i][0] = new DataPoint(timeTable[0], sliderInit[i]);
                theretholdList[i][1] = new DataPoint(timeTable[timeTable.Count - 1], sliderInit[i]);
                viewer.Update(i + 1, theretholdList[i]);
            }
            viewer.UpdateMidi(MaxMin[0], MaxMin[1]);
        }

        /// <summary> 手と肩、頭と腹、足の付け根と足の距離 </summary>
        private void GetDistanceList()
        {
            InitParameter();
            for (int i = 0; i < timeTable.Count; i++)
            {
                float[] joint = jointsList[i];
                double distance = Math.Sqrt(
                    Math.Pow(joint[anglePoint[2]    ] - joint[anglePoint[0]], 2) +
                    Math.Pow(joint[anglePoint[2] + 1] - joint[anglePoint[0] + 1], 2) +
                    Math.Pow(joint[anglePoint[2] + 2] - joint[anglePoint[0] + 2], 2));
                UpdateMaxMin(distance);
                datapointList[i] = new DataPoint(timeTable[i], distance);
            }
            UpdateView();
        }
        
        /// <summary> 手肘肩の角度 全開で2,3前後の値 </summary>
        private void GetAngleList()
        {
            InitParameter();
            for (int i = 0; i < timeTable.Count; i++)
            {
                double angle = Caliculater.GetAngle(jointsList[i], anglePoint[0], anglePoint[1], anglePoint[2]);                
                UpdateMaxMin(angle);
                datapointList[i] = new DataPoint(timeTable[i], angle);
            }
            UpdateView();
        }

        /// <summary> 手の座標 </summary>
        private void GetPList(int axis)
        {
            InitParameter();
            for (int i = 0; i < timeTable.Count; i++)
            {
                float[] joint = jointsList[i];
                double p = joint[JointType + axis];
                UpdateMaxMin(p);
                datapointList[i] = new DataPoint(timeTable[i], p);
            }
            UpdateView();
        }

        /// <summary> 速度 </summary>
        private void GetVelocityList()
        {
            InitParameter();
            float[] p0 = new float[] { jointsList[0][JointType], jointsList[0][JointType + 1], jointsList[0][JointType + 2] };
            datapointList[0] = new DataPoint(timeTable[0], 0);
            for (int i = 1; i < timeTable.Count; i++)
            {
                var joint = jointsList[i];
                double velocity = Caliculater.GetVelocity(joint, JointType, p0, (timeTable[i] - timeTable[i - 1]));

                if (i < 10) velocity = 0;
                UpdateMaxMin(velocity);
                datapointList[i] = new DataPoint(timeTable[i], velocity);

                p0 = new float[] { joint[JointType], joint[JointType + 1], joint[JointType + 2] };
            }
            UpdateView();
        }

        /// <summary> 加速度 </summary>
        private void GetAccelerationList()
        {
            InitParameter();
            float[] p0 = new float[] { jointsList[0][JointType], jointsList[0][JointType + 1], jointsList[0][JointType + 2] };
            double v0 = 0;
            datapointList[0] = new DataPoint(timeTable[0], 0);
            for (int i = 1; i < timeTable.Count; i++)
            {
                float[] joint = jointsList[i];
                int t = timeTable[i] - timeTable[i - 1];
                double v = Caliculater.GetVelocity(joint, JointType, p0, t);
                double a = (v - v0) / t;
                if (i < 3) a = 0;
                UpdateMaxMin(a);
                datapointList[i] = new DataPoint(timeTable[i], a);
                p0 = new float[] { joint[JointType], joint[JointType + 1], joint[JointType + 2] };
                v0 = v;
            }
            UpdateView();
        }

        /// <summary> 加速度の変化量 </summary>
        private void GetDeltaAccList()
        {
            InitParameter();

            float[] p0 = new float[] { jointsList[0][JointType], jointsList[0][JointType + 1], jointsList[0][JointType + 2] };
            double v0 = 0;
            double a0 = 0;
            for (int i = 1; i < timeTable.Count; i++)
            {
                float[] joint = jointsList[i];
                int t = timeTable[i] - timeTable[i-1];
                double v = Caliculater.GetVelocity(joint, JointType, p0, t);
                double a = (v - v0) / t;
                double delA = (a - a0) / t;
                if (i < 3) delA = 0;
                if (timeTable[i] > 19500) delA = 0;
                UpdateMaxMin(delA);
                v0 = v;
                a0 = a;
                datapointList[i] = new DataPoint(timeTable[i], delA);
            }
            UpdateView();
        }

        /// <summary> 角速度 </summary>
        private void GetAngularVelocityList()
        {
            InitParameter();
            double angle0 = 0;
            datapointList[0] = new DataPoint(timeTable[0], 0);
            for (int i = 1; i < timeTable.Count; i++)
            {
                double angle = Caliculater.GetAngle(jointsList[i], anglePoint[0], anglePoint[1], anglePoint[2]);
                double aVelocity = (angle - angle0) / (timeTable[i] - timeTable[i - 1]);
                if (timeTable[i] < 2000) aVelocity = 0;
                UpdateMaxMin(aVelocity);
                datapointList[i] = new DataPoint(timeTable[i], aVelocity);
                angle0 = angle;
            }
            UpdateView();
        }

        /// <summary> 重心速度 </summary>
        private void GetGravityVelocityList()
        {
            InitParameter();
            datapointList[0] = new DataPoint(timeTable[0], 0);
            float[] g0 = new float[3];
            for (int i = 1; i < timeTable.Count; i++)
            {
                float[] g = new float[3];
                for (int axis = 0; axis < 3; axis++)
                {
                    int[] jointGroup = JT.ArmRArray;
                    float p = 0;
                    for (int groupNum = 0; groupNum < jointGroup.Length; groupNum++)
                    {
                        p += jointsList[i][jointGroup[groupNum] * 3 + axis] / jointGroup.Length;
                    }
                    g[axis] = p;
                }
                double sumsq = 0;
                int time = timeTable[i] - timeTable[i - 1];
                for(int j=0; j<3; j++) sumsq += Math.Pow((g[j] - g0[j]) / time, 2);
                double gv = Math.Sqrt(sumsq);
                
                UpdateMaxMin(gv);
                datapointList[i] = new DataPoint(timeTable[i], gv);

                g0 = g;
            }
            UpdateView();
        }

        /// <summary> 重心加速度 </summary>
        private void GetGravityAccList()
        {
            InitParameter();
            datapointList[0] = new DataPoint(timeTable[0], 0);
            float[] g0 = new float[3];
            double gv0 = 0;
            for (int i = 1; i < timeTable.Count; i++)
            {
                float[] g = new float[3];
                for (int axis = 0; axis < 3; axis++)
                {
                    int[] jointGroup = JT.ArmRArray;
                    float p = 0;
                    for (int groupNum = 0; groupNum < jointGroup.Length; groupNum++)
                    {
                        p += jointsList[i][jointGroup[groupNum] * 3 + axis] / jointGroup.Length;
                    }
                    g[axis] = p;
                }
                double sumsq = 0;
                int time = timeTable[i] - timeTable[i - 1];
                for (int j = 0; j < 3; j++) sumsq += Math.Pow((g[j] - g0[j]) / time, 2);
                double gv = Math.Sqrt(sumsq);
                double ga = (gv - gv0) / time;
                UpdateMaxMin(ga);
                datapointList[i] = new DataPoint(timeTable[i], ga);

                g0 = g;
                gv0 = gv;
            }
            UpdateView();
        }

        private void UpdateMaxMin(double ans)
        {
            if (MaxMin[0] < ans) MaxMin[0] = ans;
            if (MaxMin[1] > ans) MaxMin[1] = ans;
        }

        private void GetDeltaVelocityList()
        {
            InitParameter();
            float[] p0 = new float[] { jointsList[0][JointType], jointsList[0][JointType + 1], jointsList[0][JointType + 2] };
            datapointList[0] = new DataPoint(timeTable[0], 0);
            double v0 = 0;
            for (int i = 1; i < timeTable.Count; i++)
            {
                var joint = jointsList[i];
                double velocity = Caliculater.GetVelocity(joint, JointType, p0, (timeTable[i] - timeTable[i - 1]));
                double deltavel = velocity - v0;
                if (i < 3) deltavel = 0;
                UpdateMaxMin(deltavel);
                datapointList[i] = new DataPoint(timeTable[i], deltavel);

                p0 = new float[] { joint[JointType], joint[JointType + 1], joint[JointType + 2] };
                v0 = velocity;
            }
            UpdateView();
        }

        public void ValueChange(int index, double value)
        {
            int tindex = index - 1; // max:0 min:1
            theretholdList[tindex][0] = new DataPoint(timeTable[0], value);
            theretholdList[tindex][1] = new DataPoint(timeTable[timeTable.Count - 1], value);
            viewer.Update(index, theretholdList[tindex]);
        }

    }//
}//
