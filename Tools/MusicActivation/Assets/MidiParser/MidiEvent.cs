namespace MidiParser
{
    public enum MidiEventType : byte
    {
        NoteOff = 0x80,

        NoteOn = 0x90,

        KeyAfterTouch = 0xA0,

        ControlChange = 0xB0,

        ProgramChange = 0xC0,

        ChannelAfterTouch = 0xD0,

        PitchBendChange = 0xE0,

        MetaEvent = 0xFF
    }

    public enum ControlChangeType : byte
    {
        BankSelect = 0x00,

        Modulation = 0x01,

        Volume = 0x07,

        Balance = 0x08,

        Pan = 0x0A,

        Sustain = 0x40
    }

    //--------------------------------------------------------------------------

    public class MidiEvent
    {
        public MidiEvent( int deltaTicks, double time, MidiEventType eventType, int channel, byte arg2=0, byte arg3=0)
        {
            DeltaTicks = deltaTicks;
            Time = time;
            _MidiEventType = eventType;
            Channel = channel;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        public int DeltaTicks;

        public double Time;

        public MidiEventType _MidiEventType;

        public int Channel;

        public byte Arg2;

        public byte Arg3;

        // public MidiEventType MidiEventType => (MidiEventType)this.Type;

        // public MetaEventType MetaEventType => (MetaEventType)this.Arg1;

        // public int Note => this.Arg2;

        // public int Velocity => this.Arg3;

        // public ControlChangeType ControlChangeType => (ControlChangeType)this.Arg2;

        // public int Value => this.Arg3;
    }

    public class MidiNoteEvent : MidiEvent {
        public int Note;
        public bool On;
        public int Velocity;

        public MidiNoteEvent(int deltaTicks, double time, MidiEventType eventType, int channel, int note, bool on, int velocity)
         : base(deltaTicks, time, eventType, channel)
        {
            Note = note;
            On = on;
            Velocity = velocity;
        }
    }

    public class MidiControlChangeEvent : MidiEvent {
        public int Controller;
        public int Value;

        public MidiControlChangeEvent(int deltaTicks, double time, MidiEventType eventType, int channel, int controller, int value)
         : base(deltaTicks, time, eventType, channel)
        {
            Controller = controller;
            Value = value;
        }
    }
}
