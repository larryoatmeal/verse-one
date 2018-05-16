//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// This module contains code to do Kinect NUI initialization,
// processing, displaying players on screen, and sending updated player
// positions to the game portion for hit testing.

using ShapeGame.Gestures;

namespace ShapeGame
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Media;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Threading;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Samples.Kinect.WpfViewers;
    using ShapeGame.Speech;
    using ShapeGame.Utils;
    using System.Net;
    using System.Text;
    using WebServer;
    using Newtonsoft.Json;
    using System.Diagnostics;
    
   
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty KinectSensorManagerProperty =
            DependencyProperty.Register(
                "KinectSensorManager",
                typeof(KinectSensorManager),
                typeof(MainWindow),
                new PropertyMetadata(null));

        #region Private State
        private const int TimerResolution = 2;  // ms
        private const int NumIntraFrames = 3;
        private const int MaxShapes = 80;
        private const double MaxFramerate = 70;
        private const double MinFramerate = 15;
        private const string CmdTogglePlay = "togglePlay";
        private const string CmdSetloopend = "setLoopEnd";
        private const string CmdToggleloop = "toggleLoop";
        private const string CmdSetloopstart = "setLoopStart";
        private const string CmdForward = "forward";
        private const string CmdReverse = "reverse";

        private readonly Dictionary<int, Player> players = new Dictionary<int, Player>();
        private readonly SoundPlayer popSound = new SoundPlayer();
        private readonly SoundPlayer hitSound = new SoundPlayer();
        private readonly SoundPlayer squeezeSound = new SoundPlayer();
        private static readonly SoundPlayer bellSound = new SoundPlayer();
        private static readonly SoundPlayer scratchSound = new SoundPlayer();

        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        private DateTime lastFrameDrawn = DateTime.MinValue;
        private DateTime predNextFrame = DateTime.MinValue;
        private double actualFrameTime;

        private Skeleton[] skeletonData;

        // Player(s) placement in scene (z collapsed):
        private Rect playerBounds;
        private Rect screenRect;

        private double targetFramerate = MaxFramerate;
        private int frameCount;
        private bool runningGameThread;
//        private FallingThings myFallingThings;
        private int playersAlive;

        private SpeechRecognizer mySpeechRecognizer;
        static CrossGesture crossGesture = new CrossGesture();
        static RaiseLeftHandGesture raiseLeftHandGesture = new RaiseLeftHandGesture();
        static RaiseRightHandGesture raiseRightHandGesture = new RaiseRightHandGesture();
        static WaveGesture waveGesture = new WaveGesture();
        static MoveBackGesture moveBackGesture = new MoveBackGesture();
        static MoveForwardGesture moveForwardGesture = new MoveForwardGesture();

        public static TimedQueue<Dictionary<string, string>> QUEUE;
        public static TimedQueue<JointCollection> jointQueue;

        public static Dictionary<Gesture, string> gestureMap = new Dictionary<Gesture, string>()
        {
            {crossGesture, CmdTogglePlay},
            {raiseLeftHandGesture, CmdSetloopstart },
            {raiseRightHandGesture, CmdSetloopend },
            {waveGesture, CmdToggleloop },
            {moveForwardGesture, CmdForward },
            {moveBackGesture, CmdReverse }
        };

        private float jointWindowSize = 3;
        private static float messageWindowSize = 2;
        public static Stopwatch stopWatch = new Stopwatch();
        private static int ID = 0;
        private float threshold = 0.9f;

        MIDIMaster midiMaster;
        #endregion Private State

        #region ctor + Window Events

        public MainWindow()
        {
            midiMaster = new MIDIMaster();
            midiMaster.Setup();

            this.KinectSensorManager = new KinectSensorManager();
            this.KinectSensorManager.KinectSensorChanged += this.KinectSensorChanged;
            this.DataContext = this.KinectSensorManager;

            InitializeComponent();
            StartServer();
            

            this.SensorChooserUI.KinectSensorChooser = sensorChooser;
            sensorChooser.Start();

            // Bind the KinectSensor from the sensorChooser to the KinectSensor on the KinectSensorManager
            var kinectSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.KinectSensorManager, KinectSensorManager.KinectSensorProperty, kinectSensorBinding);

            this.RestoreWindowState();
            jointQueue = new TimedQueue<JointCollection>(jointWindowSize);

            stopWatch.Start();

            CalibrationButton.Click += CalibrationButtonClicked;
        }


        private void CalibrationButtonClicked(object sender, RoutedEventArgs e)
        {
            CalibrationWindow c = new CalibrationWindow();
            this.Content = c;
        }

        public static string SendResponse(HttpListenerRequest request)
        {
            string json = JsonConvert.SerializeObject(QUEUE, Formatting.Indented);
            return json;
        }

        private static void StartServer()
        {
            QUEUE = new TimedQueue<Dictionary<string, string>>(messageWindowSize);

            var ws = new WebServer(SendResponse, "http://localhost:8080/test/");
            ws.Run();
            Console.WriteLine("A simple webserver.");
        }

        public KinectSensorManager KinectSensorManager
        {
            get { return (KinectSensorManager)GetValue(KinectSensorManagerProperty); }
            set { SetValue(KinectSensorManagerProperty, value); }
        }

        // Since the timer resolution defaults to about 10ms precisely, we need to
        // increase the resolution to get framerates above between 50fps with any
        // consistency.
        [DllImport("Winmm.dll", EntryPoint = "timeBeginPeriod")]
        private static extern int TimeBeginPeriod(uint period);

        private void RestoreWindowState()
        {
            // Restore window state to that last used
            Rect bounds = Properties.Settings.Default.PrevWinPosition;
            if (bounds.Right != bounds.Left)
            {
                this.Top = bounds.Top;
                this.Left = bounds.Left;
                this.Height = bounds.Height;
                this.Width = bounds.Width;
            }

            this.WindowState = (WindowState)Properties.Settings.Default.WindowState;
        }

        private void WindowLoaded(object sender, EventArgs e)
        {
            playfield.ClipToBounds = true;

            this.UpdatePlayfieldSize();

            this.popSound.Stream = Properties.Resources.Pop_5;
            this.hitSound.Stream = Properties.Resources.Hit_2;
            this.squeezeSound.Stream = Properties.Resources.Squeeze;
            bellSound.Stream = Properties.Resources.Bell;
            scratchSound.Stream = Properties.Resources.scratch;


            TimeBeginPeriod(TimerResolution);
            var myGameThread = new Thread(this.GameThread);
            myGameThread.SetApartmentState(ApartmentState.STA);
            myGameThread.Start();
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            sensorChooser.Stop();

            this.runningGameThread = false;
            Properties.Settings.Default.PrevWinPosition = this.RestoreBounds;
            Properties.Settings.Default.WindowState = (int)this.WindowState;
            Properties.Settings.Default.Save();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            this.KinectSensorManager.KinectSensor = null;
        }

        #endregion ctor + Window Events

        #region Kinect discovery + setup

        private void KinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {
            if (null != args.OldValue)
            {
                this.UninitializeKinectServices(args.OldValue);
            }

            // Only enable this checkbox if we have a sensor
            enableAec.IsEnabled = null != args.NewValue;

            if (null != args.NewValue)
            {
                this.InitializeKinectServices(this.KinectSensorManager, args.NewValue);
            }
        }

        // Kinect enabled apps should customize which Kinect services it initializes here.
        private void InitializeKinectServices(KinectSensorManager kinectSensorManager, KinectSensor sensor)
        {
            // Application should enable all streams first.
            kinectSensorManager.ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
            kinectSensorManager.ColorStreamEnabled = true;

            sensor.SkeletonFrameReady += this.SkeletonsReady;
            kinectSensorManager.TransformSmoothParameters = new TransformSmoothParameters
                                             {
                                                 Smoothing = 0.5f,
                                                 Correction = 0.5f,
                                                 Prediction = 0.5f,
                                                 JitterRadius = 0.05f,
                                                 MaxDeviationRadius = 0.04f
                                             };
            kinectSensorManager.SkeletonStreamEnabled = true;
            kinectSensorManager.KinectSensorEnabled = true;

            foreach (var keyValuePair in gestureMap)
            {
                var gesture = keyValuePair.Key;
                var cmd = keyValuePair.Value;
                gesture.AddListener((s, e) => SendCommand(cmd));
            }

            if (!kinectSensorManager.KinectSensorAppConflict)
            {
                // Start speech recognizer after KinectSensor started successfully.
                this.mySpeechRecognizer = SpeechRecognizer.Create();

                if (null != this.mySpeechRecognizer)
                {
                    this.mySpeechRecognizer.SaidSomething += this.RecognizerSaidSomething;
                    this.mySpeechRecognizer.Start(sensor.AudioSource);
                }

                enableAec.Visibility = Visibility.Visible;
                this.UpdateEchoCancellation(this.enableAec);
            }
        }

        // Kinect enabled apps should uninitialize all Kinect services that were initialized in InitializeKinectServices() here.
        private void UninitializeKinectServices(KinectSensor sensor)
        {
            sensor.SkeletonFrameReady -= this.SkeletonsReady;

            if (null != this.mySpeechRecognizer)
            {
                this.mySpeechRecognizer.Stop();
                this.mySpeechRecognizer.SaidSomething -= this.RecognizerSaidSomething;
                this.mySpeechRecognizer.Dispose();
                this.mySpeechRecognizer = null;
            }

            enableAec.Visibility = Visibility.Collapsed;
        }

        #endregion Kinect discovery + setup

        #region Kinect Skeleton processing
        private void SkeletonsReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    int skeletonSlot = 0;

                    if ((this.skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);

                    foreach (Skeleton skeleton in this.skeletonData)
                    {
                        if (SkeletonTrackingState.Tracked == skeleton.TrackingState)
                        {
                            Player player;
                            if (this.players.ContainsKey(skeletonSlot))
                            {
                                player = this.players[skeletonSlot];
                            }
                            else
                            {
                                player = new Player(skeletonSlot);
                                player.SetBounds(this.playerBounds);
                                this.players.Add(skeletonSlot, player);
                            }

                            player.LastUpdated = DateTime.Now;

                            // Update player's bone and joint positions
                            if (skeleton.Joints.Count > 0)
                            {
                                player.IsAlive = true;

                                crossGesture.Update(skeleton);
                                raiseRightHandGesture.Update(skeleton);
                                raiseLeftHandGesture.Update(skeleton);
                                waveGesture.Update(skeleton);
                                moveBackGesture.Update(skeleton);
                                moveForwardGesture.Update(skeleton);

                                //player.DetectGesture(skeleton.Joints);
                                // Head, hands, feet (hit testing happens in order here)
                                player.UpdateJointPosition(skeleton.Joints, JointType.Head);
                                player.UpdateJointPosition(skeleton.Joints, JointType.HandLeft);
                                player.UpdateJointPosition(skeleton.Joints, JointType.HandRight);
                                player.UpdateJointPosition(skeleton.Joints, JointType.FootLeft);
                                player.UpdateJointPosition(skeleton.Joints, JointType.FootRight);

                                // Hands and arms
                                player.UpdateBonePosition(skeleton.Joints, JointType.HandRight, JointType.WristRight);
                                player.UpdateBonePosition(skeleton.Joints, JointType.WristRight, JointType.ElbowRight);
                                player.UpdateBonePosition(skeleton.Joints, JointType.ElbowRight, JointType.ShoulderRight);

                                player.UpdateBonePosition(skeleton.Joints, JointType.HandLeft, JointType.WristLeft);
                                player.UpdateBonePosition(skeleton.Joints, JointType.WristLeft, JointType.ElbowLeft);
                                player.UpdateBonePosition(skeleton.Joints, JointType.ElbowLeft, JointType.ShoulderLeft);

                                // Head and Shoulders
                                player.UpdateBonePosition(skeleton.Joints, JointType.ShoulderCenter, JointType.Head);
                                player.UpdateBonePosition(skeleton.Joints, JointType.ShoulderLeft, JointType.ShoulderCenter);
                                player.UpdateBonePosition(skeleton.Joints, JointType.ShoulderCenter, JointType.ShoulderRight);

                                // Legs
                                player.UpdateBonePosition(skeleton.Joints, JointType.HipLeft, JointType.KneeLeft);
                                player.UpdateBonePosition(skeleton.Joints, JointType.KneeLeft, JointType.AnkleLeft);
                                player.UpdateBonePosition(skeleton.Joints, JointType.AnkleLeft, JointType.FootLeft);

                                player.UpdateBonePosition(skeleton.Joints, JointType.HipRight, JointType.KneeRight);
                                player.UpdateBonePosition(skeleton.Joints, JointType.KneeRight, JointType.AnkleRight);
                                player.UpdateBonePosition(skeleton.Joints, JointType.AnkleRight, JointType.FootRight);

                                player.UpdateBonePosition(skeleton.Joints, JointType.HipLeft, JointType.HipCenter);
                                player.UpdateBonePosition(skeleton.Joints, JointType.HipCenter, JointType.HipRight);

                                // Spine
                                player.UpdateBonePosition(skeleton.Joints, JointType.HipCenter, JointType.ShoulderCenter);
                            }

                        }

                        skeletonSlot++;
                    }
                }


            }
        }

        static void SendCommand(string command)
        {
            Console.WriteLine(command);
            bellSound.Play();

            Dictionary<string, string> cmd = new Dictionary<string, string>
            {
                { "Command", command}
            };
            MainWindow.QUEUE.Push(cmd);

 
        }
//        static void CrossGestureRecognized(object sender, EventArgs e)
//        {
//            SendCommand(CmdTogglePlay, "CROSS GESTURE RECOGNIZED");
//        }
//
//        static void RaiseRightHandGestureRecognized(object sender, EventArgs e)
//        {
//            SendCommand(CmdSetloopend, "RIGHT HAND RAISED RECOGNIZED");
//        }
//
//        static void WaveGestureRecognized(object sender, EventArgs e)
//        {
//            SendCommand(CmdToggleloop, "WAVE GESTURE RECOGNIZED");
//        }
//
//        static void RaiseLeftHandGestureRecognized(object sender, EventArgs e)
//        {
//            SendCommand(CmdSetloopstart, "LEFT HAND RAISED RECOGNIZED");
//        }
//
//        static void MoveBackGestureRecognized(object sender, EventArgs e)
//        {
//            SendCommand(CmdForward, "SEEK BACKWARDS GESTURE RECOGNIZED");
//        }
//
//        static void MoveForwardGestureRecognized(object sender, EventArgs e)
//        {
//            SendCommand(CmdReverse, "SEEK FORWARD GESTURE RECOGNIZED");
//        }

        static void ResetAllGestures()
        {
            raiseLeftHandGesture.Reset();
            raiseRightHandGesture.Reset();
            crossGesture.Reset();
            waveGesture.Reset();
            moveBackGesture.Reset();
            moveForwardGesture.Reset();
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // ... Get control that raised this event.
            var textBox = sender as TextBox;
            //this.threshold = textBox.Text;
        }

        private void CheckPlayers()
        {
            foreach (var player in this.players)
            {
                if (!player.Value.IsAlive)
                {
                    // Player left scene since we aren't tracking it anymore, so remove from dictionary
                    this.players.Remove(player.Value.GetId());
                    break;
                }
            }

            // Count alive players
            int alive = this.players.Count(player => player.Value.IsAlive);

            if (alive != this.playersAlive)
            {

                if ((this.playersAlive == 0) && (this.mySpeechRecognizer != null))
                {
                    BannerText.NewBanner(
                        Properties.Resources.Vocabulary,
                        this.screenRect,
                        true,
                        System.Windows.Media.Color.FromArgb(200, 255, 255, 255));
                }

                this.playersAlive = alive;
            }
        }

        private void PlayfieldSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdatePlayfieldSize();
        }

        private void UpdatePlayfieldSize()
        {
            // Size of player wrt size of playfield, putting ourselves low on the screen.
            this.screenRect.X = 0;
            this.screenRect.Y = 0;
            this.screenRect.Width = this.playfield.ActualWidth;
            this.screenRect.Height = this.playfield.ActualHeight;

            BannerText.UpdateBounds(this.screenRect);

            this.playerBounds.X = 0;
            this.playerBounds.Width = this.playfield.ActualWidth;
            this.playerBounds.Y = this.playfield.ActualHeight * 0.2;
            this.playerBounds.Height = this.playfield.ActualHeight * 0.75;

            foreach (var player in this.players)
            {
                player.Value.SetBounds(this.playerBounds);
            }

            Rect fallingBounds = this.playerBounds;
            fallingBounds.Y = 0;
            fallingBounds.Height = playfield.ActualHeight;
 
        }
        #endregion Kinect Skeleton processing

        #region GameTimer/Thread
        private void GameThread()
        {
            this.runningGameThread = true;
            this.predNextFrame = DateTime.Now;
            this.actualFrameTime = 1000.0 / this.targetFramerate;

            // Try to dispatch at as constant of a framerate as possible by sleeping just enough since
            // the last time we dispatched.
            while (this.runningGameThread)
            {
                // Calculate average framerate.  
                DateTime now = DateTime.Now;
                if (this.lastFrameDrawn == DateTime.MinValue)
                {
                    this.lastFrameDrawn = now;
                }

                double ms = now.Subtract(this.lastFrameDrawn).TotalMilliseconds;
                this.actualFrameTime = (this.actualFrameTime * 0.95) + (0.05 * ms);
                this.lastFrameDrawn = now;

                // Adjust target framerate down if we're not achieving that rate
                this.frameCount++;
                if ((this.frameCount % 100 == 0) && (1000.0 / this.actualFrameTime < this.targetFramerate * 0.92))
                {
                    this.targetFramerate = Math.Max(MinFramerate, (this.targetFramerate + (1000.0 / this.actualFrameTime)) / 2);
                }

                if (now > this.predNextFrame)
                {
                    this.predNextFrame = now;
                }
                else
                {
                    double milliseconds = this.predNextFrame.Subtract(now).TotalMilliseconds;
                    if (milliseconds >= TimerResolution)
                    {
                        Thread.Sleep((int)(milliseconds + 0.5));
                    }
                }

                this.predNextFrame += TimeSpan.FromMilliseconds(1000.0 / this.targetFramerate);

                this.Dispatcher.Invoke(DispatcherPriority.Send, new Action<int>(this.HandleGameTimer), 0);
            }
        }

        private void HandleGameTimer(int param)
        {
            // Draw new Wpf scene by adding all objects to canvas
            playfield.Children.Clear();
            foreach (var player in this.players)
            {
                player.Value.Draw(playfield.Children);
                
            }
            //BannerText.Draw(playfield.Children);
            FlyingText.Draw(playfield.Children);

            this.CheckPlayers();
        }
        #endregion GameTimer/Thread

        #region Kinect Speech processing
        private void RecognizerSaidSomething(object sender, SpeechRecognizer.SaidSomethingEventArgs e)
        {
            FlyingText.NewFlyingText(this.screenRect.Width / 30, new Point(this.screenRect.Width / 2, this.screenRect.Height / 2), e.Matched);
//            switch (e.Verb)
//            {
//                case SpeechRecognizer.Verbs.Pause:
//                    
//                    break;
//                case SpeechRecognizer.Verbs.Resume:
//                    break;
//            }
        }

        private void EnableAecChecked(object sender, RoutedEventArgs e)
        {
            var enableAecCheckBox = (CheckBox)sender;
            this.UpdateEchoCancellation(enableAecCheckBox);
        }

        private void UpdateEchoCancellation(CheckBox aecCheckBox)
        {
            this.mySpeechRecognizer.EchoCancellationMode = aecCheckBox.IsChecked != null && aecCheckBox.IsChecked.Value
                ? EchoCancellationMode.CancellationAndSuppression
                : EchoCancellationMode.None;
        }

        #endregion Kinect Speech processing
    }
}
