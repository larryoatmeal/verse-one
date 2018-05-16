﻿//------------------------------------------------------------------------------
// <copyright file="Player.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


//using Sanford.Multimedia.Midi;

using Midi;
using System;
using Commons.Music.Midi;
using System.Linq;

namespace ShapeGame
{
    internal class MIDIMaster
    {
        private OutputDevice outputDevice;
        private int channel = 0;
        private int xCC = 1;
        private int yCC = 7;

        public void SetChannel(int channel)
        {
            this.channel = channel;
        }
        public void Setup()
        {
            //int deviceNum = 0;
            //int numDevices = OutputDevice.DeviceCount;
            //for (int i = 0; i < numDevices; i++)
            //{
            //    var deviceProps = OutputDevice.GetDeviceCapabilities(i);
            //    Console.WriteLine(deviceProps.name);

            //    var deviceName = deviceProps.name;

            //    if (deviceName == "6835")
            //    {

            //        deviceNum = i;
            //        Console.WriteLine("Device 6835 found, Device Number {0}", deviceNum);
            //        Console.WriteLine("Support {0}", deviceProps.support);

            //        break;
            //    }
            //}
            //outputStream = new OutputDevice(0);
            //outputStream.RunningStatusEnabled = true;
            //Console.WriteLine("DEVCE INABLED {0}", outputStream.RunningStatusEnabled);
            //SendProgramChange(deviceNum);
            //outputDevice = OutputDevice.InstalledDevices[0];
            //outputDevice.Open();

            //for(int i = 0; i < OutputDevice.InstalledDevices.Count; i++)
            //{
            //    var dev = OutputDevice.InstalledDevices[i];
            //    Console.WriteLine("DEVICE {0}", dev.Name);
            //}

            ////SendProgramChange(3);
            //outputDevice.SendNoteOn(Midi.Channel.Channel1, Pitch.C3, 127);
            var access = MidiAccessManager.Default;
            var output = access.OpenOutputAsync(access.Outputs.Last().Id).Result;

            Console.WriteLine("NAME {0}", access.Outputs.Last().Name);
            output.Send(new byte[] { MidiEvent.Program, 0x00 }, 0, 2, 0); // Strings Ensemble

            //output.Send(new byte[] { 0xC0, 0x0 }, 0, 2, 0); // Piano
            output.Send(new byte[] { MidiEvent.NoteOn, 0x40, 0x70 }, 0, 3, 0); // There are constant fields for each MIDI event
            output.Send(new byte[] { MidiEvent.NoteOff, 0x40, 0x70 }, 0, 3, 1000);

            output.Send(new byte[] { MidiEvent.Program, 0x01 }, 0, 2, 0); // Strings Ensemble

            //output.Send(new byte[] { 0xC0, 0x0 }, 0, 2, 0); // Piano
            output.Send(new byte[] { MidiEvent.NoteOn, 0x47, 0x70 }, 0, 3, 0); // There are constant fields for each MIDI event
            output.Send(new byte[] { MidiEvent.NoteOff, 0x47, 0x70 }, 0, 3, 1000);
            //output.Send(new byte[] { 0x90, 0x40, 0x70 }, 0, 3, 0);
            //output.Send(new byte[] { 0x80, 0x40, 0x70 }, 0, 3, 0);
            output.CloseAsync();
        }
        public void SendXY(int x, int y)
        {
           
            

            outputDevice.SendControlChange(Midi.Channel.Channel1, Midi.Control.ModulationWheel, x);
            outputDevice.SendControlChange(Midi.Channel.Channel1, Midi.Control.Volume, y);

            //outputDevice.SendNoteOn(Channel.Channel1, Pitch.C4, 80);  // Middle C, velocity 80
            //outputDevice.SendPitchBend(Channel.Channel1, 7000);  // 8192 is centered, so 7000 is bent down


        }
        public void SendProgramChange(int program)
        {

//            output.Send(new byte[] { MidiEvent.Program, 0x00 }, 0, 2, 0); // Strings Ensemble

//            outputDevice.SendProgramChange(Midi.Channel.Channel1, Instrument.AcousticGrandPiano);
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