//------------------------------------------------------------------------------
// <copyright file="SpeechRecognizer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// This module provides sample code used to demonstrate the use
// of the KinectAudioSource for speech recognition in a game setting.

// IMPORTANT: This sample requires the Speech Platform SDK (v11) to be installed on the developer workstation

namespace ShapeGame.Speech
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.Kinect;
    using Microsoft.Speech.AudioFormat;
    using Microsoft.Speech.Recognition;
    using ShapeGame.Utils;

    public class SpeechRecognizer : IDisposable
    {

        private readonly Dictionary<string, WhatSaid> controlPhrases = new Dictionary<string, WhatSaid>
            {
                { "Stop", new WhatSaid { Verb = Verbs.Stop } },
                { "Play", new WhatSaid { Verb = Verbs.Play } },
                { "Cycle", new WhatSaid { Verb = Verbs.LoopOn } },
                { "Pass", new WhatSaid { Verb = Verbs.LoopOff } },
                { "Alpha", new WhatSaid { Verb = Verbs.SetLoopStart } },
                { "Beta", new WhatSaid { Verb = Verbs.SetLoopEnd } },
                { "Forward", new WhatSaid { Verb = Verbs.Forward } },
                { "Backward", new WhatSaid { Verb = Verbs.Backward } },
                { "Lock", new WhatSaid { Verb = Verbs.Lock } },
                { "Control", new WhatSaid { Verb = Verbs.Control } },
                { "Patch One", new WhatSaid { Verb = Verbs.PatchOne } },
                { "Patch Two", new WhatSaid { Verb = Verbs.PatchTwo } },
                { "Patch Three", new WhatSaid { Verb = Verbs.PatchThree } },
                { "Calibrate", new WhatSaid { Verb = Verbs.Calibrate } },
                { "Continue", new WhatSaid { Verb = Verbs.Done } },

            };


        private SpeechRecognitionEngine sre;
        private KinectAudioSource kinectAudioSource;
        private bool paused;
        private bool isDisposed;

        private SpeechRecognizer()
        {
            RecognizerInfo ri = GetKinectRecognizer();
            this.sre = new SpeechRecognitionEngine(ri);
            this.LoadGrammar(this.sre);
        }

        public event EventHandler<SaidSomethingEventArgs> SaidSomething;

        public enum Verbs
        {
            None = 0,
            Pause,
            Resume,
            Stop,
            Play,
            LoopOn,
            LoopOff,
            SetLoopStart,
            SetLoopEnd,
            Forward,
            Backward,
            Lock,
            Control,
            PatchOne,
            PatchTwo,
            PatchThree,
            Done,
            Calibrate
        }

        public EchoCancellationMode EchoCancellationMode
        {
            get
            {
                this.CheckDisposed();

                if (this.kinectAudioSource == null)
                {
                    return EchoCancellationMode.None;
                }

                return this.kinectAudioSource.EchoCancellationMode;
            }

            set
            {
                this.CheckDisposed();

                if (this.kinectAudioSource != null)
                {
                    this.kinectAudioSource.EchoCancellationMode = value;
                }
            }
        }

        // This method exists so that it can be easily called and return safely if the speech prereqs aren't installed.
        // We isolate the try/catch inside this class, and don't impose the need on the caller.
        public static SpeechRecognizer Create()
        {
            SpeechRecognizer recognizer = null;

            try
            {
                recognizer = new SpeechRecognizer();
            }
            catch (Exception)
            {
                // speech prereq isn't installed. a null recognizer will be handled properly by the app.
            }

            return recognizer;
        }

        public void Start(KinectAudioSource kinectSource)
        {
            this.CheckDisposed();

            this.sre.SetInputToDefaultAudioDevice();
            this.sre.RecognizeAsync(RecognizeMode.Multiple);
//            if (kinectSource != null)
//            {
//                this.kinectAudioSource = kinectSource;
//                this.kinectAudioSource.AutomaticGainControlEnabled = false;
//                this.kinectAudioSource.BeamAngleMode = BeamAngleMode.Adaptive;
////                var kinectStream = this.kinectAudioSource.Start();
////                this.sre.SetInputToAudioStream(
////                    kinectStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
//                
//                this.sre.RecognizeAsync(RecognizeMode.Multiple);
//            }
        }

        public void Stop()
        {
            this.CheckDisposed();

            if (this.sre != null)
            {
                if (this.kinectAudioSource != null)
                {
                    this.kinectAudioSource.Stop();
                }

                this.sre.RecognizeAsyncCancel();
                this.sre.RecognizeAsyncStop();

                this.sre.SpeechRecognized -= this.SreSpeechRecognized;
                this.sre.SpeechHypothesized -= this.SreSpeechHypothesized;
                this.sre.SpeechRecognitionRejected -= this.SreSpeechRecognitionRejected;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "sre",
            Justification = "This is suppressed because FXCop does not see our threaded dispose.")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Stop();

                if (this.sre != null)
                {
                    // NOTE: The SpeechRecognitionEngine can take a long time to dispose
                    // so we will dispose it on a background thread
                    ThreadPool.QueueUserWorkItem(
                        delegate(object state)
                            {
                                IDisposable toDispose = state as IDisposable;
                                if (toDispose != null)
                                {
                                    toDispose.Dispose();
                                }
                            },
                            this.sre);
                    this.sre = null;
                }

                this.isDisposed = true;
            }
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        private void CheckDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("SpeechRecognizer");
            }
        }

        private void LoadGrammar(SpeechRecognitionEngine speechRecognitionEngine)
        {
            var controls = new Choices();
            foreach (var phrase in this.controlPhrases)
            {
                controls.Add(phrase.Key);
            }
            var allChoices = new Choices();
            allChoices.Add(controls);
           

            // This is needed to ensure that it will work on machines with any culture, not just en-us.
            var gb = new GrammarBuilder { Culture = speechRecognitionEngine.RecognizerInfo.Culture };
            gb.Append(allChoices);
            gb.AppendWildcard();

            var g = new Grammar(gb);
            speechRecognitionEngine.LoadGrammar(g);
            speechRecognitionEngine.SpeechRecognized += this.SreSpeechRecognized;
            speechRecognitionEngine.SpeechHypothesized += this.SreSpeechHypothesized;
            speechRecognitionEngine.SpeechRecognitionRejected += this.SreSpeechRecognitionRejected;
        }

        private void SreSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            var said = new SaidSomethingEventArgs { Verb = Verbs.None, Matched = "?" };

            if (this.SaidSomething != null)
            {
                this.SaidSomething(new object(), said);
            }

            //Console.WriteLine("\nSpeech Rejected");
        }

        private void SreSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            //Console.Write("\rSpeech Hypothesized: \t{0}", e.Result.Text);
        }

        private void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.Write("\rSpeech Recognized: \t{0}", e.Result.Text);

            if ((this.SaidSomething == null) || (e.Result.Confidence < 0.3))
            {
                return;
            }

            var said = new SaidSomethingEventArgs
                { RgbColor = System.Windows.Media.Color.FromRgb(0, 0, 0), Shape = 0, Verb = 0, Phrase = e.Result.Text };
            bool found = false;
            foreach (var phrase in this.controlPhrases)
            {
                if (e.Result.Text.Contains(phrase.Key))
                {
                    said.RgbColor = phrase.Value.Color;
                    said.Matched = phrase.Key;
                    said.Verb = phrase.Value.Verb;
                    found = true;
                    break;
                }
            }

            if (said.Verb == Verbs.Stop)
            {
                Dictionary<string, string> cmd = new Dictionary<string, string>
                    {
                        { "Command", "pause" },
                    };  
                MainWindow.QUEUE.Push(cmd);
            }

            else if (said.Verb == Verbs.Play)
            {
                Dictionary<string, string> cmd = new Dictionary<string, string>
                    {
                        { "Command", "play" },
                    };
                MainWindow.QUEUE.Push(cmd);
            }

            else if (said.Verb == Verbs.LoopOn)
            {
                Dictionary<string, string> cmd = new Dictionary<string, string>
                    {
                        { "Command", "loopOn" },
                    };
                MainWindow.QUEUE.Push(cmd);
            }

            else if (said.Verb == Verbs.LoopOff)
            {
                Dictionary<string, string> cmd = new Dictionary<string, string>
                    {
                        { "Command", "loopOff" },
                    };
                MainWindow.QUEUE.Push(cmd);
            }

            else if (said.Verb == Verbs.SetLoopStart)
            {
                Dictionary<string, string> cmd = new Dictionary<string, string>
                    {
                        { "Command", "setLoopStart" },
                    };
                MainWindow.QUEUE.Push(cmd);
            }

            else if (said.Verb == Verbs.SetLoopEnd)
            {
                Dictionary<string, string> cmd = new Dictionary<string, string>
                    {
                        { "Command", "setLoopEnd" },
                    };
                MainWindow.QUEUE.Push(cmd);
            }

            else if (said.Verb == Verbs.Calibrate)
            {
                Dictionary<string, string> cmd = new Dictionary<string, string>
                {
                    { "Command", "calibrate" },
                };
                MainWindow.QUEUE.Push(cmd);
                MainWindow.CalibrationStep = 0;
            }

            else if (said.Verb == Verbs.Done)
            {
                Dictionary<string, string> cmd = new Dictionary<string, string>
                {
                    { "Command", "done" },
                };
                MainWindow.QUEUE.Push(cmd);
                MainWindow.CalibrationStep += 1;
            }

            if (!found)
            {
                return;
            }

            if (this.SaidSomething != null)
            {
                this.SaidSomething(new object(), said);
            }
        }
        
        private struct WhatSaid
        {
            public Verbs Verb;
            public PolyType Shape;
            public System.Windows.Media.Color Color;
        }

        public class SaidSomethingEventArgs : EventArgs
        {
            public Verbs Verb { get; set; }

            public PolyType Shape { get; set; }

            public System.Windows.Media.Color RgbColor { get; set; }

            public string Phrase { get; set; }

            public string Matched { get; set; }
        }
    }
}