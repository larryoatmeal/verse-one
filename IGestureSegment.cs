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
    
}