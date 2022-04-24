using UnityEngine;

namespace MidiParser
{
    public enum MetaEventType : byte
    {
        Tempo = 0x51,

        TimeSignature = 0x58,

        KeySignature = 0x59
    }

    public class MetaEvent : MidiEvent
    {
        public MetaEvent(int deltaTicks, double time, MetaEventType metaType)
            : base(deltaTicks, time, MidiEventType.MetaEvent, 0)
        {
            _MetaEventType = metaType;
        }

        public MetaEventType _MetaEventType;
    }

    public class MetaTempoEvent : MetaEvent {
        public int Tempo;

        public MetaTempoEvent(int deltaTicks, double time, int tempo)
            : base(deltaTicks, time, MetaEventType.Tempo)
        {
            Tempo = tempo;

            Debug.Log( $"Tempo BPM: {this.BPM}");
        }

        public double BPM => 60000000.0 / Tempo;
    }

    // https://www.recordingblogs.com/wiki/midi-time-signature-meta-message
    public class MetaTimeSignatureEvent : MetaEvent {
        public int Numerator;
        public int Denominator;
        public int Metronome;
        public int ThirtySeconds;

        public MetaTimeSignatureEvent(int deltaTicks, double time, int numerator, int denominator, int metronome, int thirtySeconds)
            : base(deltaTicks, time, MetaEventType.TimeSignature)
        {
            Numerator = numerator;
            Denominator = denominator;
            Metronome = metronome;
            ThirtySeconds = thirtySeconds;
            Debug.Log( $"Time Sig: {Numerator}/{Denominator} {Metronome}:{ThirtySeconds}");
        }
    }

    public class MetaKeySignatureEvent : MetaEvent {
        public int Key;
        public int Scale;

        public MetaKeySignatureEvent(int deltaTicks, double time, int key, int scale)
            : base(deltaTicks, time, MetaEventType.KeySignature)
        {
            Key = key;
            Scale = scale;
        }
    }
}
