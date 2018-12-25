using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace MotionDataRecorder
{
    class KinectReplay
    {
        MainWindow main;
        private KinectSensor kinect;
        private List<float[]> body = null;
        public int index = -1;

        public KinectReplay(MainWindow mainWindow, KinectSensor kinectSensor)
        {
            main = mainWindow;
            kinect = kinectSensor;
            InitializeReplay();
        }

        private void InitializeReplay()
        {
            var dialog = new OpenFileDialog();
            string startupPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Environment.GetCommandLineArgs()[0]));
            string dialogPath = startupPath.Replace("bin\\x64\\Debug","") + "Data\\Kinect";
            dialog.InitialDirectory = dialogPath;
            dialog.Title = "ファイルを開く";
            dialog.Filter = "csvファイル(*.*)|*.csv";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    body = new List<float[]>();
                    using (StreamReader sr = new StreamReader(dialog.FileName))
                    {
                        String line = "";
                        while ((line = sr.ReadLine()) != null) {
                            string[] tokens = line.Split(',');
                            float[] center = new float[3]{ float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3])};
                            float[] elements = new float[tokens.Length];
                            for (int i = 0; i < (tokens.Length - 1) / 3; i++)
                            {
                                elements[1 + 3 * i] = (float.Parse(tokens[1 + 3 * i]) - center[0]) * 100 + (float)main.CanvasBody.ActualWidth / 2;
                                elements[2 + 3 * i] = - (float.Parse(tokens[2 + 3 * i]) - center[1]) * 100 + (float)main.CanvasBody.ActualHeight / 2;
                                elements[3 + 3 * i] = (float.Parse(tokens[3 + 3 * i]) - center[2]) * 100;
                            }
                            body.Add(elements);
                        }
                    }
                    index = 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                    body = null;
                    return;
                }
            }
            else
            {
                Console.WriteLine("open file error");
                return;
            }
        }

        private void Map()
        {
            /*
            try
            {
                body = new List<float[]>();
                using (StreamReader sr = new StreamReader(dialog.FileName))
                {
                    String line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] tokens = line.Split(',');
                        float[] elements = new float[tokens.Length];
                        for (int i = 0; i < (tokens.Length - 1) / 3; i++)
                        {
                            Console.WriteLine(tokens[1 + 3 * i] + "," + tokens[2 + 3 * i] + "," + tokens[3 + 3 * i]);
                            ColorSpacePoint point = kinect.CoordinateMapper.MapCameraPointToColorSpace(new CameraSpacePoint()
                            {
                                X = float.Parse(tokens[1 + 3 * i]),
                                Y = float.Parse(tokens[2 + 3 * i]),
                                Z = float.Parse(tokens[3 + 3 * i]),
                            });
                            Console.WriteLine(point.X + "," + point.Y);
                            elements[1 + 3 * i] = point.X;
                            elements[2 + 3 * i] = point.Y;
                        }
                        body.Add(elements);
                    }
                }
                index = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                body = null;
                return;
            }
            */
        }

        public void StartReplay()
        {
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        public void StopReplay()
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            main.CanvasBody.Children.Clear();
            float[] data = body[index];
            for (int i = 0; i < (data.Length - 1) / 3; i++)
            {
                DrawEllipse(data[1 + 3 * i], data[2 + 3 * i]);
            }
            index++;
            if (index >= body.Count - 1) index = 0;
        }

        /// <summary> 画面に円を表示 </summary>
        private void DrawEllipse(float x, float y)
        {
            Ellipse ellipse = new Ellipse()
            {
                Width = 2,
                Height = 2,
                Fill = Brushes.Navy,
            };
            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, y);

            main.CanvasBody.Children.Add(ellipse);
        }
    }
}
