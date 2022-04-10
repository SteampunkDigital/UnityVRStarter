using System.Collections.Generic;

namespace MidiParser
{
    public class MidiTrack
    {
        public int Index;

        public List<MidiEvent> MidiEvents = new List<MidiEvent>();

        public List<TextEvent> TextEvents = new List<TextEvent>();
    }
}