using System;
using System.IO;
using System.Text;
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
        static MidiFileDomain domain;
        static MidiOutPort port;

        public static void InitMidi()
        {
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
        }

        public static void InitTrack()
        {
            var track = new MidiTrack();
            track.Insert(new TempoEvent() { Tempo = 120, Tick = 0 });
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

        /// <summary> Midiクラスの初期化 </summary>
        public static void InitPlayer()
        {
            // MIDI ファイルを読み込み
            string fname = "../../../Resources/wood.mid";// 0 | 480 1440 | 2400 2880 3360 | 4320 4800 5280 5760 | 6240 …
            if (!File.Exists(fname))
            {
                Console.WriteLine("File does not exist");
                return;
            }
            var midiData = MidiReader.ReadFrom(fname, Encoding.GetEncoding("shift-jis"));

            // テンポマップを作成
            domain = new MidiFileDomain(midiData);

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

        public static void PlayMidi()
        {
            // MIDI ファイルを再生
            player.Play(domain);
        }
    }
}
