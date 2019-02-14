using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionDataRecorder
{
    static public class Chord
    {
        static public string[] inputedChord = { "C", "Am", "F", "G", "Em", "F", "G", "C", "C", "Am", "F", "G", "Em", "F", "G", "C" };

        static public byte[][] MidiNumber = new byte[][]
        {
            new byte[]{ 60, 64, 67 }, // C
            new byte[]{ 57, 60, 64 }, // Am
            new byte[]{ 65, 69, 72 }, // F
            new byte[]{ 55, 59, 62 }, // G

            new byte[]{ 64, 67, 71 }, // Em
            new byte[]{ 65, 69, 72 }, // F
            new byte[]{ 55, 59, 62 }, // G
            new byte[]{ 60, 64, 67 }, // C

            new byte[]{ 60, 64, 67 }, // C
            new byte[]{ 57, 60, 64 }, // Am
            new byte[]{ 65, 69, 72 }, // F
            new byte[]{ 55, 59, 62 }, // G

            new byte[]{ 64, 67, 71 }, // Em
            new byte[]{ 65, 69, 72 }, // F
            new byte[]{ 55, 59, 62 }, // G
            new byte[]{ 60, 64, 67 }, // C
        };

        static public int NoteIndex = 0;
        static public byte[][] NoteList = new byte[MidiNumber.Length][];
    }
}
