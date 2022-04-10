namespace MidiParser
{
    public enum TextEventType : byte
    {
        Text = 0x01,

        TrackName = 0x03,

        Lyric = 0x05,
    }

    public class TextEvent : MidiEvent
    {
        public TextEventType _TextEventType;
        public string Value;

        public TextEvent(int deltaTicks, double time, TextEventType textEventType, string value)
        : base(deltaTicks, time, MidiEventType.MetaEvent, channel:0)
        {
            _TextEventType = textEventType;
            Value = value;
        }
    }
}
