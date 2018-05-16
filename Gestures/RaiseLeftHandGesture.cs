using System;
using Microsoft.Kinect;

namespace ShapeGame.Gestures
{

    
    public class RaiseLeftHandGesture
    {
        
        readonly int WINDOW_SIZE = 50;

        IGestureSegment[] _segments;

        int _currentSegment = 0;
        int _frameCount = 0;
        float lastGestureTime = 0.0f;


        public event EventHandler GestureRecognized;

        public RaiseLeftHandGesture()
        {
            RaiseLeftHandSegment1 raiseLeftHandSegment1 = new RaiseLeftHandSegment1();
            RaiseLeftHandSegment2 raiseLeftHandSegment2 = new RaiseLeftHandSegment2();
            RaiseLeftHandSegment3 raiseLeftHandSegment3 = new RaiseLeftHandSegment3();
            RaiseLeftHandSegment4 raiseLeftHandSegment4 = new RaiseLeftHandSegment4();

            _segments = new IGestureSegment[]
            {
                raiseLeftHandSegment1,
                raiseLeftHandSegment2,
                raiseLeftHandSegment3,
                raiseLeftHandSegment4
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
            //Console.WriteLine(_currentSegment);
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