//------------------------------------------------------------------------------
// <copyright file="Player.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.Kinect;
using System;

namespace ShapeGame
{
    /// <summary>
    /// Represents the gesture part recognition result.
    /// </summary>
    public enum GesturePartResult
    {
        /// <summary>
        /// Gesture part failed.
        /// </summary>
        Failed,

        /// <summary>
        /// Gesture part succeeded.
        /// </summary>
        Succeeded
    }
    /// <summary>
    /// Represents a single gesture segment which uses relative positioning of body parts to detect a gesture.
    /// </summary>
    public interface IGestureSegment
        {
            /// <summary>
            /// Updates the current gesture.
            /// </summary>
            /// <param name="skeleton">The skeleton.</param>
            /// <returns>A GesturePartResult based on whether the gesture part has been completed.</returns>
            GesturePartResult Update(Skeleton skeleton);
        }

        public class WaveSegment1 : IGestureSegment
        {
            /// <summary>
            /// Updates the current gesture.
            /// </summary>
            /// <param name="skeleton">The skeleton.</param>
            /// <returns>A GesturePartResult based on whether the gesture part has been completed.</returns>
            public GesturePartResult Update(Skeleton skeleton)
            {
                // Hand above elbow
                if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
                {
                    // Hand right of elbow
                    if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ElbowRight].Position.X)
                    {
                        return GesturePartResult.Succeeded;
                    }
                }

                // Hand dropped
                Console.WriteLine("1 failed");
                return GesturePartResult.Failed;
            }
        }

        public class WaveSegment2 : IGestureSegment
        {
            /// <summary>
            /// Updates the current gesture.
            /// </summary>
            /// <param name="skeleton">The skeleton.</param>
            /// <returns>A GesturePartResult based on whether the gesture part has been completed.</returns>
            public GesturePartResult Update(Skeleton skeleton)
            {
                // Hand above elbow
                if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
                {
                    // Hand left of elbow
                    if (skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ElbowRight].Position.X)
                    {
                        return GesturePartResult.Succeeded;
                    }
                }

            // Hand dropped
            Console.WriteLine("2 failed");
            return GesturePartResult.Failed;
            }
        }


    public class CrossSegment1 : IGestureSegment
    {

        public GesturePartResult Update(Skeleton skeleton)
        {
            // left hand to right of left elbow and right hand to the left of right elbow
            var leftArmCorrect = skeleton.Joints[JointType.HandLeft].Position.X > skeleton.Joints[JointType.ElbowLeft].Position.X;
            var rightArmCorrect = skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ElbowRight].Position.X;
            var handsCorrect = skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.HandLeft].Position.X;
            if (leftArmCorrect && rightArmCorrect && handsCorrect)
            {

             return GesturePartResult.Succeeded;

            }
            return GesturePartResult.Failed;
        }
    }


    public class CrossSegment2 : IGestureSegment
    {

        public GesturePartResult Update(Skeleton skeleton)
        {
            var handsCorrect = skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.HandRight].Position.X;
            if (handsCorrect)
            {
                return GesturePartResult.Succeeded;

            }
            return GesturePartResult.Failed;
        }
    }

    public class CrossSegment3 : IGestureSegment
    {

        public GesturePartResult Update(Skeleton skeleton)
        {
            //left hand to left of elbow and shoulder, right arm to right of elbow and shoulders
            var leftArmCorrect = skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ElbowLeft].Position.X;
            var rightArmCorrect = skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ElbowRight].Position.X;
            var handsCorrect = skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.HandRight].Position.X;
            if (leftArmCorrect && rightArmCorrect && handsCorrect)
            {
                return GesturePartResult.Succeeded;

            }
            return GesturePartResult.Failed;
        }
    }

    public class RaiseRightHandSegment1 : IGestureSegment
    {

        public GesturePartResult Update(Skeleton skeleton)
        {
            var armCorrect = skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.HipRight].Position.Y;
            if (armCorrect)
            {
                return GesturePartResult.Succeeded;

            }
            return GesturePartResult.Failed;
        }
    }

    public class RaiseRightHandSegment2 : IGestureSegment
    {

        public GesturePartResult Update(Skeleton skeleton)
        {
            var armCorrect = (skeleton.Joints[JointType.Head].Position.Y) > skeleton.Joints[JointType.HandRight].Position.Y;
            if (armCorrect)
            {
                return GesturePartResult.Succeeded;

            }
            return GesturePartResult.Failed;
        }
    }

    public class RaiseRightHandSegment3 : IGestureSegment
    {

        public GesturePartResult Update(Skeleton skeleton)
        {
            var armCorrect = skeleton.Joints[JointType.Head].Position.Y < skeleton.Joints[JointType.HandRight].Position.Y;
            if (armCorrect)
            {
                return GesturePartResult.Succeeded;

            }
            return GesturePartResult.Failed;
        }
    }

    public class RaiseRightHandSegment4 : IGestureSegment
    {

        public GesturePartResult Update(Skeleton skeleton)
        {
            var armCorrect = (skeleton.Joints[JointType.Head].Position.Y) < skeleton.Joints[JointType.ElbowRight].Position.Y && skeleton.Joints[JointType.Head].Position.Y < skeleton.Joints[JointType.HandRight].Position.Y;
            if (armCorrect)
            {
                return GesturePartResult.Succeeded;

            }
            return GesturePartResult.Failed;
        }
    }

    public class RaiseLeftHandSegment1 : IGestureSegment
    {

        public GesturePartResult Update(Skeleton skeleton)
        {
            var armCorrect = skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.HipLeft].Position.Y;
            if (armCorrect)
            {
                return GesturePartResult.Succeeded;

            }
            return GesturePartResult.Failed;
        }
    }

    public class RaiseLeftHandSegment2 : IGestureSegment
    {

        public GesturePartResult Update(Skeleton skeleton)
        {
            var armCorrect = (skeleton.Joints[JointType.Head].Position.Y) > skeleton.Joints[JointType.HandLeft].Position.Y;
            if (armCorrect)
            {
                return GesturePartResult.Succeeded;

            }
            return GesturePartResult.Failed;
        }
    }

    public class RaiseLeftHandSegment3 : IGestureSegment
    {

        public GesturePartResult Update(Skeleton skeleton)
        {
            var armCorrect = skeleton.Joints[JointType.Head].Position.Y < skeleton.Joints[JointType.HandLeft].Position.Y;
            if (armCorrect)
            {
                return GesturePartResult.Succeeded;

            }
            return GesturePartResult.Failed;
        }
    }

    public class RaiseLeftHandSegment4 : IGestureSegment
    {

        public GesturePartResult Update(Skeleton skeleton)
        {
            var armCorrect = (skeleton.Joints[JointType.Head].Position.Y) < skeleton.Joints[JointType.ElbowLeft].Position.Y && skeleton.Joints[JointType.Head].Position.Y < skeleton.Joints[JointType.HandLeft].Position.Y;
            if (armCorrect)
            {
                return GesturePartResult.Succeeded;

            }
            return GesturePartResult.Failed;
        }
    }



}