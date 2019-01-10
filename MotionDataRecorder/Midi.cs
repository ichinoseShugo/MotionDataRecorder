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

        static int tempo = 120;
        static public int resolution = 240;
        static double sec1Tick;

        static public int[] time = new int[] { 0, 480, 960, 1200, 1440, 1920, 2160, 2400, 2640, 2880, 3120, 3360, 3600, 3840, 4080, 4320, 4560, 4800, 5040, 5280, 5520, 5760, 6000, 6240, 6480, 6720, 6960, 7200, 7440, 7680, 7920, 8160, 8400, 8640, 8880, 9120, 9360 };
        static public int[] mill;

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
            sec1Tick = beat / resolution;
            mill = new int[time.Length];
            for (int i = 0; i < time.Length; i++)
            {
                mill[i] = (int)(sec1Tick * time[i] * 1000);
                Console.WriteLine(mill[i]);
                //Console.WriteLine(mill[i] + ",0");
                //Console.WriteLine(mill[i] + ",1");
                //Console.WriteLine(mill[i] + ",0");
            }
        }
        public static void OnNote(byte note)
        {
            port.Send(new NoteEvent()
            {
                Note = note,
                Gate = 240,
            });
        }

        public static void OnNote(byte note, byte gate)
        {
            port.Send(new NoteEvent()
            {
                Note = note,
                Gate = gate,
            });
        }

        public static void OnNote(byte value, byte note, byte gate)
        {
            port.Send(new ProgramEvent
            {
                Value = value,
            });
            port.Send(new NoteEvent()
            {
                Note = note,
                Gate = gate,
            });
        }

        public static void PlayMidi()
        {
            // MIDI ファイルを再生
            player.Play(domain);
        }

        public static void StopMidi()
        {
            player.Stop();
        }
    }
}
