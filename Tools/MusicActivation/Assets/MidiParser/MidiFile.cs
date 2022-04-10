using System;
using System.IO;
using UnityEngine;

namespace MidiParser
{
    public class MidiFile
    {
        public readonly int Format;

        public readonly int TicksPerQuarterNote;
        public int usPerQuarter = 500000; // Equivalent to 120 BPM
        double SecondsPerTick => (double)usPerQuarter / (double)TicksPerQuarterNote / 1000000.0;

        public readonly MidiTrack[] Tracks;

        public MidiFile(Stream stream)
            : this(Reader.ReadAllBytesFromStream(stream))
        {
        }

        public MidiFile(string path)
            : this(File.ReadAllBytes(path))
        {
        }

        public MidiFile(byte[] data)
        {
            var position = 0;

            if (Reader.ReadString(data, ref position, 4) != "MThd")
            {
                throw new FormatException("Invalid file header (expected MThd)");
            }

            if (Reader.Read32(data, ref position) != 6)
            {
                throw new FormatException("Invalid header length (expected 6)");
            }

            this.Format = Reader.Read16(data, ref position);
            var TracksCount = Reader.Read16(data, ref position);
            this.TicksPerQuarterNote = Reader.Read16(data, ref position);

            if ((this.TicksPerQuarterNote & 0x8000) != 0)
            {
                throw new FormatException("Invalid timing mode (SMPTE timecode not supported)");
            }

            this.Tracks = new MidiTrack[TracksCount];

            for (var i = 0; i < TracksCount; i++)
            {
                this.Tracks[i] = ParseTrack(i, data, ref position);
            }
        }

        private MidiTrack ParseTrack(int index, byte[] data, ref int position)
        {
            if (Reader.ReadString(data, ref position, 4) != "MTrk")
            {
                throw new FormatException("Invalid track header (expected MTrk)");
            }

            var trackLength = Reader.Read32(data, ref position);
            var trackEnd = position + trackLength;

            var track = new MidiTrack { Index = index };
            var status = (byte)0;

            // TicksPerQuarterNote = <PPQ from the header>
            // usPerQuarter = <Tempo in latest Set Tempo event>
            // usPerTick = usPerQuarter / TicksPerQuarterNote
            // secondsPerTick = usPerTick / 1,000,000
            // seconds = ticks * secondsPerTick

            double seconds=0;

            while (position < trackEnd)
            {
                var deltaTicks = Reader.ReadVarInt(data, ref position);
                seconds += SecondsPerTick * deltaTicks;

                var peekByte = data[position];

                // If the most significant bit is set then this is a status byte
                if ((peekByte & 0x80) != 0)
                {
                    status = peekByte;
                    ++position;
                }

                if ((status & 0xF0) != 0xF0) // If the most significant nibble is not an 0xF this is a channel event
                {
                    // Separate event type from channel into two
                    var eventType = (byte)(status & 0xF0);
                    var channel = (byte)((status & 0x0F) + 1);

                    var data1 = data[position++];

                    // If the event type doesn't start with 0b110 it has two bytes of data (i.e. except 0xC0 and 0xD0)
                    var data2 = (eventType & 0xE0) != 0xC0 ? data[position++] : (byte)0;

                    // Convert NoteOn events with 0 velocity into NoteOff events
                    if (eventType == (byte)MidiEventType.NoteOn && data2 == 0)
                    {
                        eventType = (byte)MidiEventType.NoteOff;
                    }

                    MidiEvent midiEvent = null;
                    // Debug.Log(string.Format("{0} {1} ch:{2}", seconds, ((MidiEventType)eventType).ToString(), channel));
                    switch( eventType ) {
                    case (byte)MidiEventType.NoteOn:
                    case (byte)MidiEventType.KeyAfterTouch:
                        midiEvent = new MidiNoteEvent( deltaTicks, seconds, (MidiEventType)eventType, channel, (int)data1, true, (int)data2);
                        break;
                    case (byte)MidiEventType.NoteOff:
                        midiEvent = new MidiNoteEvent( deltaTicks, seconds, MidiEventType.NoteOff, channel, (int)data1, false, (int)data2);
                        break;
                    
                    case (byte)MidiEventType.ControlChange:
                        midiEvent = new MidiControlChangeEvent( deltaTicks, seconds, MidiEventType.ControlChange, channel, (int)data1, (int)data2);
                        break;
                    }

                    if( midiEvent != null ) {
                        track.MidiEvents.Add( midiEvent );
                    }
                }
                else
                {
                    if (status == 0xFF) { // Meta Event
                        var metaEventType = Reader.Read8(data, ref position);

                        // There is a group of meta event types reserved for text events which we store separately
                        if (metaEventType >= 0x01 && metaEventType <= 0x0F) {
                            var textLength = Reader.ReadVarInt(data, ref position);
                            var textValue = Reader.ReadString(data, ref position, textLength);
                            var textEvent = new TextEvent( deltaTicks, seconds, (TextEventType)metaEventType, textValue );
                            track.TextEvents.Add(textEvent);
                        }
                        else
                        {
                            // We only handle the few meta events we care about and skip the rest
                            MetaEvent metaEvent = null;
                            switch (metaEventType) {
                                case (byte)MetaEventType.Tempo:
                                    var mspqn = (data[position + 1] << 16) | (data[position + 2] << 8) | data[position + 3];
                                    var tempoEvent = new MetaTempoEvent( deltaTicks, seconds, mspqn );
                                    metaEvent = tempoEvent;
                                    usPerQuarter = tempoEvent.Tempo;
                                    position += 4;
                                    break;

                                case (byte)MetaEventType.TimeSignature: {
                                    var numerator = data[position + 1];
                                    var denominator = (byte)Math.Pow(2.0, data[position + 2]);
                                    var metronome = data[position + 3];
                                    var thirtySecondNotes = data[position + 4];

                                    metaEvent = new MetaTimeSignatureEvent( deltaTicks, seconds, numerator, denominator, metronome, thirtySecondNotes );
                                    position += 5;
                                    break;
                                }

                                case (byte)MetaEventType.KeySignature: {
                                    var key = data[position + 1];
                                    var scale = data[position + 2];
                                    metaEvent = new MetaKeySignatureEvent( deltaTicks, seconds, key, scale );
                                    position += 3;
                                    break;
                                }

                                // Ignore Other Meta Events
                                default:
                                    var length = Reader.ReadVarInt(data, ref position);
                                    position += length;
                                    break;
                            }

                            if( metaEvent != null ) {
                                track.MidiEvents.Add( metaEvent );
                            }
                        }
                    }
                    else 
                    if (status == 0xF0 || status == 0xF7) // SysEx event (skip)
                    {
                        var length = Reader.ReadVarInt(data, ref position);
                        position += length;
                    }
                    else
                    {
                        ++position;
                    }
                }
            }

            return track;
        }
    }
}
