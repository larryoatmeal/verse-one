//------------------------------------------------------------------------------
// <copyright file="Player.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace ShapeGame.Timing
{
    public class TimedQueue<T>
    {
        public Queue<TimedEvent<T>> queue;
        public Dictionary<string, string> status;
        int idCounter = 0;
        float windowSize;
        public TimedQueue(float windowSize)
        {
            queue = new Queue<TimedEvent<T>>();
            status = new Dictionary<string, string>();
            this.windowSize = windowSize;
        }

        public void Push(T data)
        { 
            float t = MainWindow.stopWatch.ElapsedMilliseconds / 1000.0f;
            queue.Enqueue(new TimedEvent<T>(data, t, idCounter));
            idCounter += 1;
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

    public class Throttle<T>
    {
        public Queue<TimedEvent<T>> queue;
        float windowSize;
        private Func<List<TimedEvent<T>>, int> callback;
        public Throttle(float windowSize, Func<List<TimedEvent<T>>, int> callback)
        {
            queue = new Queue<TimedEvent<T>>();
            this.windowSize = windowSize;
            this.callback = callback;
        }

        public void Push(T data)
        {
            float t = MainWindow.stopWatch.ElapsedMilliseconds / 1000.0f;
            queue.Enqueue(new TimedEvent<T>(data, t, 0));
            Update(t);
        }

        private void Update(float t)
        {
            while (queue.Count > 0 && queue.Peek().Timestamp < t - windowSize)
            {
//                queue.Dequeue();
                int itemsToRemove = callback(GetData());
                for (int i = 0; i < itemsToRemove; i++)
                {
                    queue.Dequeue();
                }
            }
        }

        public void UpdateThrottle()
        {
            float t = MainWindow.stopWatch.ElapsedMilliseconds / 1000.0f;
            Update(t);
        }

        public List<TimedEvent<T>> GetData()
        {
            return new List<TimedEvent<T>>(queue.ToArray());
        }
    }
}