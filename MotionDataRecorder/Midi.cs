using System;
using System.IO;
using System.Text;
using NextMidi.Data.Domain;
using NextMidi.DataElement;
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

        /// <summary> Midiクラスの初期化 </summary>
        public static void InitMidi()
        {
            // MIDI ファイルを読み込み
            string fname = "../../../Resources/wood.mid";
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

        public static void PlayMidi()
        {
            // MIDI ファイルを再生
            player.Play(domain);
        }
    }
}
