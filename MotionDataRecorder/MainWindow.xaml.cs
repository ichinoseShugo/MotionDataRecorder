using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;
using Microsoft.Win32;

namespace MotionDataRecorder
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectManager kinectManager = null;
        KinectReplay kinectReplay = null;
        public Metronomo metronomo;
        List<ParameterListItem> paramList = new List<ParameterListItem>();
        public MainWindow()
        {
            InitializeComponent();
            Top = 0;
            Left = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Midi.InitMidi();
            Midi.InitAccompanimentDomain();
            metronomo = new Metronomo(this);
            MethodBox.ItemsSource = MethodList.method;
            GestureBox.ItemsSource = KinectGesture.GestureList;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (kinectManager != null)
            {
            }
            if(kinectReplay != null)
            {
                kinectReplay.Close();
            }
            Metronomo.stopwatch.Stop();
        }

        private void KinectButton_Click(object sender, RoutedEventArgs e)
        {
            if(kinectReplay != null)
            {
                kinectReplay.Close();
            }
            if (kinectManager == null)
            {
                Console.WriteLine("try to open kinect");
                kinectManager = new KinectManager(this);
            }
            else
            {
                kinectManager.StartFrameRead();
            }
        }

        #region record
        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if (kinectManager != null)
            {
                kinectManager.record.StartRecord();
                metronomo.Start();
                Console.WriteLine(Metronomo.stopwatch.ElapsedMilliseconds - kinectManager.record.recTimer.ElapsedMilliseconds);
            }
        }

        private void Record_Unchecked(object sender, RoutedEventArgs e)
        {
            switch (Constants.deviceSelect)
            {
                case 1:
                    if (kinectManager != null)
                    {
                        kinectManager.record.StopRecord();
                    }
                    break;
                case 2:
                    break;
                default:
                    ;
                    break;
            }
        }
        #endregion

        #region midi
        
        public void Midi_Click(object sender, RoutedEventArgs e)
        {
            metronomo.PlayMidi();
            Console.WriteLine("midi");
        }

        private void BGMButton_Click(object sender, RoutedEventArgs e)
        {
            metronomo.Start2();
            Console.WriteLine("bgm");
        }

        #endregion

        #region replay
        private void ReplayButton_Click(object sender, RoutedEventArgs e)
        {
            kinectReplay = new KinectReplay(this);
            if(kinectReplay.InitializeReplay() == false) return;
            if (kinectManager != null)
            {
                kinectManager.StopFrameRead();
            }
            kinectReplay.StartReplay();
            StopPlayButton.IsEnabled = true;
        }

        private void StopPlayButton_Checked(object sender, RoutedEventArgs e)
        {
            if (kinectReplay != null)
            {
                kinectReplay.StopReplay();
            }
            StopPlayButton.Content = "▶";
        }

        private void StopPlayButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (kinectReplay != null)
            {
                kinectReplay.StartReplay();
            }
            StopPlayButton.Content = "||";
        }
        #endregion

        private DataViewer dataViewer = null;
        private MotionLearner motionLearner = null;
        string fileName = "";
        private void LearnButton_Click(object sender, RoutedEventArgs e)
        {
            if (motionLearner != null) return;

            fileName = RecordData.OpenFile();
            if (fileName.Length == 1) return;

            Evaluater.Visibility = Visibility.Visible;
            Plot.Visibility = Visibility.Visible;
            SavePanel.Visibility = Visibility.Visible;

            dataViewer = new DataViewer(Plot);
            motionLearner = new MotionLearner(this, dataViewer, fileName);
            MethodBox.SelectedIndex = 4;
            Console.WriteLine("file opened");
        }

        private void MaxSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            float value = (float)e.NewValue;
            MaxValue.Text = value.ToString();
            if (motionLearner != null) motionLearner.ValueChange(1, value);
        }

        private void MinSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            float value = (float)e.NewValue;
            MinValue.Text = value.ToString();
            if (motionLearner != null) motionLearner.ValueChange(2, value);
        }
        
        private StreamWriter saveWriter = null;
        int gestureKind = 0;
        List<ParameterListItem> pList = new List<ParameterListItem>();
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (MethodBox.SelectedValue == null) return;
            var type = MethodBox.SelectedValue.ToString();
            int index = -1;
            if (HeadCheck.IsChecked == true) index = 0;
            if (HandCheck.IsChecked == true) index = 1;
            if (FootCheck.IsChecked == true) index = 2;
            Console.WriteLine(index);
            ParameterListItem p = new ParameterListItem(index, type, MaxSlider.Value, MinSlider.Value);
            ParameterListBox.Items.Add(p);
        }

        private void GestureButton_Click(object sender, RoutedEventArgs e)
        {
            if(GetSaveData() == false) return;
            if (kinectReplay != null)
            {
                kinectReplay.Close();
            }
            if (kinectManager == null)
            {
                Console.WriteLine("try to open kinect");
                kinectManager = new KinectManager(this);
            }
            else
            {
                kinectManager.StartFrameRead();
            }
            KinectManager.GestureIsRecognized = true;
        }

        private bool GetSaveData()
        {
            var dialog = new OpenFileDialog();
            string startupPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Environment.GetCommandLineArgs()[0]));
            string dialogPath = startupPath.Replace("bin\\x64\\Debug", "") + "Data\\Kinect\\setting";
            dialog.InitialDirectory = dialogPath;
            dialog.Title = "ファイルを開く";
            dialog.Filter = "csvファイル(*.*)|*.csv";
            if (dialog.ShowDialog() == true)
            {
                Console.WriteLine("save open");
                using (StreamReader sr = new StreamReader(dialog.FileName))
                {
                    String line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] tokens = line.Split(',');
                        if (tokens[1] == "Velocity")
                        {
                            int joint = int.Parse(tokens[0]);
                            KinectGesture.Vel[joint][0] = double.Parse(tokens[2]);
                            KinectGesture.Vel[joint][1] = double.Parse(tokens[3]);
                        }
                        if (tokens[1] == "AngVel")
                        {
                            int joint = int.Parse(tokens[0]);
                            KinectGesture.AngVel[joint][0] = double.Parse(tokens[2]);
                            KinectGesture.AngVel[joint][1] = double.Parse(tokens[3]);
                        }
                        if (tokens[1] == "Acc")
                        {
                            int joint = int.Parse(tokens[0]);
                            KinectGesture.Acc[joint][0] = double.Parse(tokens[2]);
                            KinectGesture.Acc[joint][1] = double.Parse(tokens[3]);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("open file error");
                return false;
            }
            return true;
        }

        private void GestureBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //GestureButton.IsEnabled = true;
        }

        private void MethodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var box = (ComboBox)e.Source;
            if (motionLearner != null) motionLearner.SelectList(box.SelectedIndex);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ParameterListBox.Items.Count == 0) return;
            if(NameBox.Text == "name")
            {
                MessageBox.Show("enter name");
                return;
            }
            var token = fileName.Split('.');
            Console.WriteLine(fileName);
            string newName = "";
            for (int i = 0; i < token.Length - 1; i++)
            {
                newName += token[i];
                if (i < token.Length - 2) newName += ".";
            }
            fileName = newName + "_" + gestureKind + "_save.csv";

            //DateTime dt = DateTime.Now;
            //string time = dt.Day.ToString() + dt.Hour.ToString() + dt.Minute.ToString() + dt.Second.ToString();
            fileName = RecordData.FolderPath + "setting\\" + NameBox.Text + "_save.csv";
            saveWriter = new StreamWriter(fileName, true);

            var obj = ParameterListBox.Items;
            foreach (var o in obj)
            {
                var p = (ParameterListItem)o;
                saveWriter.WriteLine(p.Joint + "," + p.Type + "," + p.Max + "," + p.Min);
            }
            saveWriter.Close();
            saveWriter = null;

            Evaluater.Visibility = Visibility.Hidden;
            ParameterListBox.Visibility = Visibility.Hidden;
            Plot.Visibility = Visibility.Hidden;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (motionLearner == null) return;
            motionLearner.SelectList(MethodBox.SelectedIndex);
        }

        private void ExperimentButton_Click(object sender, RoutedEventArgs e)
        {
            KinectRecorder.stoptime = Metronomo.UpdateTiming[Metronomo.UpdateTiming.Length - 1] +2000;
            if (kinectManager != null)
            {
                kinectManager.record.StartRecordExperiment();
                metronomo.Start2();
            }
        }

    }//class
}//namespace
