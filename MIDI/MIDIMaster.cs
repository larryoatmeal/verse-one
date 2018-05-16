//------------------------------------------------------------------------------
// <copyright file="Player.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


//using Sanford.Multimedia.Midi;

using System.Linq;
using Commons.Music.Midi;
using Midi;

namespace ShapeGame.MIDI
{
    internal class MIDIMaster
    {
        private OutputDevice outputDevice;
        private int channel = 0;
        private int xCC = 1;
        private int yCC = 7;
        private IMidiOutput _midiOutput;

        public void SetChannel(int channel)
        {
            this.channel = channel;
        }
        public void Setup()
        {
            var access = MidiAccessManager.Default;
            _midiOutput = access.OpenOutputAsync(access.Outputs.Last().Id).Result;
//            Console.WriteLine("NAME {0}", access.Outputs.Last().Name);
//            _midiOutput.Send(new byte[] { MidiEvent.Program, 0x00 }, 0, 2, 0); // Strings Ensemble
//
//            //output.Send(new byte[] { 0xC0, 0x0 }, 0, 2, 0); // Piano
//            _midiOutput.Send(new byte[] { MidiEvent.NoteOn, 0x40, 0x70 }, 0, 3, 0); // There are constant fields for each MIDI event
//            _midiOutput.Send(new byte[] { MidiEvent.NoteOff, 0x40, 0x70 }, 0, 3, 1000);
//
//            _midiOutput.Send(new byte[] { MidiEvent.Program, 0x01 }, 0, 2, 0); // Strings Ensemble
//
//            //output.Send(new byte[] { 0xC0, 0x0 }, 0, 2, 0); // Piano
//            _midiOutput.Send(new byte[] { MidiEvent.NoteOn, 0x47, 0x70 }, 0, 3, 0); // There are constant fields for each MIDI event
//            _midiOutput.Send(new byte[] { MidiEvent.NoteOff, 0x47, 0x70 }, 0, 3, 1000);
//            //output.Send(new byte[] { 0x90, 0x40, 0x70 }, 0, 3, 0);
//            //output.Send(new byte[] { 0x80, 0x40, 0x70 }, 0, 3, 0);
//            _midiOutput.CloseAsync();
        }
        public void SendXY(byte x, byte y)
        {
            _midiOutput.Send(new byte[] { MidiEvent.CC, 0x01, x}, 0, 3, 0);
            _midiOutput.Send(new byte[] { MidiEvent.CC, 0x07, y }, 0, 3, 0);
            //           
            //           
            //            outputDevice.SendControlChange(Midi.Channel.Channel1, Midi.Control.ModulationWheel, x);
            //            outputDevice.SendControlChange(Midi.Channel.Channel1, Midi.Control.Volume, y);

            //outputDevice.SendNoteOn(Channel.Channel1, Pitch.C4, 80);  // Middle C, velocity 80
            //outputDevice.SendPitchBend(Channel.Channel1, 7000);  // 8192 is centered, so 7000 is bent down


        }
        public void SendProgramChange(byte program)
        {
            _midiOutput.Send(new byte[] { MidiEvent.Program, program }, 0, 2, 0);
        }

        ~MIDIMaster()
        {
            _midiOutput.CloseAsync();
        }


        public void Test()
        {
            //ChannelMessageBuilder builder = new ChannelMessageBuilder();
            //builder.Command = ChannelCommand.NoteOn;
            //builder.MidiChannel = 0;
            //builder.Data1 = 60;
            //builder.Data2 = 127;
            //builder.Build();
            //Console.WriteLine(builder.Result);
            //OutputStream outputStream = new OutputStream();
        }

    }
}