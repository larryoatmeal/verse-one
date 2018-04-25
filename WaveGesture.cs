//------------------------------------------------------------------------------
// <copyright file="Player.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.Kinect;
using System;

namespace ShapeGame
{
    public class WaveGesture
    {

        readonly int WINDOW_SIZE = 50;

        IGestureSegment[] _segments;

        int _currentSegment = 0;
        int _frameCount = 0;
        float lastGestureTime = 0.0f;


        public event EventHandler GestureRecognized;

        public WaveGesture()
        {
            WaveSegment1 waveRightSegment1 = new WaveSegment1();
            WaveSegment2 waveRightSegment2 = new WaveSegment2();

            _segments = new IGestureSegment[]
            {
                waveRightSegment1,
                waveRightSegment2,
                waveRightSegment1,
                waveRightSegment2,
                waveRightSegment1,
                waveRightSegment2
            };
        }

        /// <summary>
        /// Updates the current gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton data.</param>
        public void Update(Skeleton skeleton)
        {
            var currentTime = MainWindow.stopWatch.ElapsedMilliseconds / 1000.0f;
            GesturePartResult result = _segments[_currentSegment].Update(skeleton);
            Console.WriteLine(_currentSegment);
            if (result == GesturePartResult.Succeeded)
            {
                if (_currentSegment + 1 < _segments.Length)
                {
                    _currentSegment++;
                    _frameCount = 0;
                    lastGestureTime = currentTime;
                }
                else
                {
                    if (GestureRecognized != null)
                    {
                        GestureRecognized(this, new EventArgs());
                        Reset();
                    }
                }
            }
            else if (result == GesturePartResult.Failed || _frameCount == WINDOW_SIZE)
            {
                if (_currentSegment > 0)
                {
                    GesturePartResult prevResult = _segments[_currentSegment - 1].Update(skeleton);

                    if (currentTime - lastGestureTime < 2 && prevResult == GesturePartResult.Succeeded) //we still havent left prev state
                    {
                        Console.WriteLine("holding gesture");
                    } 
                    else
                    {
                        Reset();
                    }
                }
                else
                {
                    Reset();
                }
            }
            else
            {
                _frameCount++;
            }
        }

        /// <summary>
        /// Resets the current gesture.
        /// </summary>
        public void Reset()
        {
            _currentSegment = 0;
            _frameCount = 0;
            lastGestureTime = 0.0f;
        }
    }
    
}