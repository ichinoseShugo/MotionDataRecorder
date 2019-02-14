using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionDataRecorder
{
    static public class RecordData
    {
        static public string FilePath = "";
        static public string FolderPath = "";
        static public string FileName = "";

        static public string OpenFile()
        {
            var dialog = new OpenFileDialog();
            string startupPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Environment.GetCommandLineArgs()[0]));
            string dialogPath = startupPath.Replace("bin\\x64\\Debug", "") + "Data\\Kinect";
            dialog.InitialDirectory = dialogPath;
            dialog.Title = "ファイルを開く";
            dialog.Filter = "csvファイル(*.*)|*.csv";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
                var tokens = FilePath.Split('\\');
                Console.WriteLine(FilePath);
                for(int i=0; i<9; i++)
                {
                    FolderPath += tokens[i] + "\\";
                }
                FileName = tokens[tokens.Length-1].Split('.')[0];
                Console.WriteLine(FileName);
                return dialog.FileName;
            }
            else
            {
                Console.WriteLine("open file error");
                return "E";
            }
        }
    }
}
