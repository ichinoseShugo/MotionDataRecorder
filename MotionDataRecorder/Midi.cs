using System;
using System.IO;
using System.Text;
using System.Windows.Media;
using NextMidi.Data;
using NextMidi.Data.Domain;
using NextMidi.Data.Track;
using NextMidi.DataElement;
using NextMidi.DataElement.MetaData;
using NextMidi.Filing.Midi;
using NextMidi.MidiPort.Output;
using NextMidi.Time;

namespace MotionDataRecorder
{
    public class Midi
    {
        static MidiPlayer player;
        static MidiOutPort port;

        static MidiFileDomain domain;
        static MidiData midiData;

        public static MidiFileDomain BGMdomain;
        static MidiData BGMmidiData;

        /// <summary> bpm (1分間あたりの拍数) </summary>
        static int tempo = 120;
        /// <summary> 四分音符のTick </summary>
        static public int resolution = 240;
        /// <summary> 1Tickあたりの秒数 </summary>
        static public double secPerTick;

        static public int[] time = new int[] { 0, 480, 960, 1200, 1440, 1920, 2160, 2400, 2640, 2880, 3120, 3360, 3600, 3840, 4080, 4320, 4560, 4800, 5040, 5280, 5520, 5760, 6000, 6240, 6480, 6720, 6960, 7200, 7440, 7680, 7920, 8160, 8400, 8640, 8880, 9120, 9360 };
        static public int[] mill;
        static public int stoptime;

        public static void InitMidi()
        {
            //InitDomain("../../../Resources/wood.mid");
            InitDomain();
            
            // MIDI ポートを作成
            port = new MidiOutPort(0);
            try
            {
                port.Open();
            }
            catch
            {
                Console.WriteLine("no such port exists");
                return;
            }
            

            // MIDI プレーヤーを作成
            player = new MidiPlayer(port);
        }

        private static void InitDomain()
        {
            var track = new MidiTrack();
            track.Insert(new TempoEvent() { Tempo = 120, Tick = 0 });
            for (int i = 0; i < time.Length; i++)
            {
                byte note = 60;
                if (i > 2 && i % 4 == 1) note = 67;
                if (i > 2 && i % 4 == 2) note = 64;
                track.Insert(new NoteEvent() { Note = note, Tick = time[i], Gate = resolution, Velocity = 64 });
            }
            
            //midiDataにtrackを対応付け
            midiData = new MidiData();
            midiData.Tracks.Add(track);
            midiData.Resolution.Resolution = resolution;

            // テンポマップを作成
            domain = new MidiFileDomain(midiData);
            SetTime();
        }

        static public int[] BGMtime = new int[] {
            0, 480, 960, 1200, 1440,

            1920, 2160, 2400, 2640,
            2880, 3120, 3360, 3600,
            3840, 4080, 4320, 4560,
            4800, 5040, 5280, 5520,

            5760, 6000, 6240, 6480,
            6720, 6960, 7200, 7440,
            7680, 7920, 8160, 8400,
            8640, 8880, 9120, 9360,

            9600, 9840, 10080, 10320,
            10560, 10800, 11040, 11280,
            11520, 11760, 12000, 12240,
            12480, 12720, 12960, 13200,

            13440, 13680, 13920, 14160,
            14400, 14640, 14880, 15120,
            15360, 15600, 15840, 16080,
            16320, 16560, 16800, 17040,
        };

        public static void InitAccompanimentDomain()
        {
            var track = new MidiTrack();
            track.Insert(new TempoEvent() { Tempo = 120, Tick = 0 });
            for (int i = 0; i < 5; i++)
            {
                byte note = 60;
                track.Insert(new NoteEvent() { Note = note, Tick = BGMtime[i], Gate = resolution, Velocity = 64 , Channel = 0});
            }
            for (int i = 5; i < BGMtime.Length; i++)
            {
                int byteIndex = (i-5)/4;
                foreach(byte note in Chord.MidiNumber[byteIndex])
                    track.Insert(new NoteEvent() { Note = note, Tick = BGMtime[i], Gate = resolution, Velocity = 64, Channel = 0});
            }

            for (int i=0; i < Chord.NoteList.Length; i++)
            {
                int length = Chord.MidiNumber[i].Length * 3;
                Chord.NoteList[i] = new byte[length];
                for (int j = 0; j < 3; j++)
                {
                    Chord.NoteList[i][j] = (byte)(Chord.MidiNumber[i][j] - 12);
                    Chord.NoteList[i][j + 3] = Chord.MidiNumber[i][j];
                    Chord.NoteList[i][j + 6] = (byte)(Chord.MidiNumber[i][j] + 12);
                }
            }
            foreach (var notes in Chord.NoteList)
            {
                for(int i=0; i<notes.Length; i++)
                {
                    Console.Write(notes[i] + ", ");
                }
                Console.WriteLine();
            }
            //midiDataにtrackを対応付け
            BGMmidiData = new MidiData();
            BGMmidiData.Tracks.Add(track);
            BGMmidiData.Resolution.Resolution = resolution;

            // テンポマップを作成
            BGMdomain = new MidiFileDomain(BGMmidiData);
        }

        private static void InitDomain(string filename)
        {
            // MIDI ファイルを読み込み
            //string fname = "../../../Resources/wood.mid";// 0 | 480 1440 | 2400 2880 3360 | 4320 4800 5280 5760 | 6240 …
            string fname = filename;
            if (!File.Exists(fname))
            {
                Console.WriteLine("File does not exist");
                return;
            }
            var midiData = MidiReader.ReadFrom(fname, Encoding.GetEncoding("shift-jis"));

            // テンポマップを作成
            domain = new MidiFileDomain(midiData);
        }

        private static void SetTime()
        {
            double beat = 60 / (double)tempo;
            secPerTick = beat / resolution;
            mill = new int[time.Length];
            for (int i = 0; i < time.Length; i++)
            {
                mill[i] = (int)(secPerTick * time[i] * 1000);
                Console.WriteLine(mill[i]);
                //Console.WriteLine(mill[i] + ",0");
                //Console.WriteLine(mill[i] + ",1");
                //Console.WriteLine(mill[i] + ",0");
            }
            stoptime = (int)(mill[mill.Length - 1] + secPerTick * resolution * 1000);
            Console.WriteLine(stoptime);
        }

        public static void OnNote(byte note)
        {
            port.Send(new NoteEvent()
            {
                Note = note,
                Gate = 240,
                Channel = 1,
            });
        }

        public static void OnNote(byte note, byte gate)
        {
            port.Send(new NoteEvent()
            {
                Note = note,
                Gate = gate,
                Channel = 1,
            });
        }

        public static void OnNote(byte value, byte note, byte gate)
        {
            port.Send(new ProgramEvent
            {
                Value = value,
                Channel = 1,
            });
            port.Send(new NoteEvent()
            {
                Note = note,
                Gate = gate,
                Channel = 1,
            });
        }

        public static void PlayMidi()
        {
            // MIDI ファイルを再生
            player.Play(domain);
        }

        public static void PlayMidi(MidiFileDomain d)
        {
            // MIDI ファイルを再生
            player.Play(d);
        }

        public static void StopMidi()
        {
            player.Stop();
        }
    }
}
