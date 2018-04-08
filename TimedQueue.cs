//------------------------------------------------------------------------------
// <copyright file="Player.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace ShapeGame
{
    public class TimedQueue<T>
    {
        public Queue<TimedEvent<T>> queue;

        int idCounter = 0;
        float windowSize;
        public TimedQueue(float windowSize)
        {
            queue = new Queue<TimedEvent<T>>();
            this.windowSize = windowSize;
        }

        public void Push(T data)
        { 
            float t = MainWindow.stopWatch.ElapsedMilliseconds / 1000.0f;
            queue.Enqueue(new TimedEvent<T>(data, t, idCounter));
            idCounter += 1;
            Console.WriteLine(queue.Count);
            Update(t);
        }

        private void Update(float t)
        {

            while (queue.Count > 0 && queue.Peek().Timestamp < t - windowSize)
            {
                queue.Dequeue();
            }

        }

        public List<TimedEvent<T>> GetData()
        {
            return new List<TimedEvent<T>>(queue.ToArray());
        }
    }



}