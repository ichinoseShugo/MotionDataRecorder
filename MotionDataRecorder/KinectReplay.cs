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
using System.Windows.Threading;

namespace MotionDataRecorder
{
    class KinectReplay
    {
        MainWindow main;

        private List<int> timeTable = new List<int>();
        private List<float[]> body = new List<float[]>();
        private List<float[]> front = new List<float[]>();
        private List<float[]> side = new List<float[]>();

        public int index = 0;

        public System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        float center_x = 0;
        float center_y = 0;
        public KinectReplay(MainWindow mainWindow)
        {
            main = mainWindow;
            InitializeReplay();
            center_x = (float)main.CanvasMidi.ActualWidth / 2;
            center_y = (float)main.CanvasMidi.ActualHeight / 2;
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
                using (StreamReader sr = new StreamReader(dialog.FileName))
                {
                    String line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] tokens = line.Split(',');
                        //タイムテーブルに時間を追加
                        timeTable.Add(int.Parse(tokens[0]));
                        //SPINE_BASEを原点とする
                        float[] center = new float[3] { float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]) };
                        //各リスト格納用配列
                        float[] bElements = new float[tokens.Length - 1]; //3次元座標
                        float[] fElements = new float[(tokens.Length - 1) * 2 / 3]; //x,y座標を格納
                        float[] sElements = new float[(tokens.Length - 1) * 2 / 3]; //z,y座標を格納
                        for (int i = 0; i < tokens.Length / 3; i++)
                        {
                            bElements[i * 3] = (float.Parse(tokens[i * 3 + 1]) - center[0]);
                            bElements[i * 3 + 1] = -(float.Parse(tokens[i * 3 + 2]) - center[1]);
                            bElements[i * 3 + 2] = (float.Parse(tokens[i * 3 + 3]) - center[2]);

                            fElements[i * 2] = bElements[i * 3] * 100 + (float)(main.CanvasBody.ActualWidth / 8); //x
                            fElements[i * 2 + 1] = bElements[i * 3 + 1] * 100 + (float)main.CanvasBody.ActualHeight / 2; //y

                            sElements[i * 2] = bElements[i * 3 + 2] * 100 + (float)(main.CanvasBody.ActualWidth * 5 / 8); //z
                            sElements[i * 2 + 1] = bElements[i * 3 + 1] * 100 + (float)main.CanvasBody.ActualHeight / 2; //y
                        }
                        body.Add(bElements);
                        front.Add(fElements);
                        side.Add(sElements);
                    }
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
            rendering = true;
            sw.Start();
            Render();
        }

        public void StopReplay()
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            rendering = false;
        }

        private async void Render()
        {
            await Task.Run(() => WaitForRendering());
        }

        bool rendering = false;
        private void WaitForRendering()
        {
            while (rendering)
            {
                int waitTime = (int)(timeTable[index] - sw.ElapsedMilliseconds);
                if (waitTime < 0)
                {
                    index++;
                    continue;
                }
                else
                {
                    System.Threading.Thread.Sleep(waitTime);
                    main.CanvasPoint.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DrawSkeleton(front[index], side[index]);
                    }));
                    index++;
                    if (index >= front.Count - 1)
                    {
                        Console.WriteLine("reset");
                        index = 0;
                        sw.Restart();
                        mindex = 0;
                        gate = 0;
                        erase = true;
                    }
                }
            }
        }

        int[] mill = Midi.mill;
        int mindex = 0;
        int gate = 0;
        bool erase = true;
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (mindex >= mill.Length - 1) return;
            if(mill[mindex] <= sw.ElapsedMilliseconds)
            {
                DrawMidi(center_x, center_y, 10, Brushes.Red);
                gate = mill[mindex + 1] - mill[mindex];
                mindex++;
                erase = true;
            }
            else
            {
                if(mill[mindex] - gate <= sw.ElapsedMilliseconds && erase)
                {
                    main.CanvasMidi.Children.Clear();
                    erase = false;
                }
            }
        }

        private void CompositionTarget_SkeletonRendering(object sender, EventArgs e)
        {
            DrawSkeleton(front[index], side[index]);
            index++;
            if (index >= front.Count - 1) index = 0;
        }

        int[] edge = new int[] { 0, 1, 0, 12, 0, 16, 1, 20, 20, 2, 20, 4, 20, 8, 2, 3, 4, 5, 5, 6, 6, 7, 7, 21, 7, 22, 8, 9, 9, 10, 10, 11, 11, 23, 11, 24, 12, 13, 13, 14, 14, 15, 16, 17, 17, 18, 18, 19 };
        private void DrawSkeleton(float[] fData, float[] sData)
        {
            main.CanvasBody.Children.Clear();
            for (int i = 0; i < fData.Length / 2; i++)
            {
                DrawEllipse(fData[i * 2], fData[i * 2 + 1], 4, Brushes.Navy); // x, y
                DrawEllipse(sData[i * 2], sData[i * 2 + 1], 4, Brushes.Navy); // z, y
            }
            for (int i = 0; i < edge.Length / 2; i++)
            {
                int j1 = edge[i * 2] * 2;
                int j2 = edge[i * 2 + 1] * 2;
                DrawLine(fData[j1], fData[j1 + 1], fData[j2], fData[j2 + 1]);
                DrawLine(sData[j1], sData[j1 + 1], sData[j2], sData[j2 + 1]);
            }
        }

        /// <summary> 画面に円を表示 </summary>
        private void DrawEllipse(float x, float y, double r, Brush b)
        {
            Ellipse ellipse = new Ellipse()
            {
                Width = r,
                Height = r,
                Fill = b,
            };
            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, y);

            main.CanvasBody.Children.Add(ellipse);
        }
        
        private void DrawMidi(float x, float y, double r, Brush b)
        {
            Ellipse ellipse = new Ellipse()
            {
                Width = r,
                Height = r,
                Fill = b,
            };
            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, y);

            main.CanvasMidi.Children.Add(ellipse);
        }

        private void DrawLine(float x1, float y1, float x2, float y2)
        {
            Line line = new Line()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = Brushes.Blue,
            };

            main.CanvasBody.Children.Add(line);
        }

        public void Close()
        {
            this.rendering = false;
            StopReplay();
            main.CanvasBody.Children.Clear();
            if (timeTable != null)
            {
                timeTable.Clear();
                timeTable = null;
            }
            if (body != null)
            {
                body.Clear();
                body = null;
            }
            if (front != null)
            {
                front.Clear();
                front = null;
            }
            if (side != null)
            {
                side.Clear();
                side = null;
            }
        }
    }
}
