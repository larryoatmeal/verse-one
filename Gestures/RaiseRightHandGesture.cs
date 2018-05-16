using System;
using Microsoft.Kinect;

namespace ShapeGame.Gestures
{
    public class RaiseRightHandGesture
    {

        
        readonly int WINDOW_SIZE = 50;

        IGestureSegment[] _segments;

        int _currentSegment = 0;
        int _frameCount = 0;
        float lastGestureTime = 0.0f;
        float lastSuccessTime = 0.0f;
        float timeBetweenGesture = 1.0f;


        public event EventHandler GestureRecognized;

        public RaiseRightHandGesture()
        {
            RaiseRightHandSegment1 raiseRightHandSegment1 = new RaiseRightHandSegment1();
            RaiseRightHandSegment2 raiseRightHandSegment2 = new RaiseRightHandSegment2();
            RaiseRightHandSegment3 raiseRightHandSegment3 = new RaiseRightHandSegment3();
            RaiseRightHandSegment4 raiseRightHandSegment4 = new RaiseRightHandSegment4();

            _segments = new IGestureSegment[]
            {
                raiseRightHandSegment1,
                raiseRightHandSegment2,
                raiseRightHandSegment3,
                raiseRightHandSegment4
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
                        if (currentTime - lastSuccessTime > timeBetweenGesture)
                        {
                            GestureRecognized(this, new EventArgs());
                            lastSuccessTime = currentTime;
                            Reset();
                        }

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
                        //Console.WriteLine("holding gesture");
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