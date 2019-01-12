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
        private List<float[]> jointList = new List<float[]>();
        private List<float[]> frontList = new List<float[]>();
        private List<float[]> sideList = new List<float[]>();

        private List<float[]> xAxis = null;
        private List<float[]> yAxis = null;

        public int index = 0;

        public System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        PointF mCenter;

        public KinectReplay(MainWindow mainWindow)
        {
            main = mainWindow;
            InitializeReplay();

            mCenter.X= (float)main.CanvasMidi.ActualWidth / 2;
            mCenter.Y = (float)main.CanvasMidi.ActualHeight / 2;
        }

        private void InitializeReplay()
        {
            var dialog = new OpenFileDialog();
            string startupPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Environment.GetCommandLineArgs()[0]));
            string dialogPath = startupPath.Replace("bin\\x64\\Debug", "") + "Data\\Kinect";
            dialog.InitialDirectory = dialogPath;
            dialog.Title = "ファイルを開く";
            dialog.Filter = "csvファイル(*.*)|*.csv";
            if (dialog.ShowDialog() == true)
            {
                using (StreamReader sr = new StreamReader(dialog.FileName))
                {
                    //LineToData(sr, true);
                    LineToNormData(sr, true);
                }
            }
            else
            {
                Console.WriteLine("open file error");
                return;
            }
        }

        private void LineToData(StreamReader sr, bool moveOrigin)
        {
            String line = "";
            while ((line = sr.ReadLine()) != null)
            {
                string[] tokens = line.Split(',');
                //タイムテーブルに時間を追加
                timeTable.Add(int.Parse(tokens[0]));

                //SPINE_BASEを原点とする
                float[] origin = new float[3] { float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]) };
                if (moveOrigin == false) origin = new float[3] { 0, 0, 0 };

                //各リスト格納用配列
                float[] joint = TokensTo3DPoints(tokens, origin);
                jointList.Add(joint);
                GetCanvasPoint(joint);
            }
        }

        private void LineToNormData(StreamReader sr, bool moveOrigin)
        {
            xAxis = new List<float[]>();
            yAxis = new List<float[]>();
            String line = "";
            while ((line = sr.ReadLine()) != null)
            {
                string[] tokens = line.Split(',');
                //タイムテーブルに時間を追加
                timeTable.Add(int.Parse(tokens[0]));
                
                //SPINE_BASEを原点とする
                float[] origin = new float[3] { float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]) };
                if (moveOrigin == false) origin = new float[3] { 0, 0, 0 };

                //各リスト格納用配列
                float[] joint = TokensTo3DPoints(tokens, origin);
                var joint_norm = Norm.ToModel(joint);

                jointList.Add(joint_norm);
                GetCanvasPoint(joint_norm);
            }
        }

        private float[] TokensTo3DPoints(string[] tokens, float[] origin)
        {
            float[] joint = new float[tokens.Length - 1]; //0番目は時間
            for (int i = 0; i < joint.Length; i++)
            {
                int index_o = i % 3;
                joint[i] = (float.Parse(tokens[i + 1]) - origin[index_o]);
            }
            return joint;
        }

        private void GetCanvasPoint(float[] joint)
        {
            PointF center = new PointF() { X = (float)(main.CanvasReplayFront.ActualWidth / 2), Y = (float)main.CanvasReplayFront.ActualHeight / 2 };
            float[] front = new float[joint.Length * 2 / 3]; //x,y座標を格納
            float[] side = new float[joint.Length * 2 / 3]; //z,y座標を格納

            for (int i = 0; i < joint.Length / 3; i++)
            {
                int jx = i * 3, jy = i * 3 + 1, jz = i * 3 + 2;
                int x = i * 2, y = i * 2 + 1;

                front[x] = joint[jx] * 100 + center.X; //x
                front[y] = joint[jy] * -100 + center.Y; //y
                side[x] = joint[jz] * 100 + center.X; //z
                side[y] = joint[jy] * -100 + center.Y; //y
            }

            frontList.Add(front);
            sideList.Add(side);
        }

        private void GetAxis(List<float[]> axis, float[] joints, int type1, int type2, float[] origin)
        {
            float[] points = new float[]
            {
                (joints[type1] - origin[0]) * 100 + (float)(main.CanvasBody.ActualWidth / 8),
                (joints[type1 + 1] - origin[1]) * 100 + (float)main.CanvasBody.ActualHeight / 2,
                joints[type1 + 2] - origin[2],
                (joints[type2] - origin[0]) * 100 + (float)(main.CanvasBody.ActualWidth / 8),
                (joints[type2 + 1] - origin[1]) * 100 + (float)main.CanvasBody.ActualHeight / 2,
                joints[type2 + 2] - origin[2]
            };
            axis.Add(points);
        }

        public void StartReplay()
        {
            //CompositionTarget.Rendering += CompositionTarget_Rendering;
            rendering = true;
            sw.Start();
            Render();
        }

        public void StopReplay()
        {
            //CompositionTarget.Rendering -= CompositionTarget_Rendering;
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
                    main.CanvasMidi.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DrawSkeleton(frontList[index], sideList[index]);
                        //if (xAxis != null && yAxis != null) DrawAxis(xAxis[index], yAxis[index]);
                    }));
                    index++;
                    if (index >= frontList.Count - 1)
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
                DrawMidi(mCenter.X, mCenter.Y, 10, Brushes.Red);
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
            DrawSkeleton(frontList[index], sideList[index]);
            index++;
            if (index >= frontList.Count - 1) index = 0;
        }

        int[] edge = new int[] { 0, 1, 0, 12, 0, 16, 1, 20, 20, 2, 20, 4, 20, 8, 2, 3, 4, 5, 5, 6, 6, 7, 7, 21, 7, 22, 8, 9, 9, 10, 10, 11, 11, 23, 11, 24, 12, 13, 13, 14, 14, 15, 16, 17, 17, 18, 18, 19 };
        private void DrawSkeleton(float[] fData, float[] sData)
        {
            main.CanvasReplayFront.Children.Clear();
            main.CanvasReplaySide.Children.Clear();
            for (int i = 0; i < fData.Length / 2; i++)
            {
                DrawEllipse(fData[i * 2], fData[i * 2 + 1], 4, Brushes.Navy, main.CanvasReplayFront); // x, y
                DrawEllipse(sData[i * 2], sData[i * 2 + 1], 4, Brushes.Navy, main.CanvasReplaySide); // z, y
            }
            for (int i = 0; i < edge.Length / 2; i++)
            {
                int j1 = edge[i * 2] * 2;
                int j2 = edge[i * 2 + 1] * 2;
                DrawLine(fData[j1], fData[j1 + 1], fData[j2], fData[j2 + 1] , main.CanvasReplayFront);
                DrawLine(sData[j1], sData[j1 + 1], sData[j2], sData[j2 + 1] , main.CanvasReplaySide);
            }
        }

        private void DrawAxis(float[] xAxis, float[] yAxis)
        {
            DrawLine(xAxis[0], xAxis[1], xAxis[3], xAxis[4], main.CanvasReplayFront);
            DrawLine(yAxis[0], yAxis[1], yAxis[3], yAxis[4], main.CanvasReplayFront);
        }

        /// <summary> 画面に円を表示 </summary>
        private void DrawEllipse(float x, float y, double r, Brush b, Canvas canvas)
        {
            Ellipse ellipse = new Ellipse()
            {
                Width = r,
                Height = r,
                Fill = b,
            };
            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, y);

            canvas.Children.Add(ellipse);
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

        private void DrawLine(float x1, float y1, float x2, float y2, Canvas canvas)
        {
            Line line = new Line()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = Brushes.Blue,
            };

            canvas.Children.Add(line);
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
            if (jointList != null)
            {
                jointList.Clear();
                jointList = null;
            }
            if (frontList != null)
            {
                frontList.Clear();
                frontList = null;
            }
            if (sideList != null)
            {
                sideList.Clear();
                sideList = null;
            }
        }
    }
}
