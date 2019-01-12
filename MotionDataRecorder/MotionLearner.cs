using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MotionDataRecorder
{
    class MotionLearner
    {
        private List<int> timeTable = new List<int>();
        private List<float[]> jointsList = new List<float[]>();

        MainWindow main;
        double canvasWidth = 0;
        double unit = 1;
        public MotionLearner(MainWindow mainWindow, string filename)
        {
            main = mainWindow;
            canvasWidth = main.CanvasTarget.ActualWidth;
            unit = canvasWidth / 20000;
            FileToList(filename);
            Caliculate();
            DrawData();
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
                    for (int i = 0; i < tokens.Length-1; i++)
                    {
                        joints[i] = float.Parse(tokens[i+1]);
                    }
                    jointsList.Add(joints);
                }
            }
        }

        private void Caliculate()
        {
        }

        Polygon myPolygon;
        private int ShoulderRight = 24;
        private int HandRight = 33;
        private void DrawData()
        {
            myPolygon = new Polygon();
            myPolygon.Stroke = System.Windows.Media.Brushes.Black;
            myPolygon.Fill = System.Windows.Media.Brushes.LightSeaGreen;
            myPolygon.StrokeThickness = 2;
            myPolygon.HorizontalAlignment = HorizontalAlignment.Left;
            myPolygon.VerticalAlignment = VerticalAlignment.Center;

            PointCollection points = new PointCollection();
            for (int i=0; i<timeTable.Count; i++)
            {
                float[] joint = jointsList[i];
                var d = Math.Sqrt(
                    Math.Pow(joint[HandRight] - joint[ShoulderRight], 2) +
                    Math.Pow(joint[HandRight + 1] - joint[ShoulderRight + 1], 2) +
                    Math.Pow(joint[HandRight + 1] - joint[ShoulderRight + 2], 2))
                    * 60;
                    //+ main.CanvasTarget.ActualHeight / 2;
                Point p = new Point(timeTable[i] * unit, d);
                points.Add(p);
            }

            myPolygon.Points = points;
            main.CanvasTarget.Children.Add(myPolygon);
        }
    }
}
