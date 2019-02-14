using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Wpf;

namespace MotionDataRecorder
{
    class DataViewer
    {
        Plot myPlot;
        LineSeries[] midiSeries = new LineSeries[Midi.mill.Length];

        public static int joints = 0;
        public static int threthold = 1;

        public static double midimax = 1;
        public static double midimin = -1;

        public DataViewer(Plot plot)
        {
            myPlot = plot;
            Init();
        }

        private void Init()
        {
            SetAxis();
            SetMidi();
        }

        private void SetAxis()
        {
            // X軸の定義
            LinearAxis xAxis = new LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Bottom,
            };
            myPlot.Axes.Add(xAxis);

            // Y軸の定義
            LinearAxis yAxis = new LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
            };
            myPlot.Axes.Add(yAxis);
        }

        private void SetMidi()
        {
            for (int i = 0; i < midiSeries.Length; i++)
            {
                DataPoint[] midi = new DataPoint[2];
                midi[0] = new DataPoint(Midi.mill[i], midimax);
                midi[1] = new DataPoint(Midi.mill[i], midimin);
                midiSeries[i] = new LineSeries()
                {
                    Color = Color.FromRgb(200, 50, 0),
                    StrokeThickness = 1,
                    ItemsSource = midi,
                };
                if (i > 4 && i % 4 == 1)
                {
                    midiSeries[i].Color = Color.FromRgb(255, 0, 0);
                    midiSeries[i].StrokeThickness = 2;
                }
                myPlot.Series.Add(midiSeries[i]);
            }
            myPlot.InvalidatePlot();
        }

        public void UpdateMidi(double max, double min)
        {
            midimax = max;
            midimin = min;
            for (int i = 0; i < midiSeries.Length; i++)
            {
                DataPoint[] midi = new DataPoint[2];
                midi[0] = new DataPoint(Midi.mill[i], min);
                midi[1] = new DataPoint(Midi.mill[i], max);
                myPlot.Series[i + 3].ItemsSource = midi;
            }
            myPlot.InvalidatePlot();
        }

        public void Update(int index, DataPoint[] points)
        {
            myPlot.Series[index].ItemsSource = points;
            // Plotを更新する
            myPlot.InvalidatePlot();
        }
    }
}
