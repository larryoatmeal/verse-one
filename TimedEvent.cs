//------------------------------------------------------------------------------
// <copyright file="Player.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace ShapeGame
{

    public class TimedEvent<T>
    {

        T data;
        float timestamp;
        int id;

        public TimedEvent(T data, float timestamp, int id)
        {
            this.data = data;
            this.timestamp = timestamp;
            this.id = id;
        }

        public float Timestamp { get => timestamp; }
        public T Data { get => data;  }
        public int ID { get => id; }

    }
}