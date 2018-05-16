//------------------------------------------------------------------------------
// <copyright file="Player.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace ShapeGame.Gestures
{
    public abstract class Gesture
    {
        public abstract EventHandler GetGestureRecognizedHandler();
    }
}