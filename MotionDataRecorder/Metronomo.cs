using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MotionDataRecorder
{
    public class Metronomo
    {
        private MainWindow main;
        public System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        private int index = 0;
        private static int[] times;

        private float X;
        private float Xspace;
        private float Y;
        private float Yspace;

        public Metronomo(MainWindow mainWindow)
        {
            main = mainWindow;
            times = Midi.mill;

            X = (float)main.CanvasTarget.ActualWidth / 4;
            Xspace = (float)main.CanvasTarget.ActualWidth / 6;
            Y = (float)main.CanvasTarget.ActualHeight / 4;
            Yspace = (float)main.CanvasTarget.ActualHeight / 12;
        }

        public void Start()
        {
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            PlayMidi();
            StartWatch();
            InitCanvas();
        }

        public void Stop()
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            stopwatch.Stop();
            //Midi.StopMidi();
        }

        private async void StartWatch()
        {
            await Task.Run(() =>
            {
                stopwatch.Start();
            });
        }

        private async void PlayMidi()
        {
            await Task.Run(() =>
            {
                Midi.PlayMidi();
            });
        }
        
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (index >= 32)
            {
                Stop();
                return;
            }
            if(times[index + 5] < stopwatch.ElapsedMilliseconds)
            {
                DrawEllipse(X + Xspace * (index % 4), Y + Yspace * (index / 4), Brushes.Red);
                index++;
            }
        }

        /// <summary> 画面に円を表示 </summary>
        private void DrawEllipse(float x, float y, Brush b)
        {
            Ellipse ellipse = new Ellipse()
            {
                Width = 10,
                Height = 10,
                Fill = b,
            };
            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, y);

            main.CanvasTarget.Children.Add(ellipse);
        }

        private void InitCanvas()
        {
            main.CanvasTarget.Children.Clear();
            for (int i = 0; i < 32; i++)
            {
                DrawEllipse(X + Xspace * (i % 4), Y + Yspace * (i / 4) , Brushes.Blue);
            }
        }

        #region sleep
        int[] time = new int[] { 0, 960, 1920, 2400, 2880, 3840, 4320, 4800, 5280, 5760, 6240, 6720, 7200, 7680, 8160, 8640, 9120, 9600, 10080, 10560, 11040, 11520, 12000, 12480, 12960, 13440, 13920, 14400, 14880, 15360, 15840, 16320, 16800, 17280, 17760, 18240, 18720 };
        private async void StartNoteBySleep()
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < time.Length; i++)
                {
                    int waitTime = (int)(time[i] - stopwatch.ElapsedMilliseconds);
                    System.Threading.Thread.Sleep(waitTime);
                    Midi.OnNote(11, 80, 240);
                }
            });
        }
        #endregion
    }
}
