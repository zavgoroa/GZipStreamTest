using System;
using System.Collections.Generic;
using System.Threading;

namespace GZipTest
{
    static class CustomFixedThreadPool
    {
        private static List<Thread> m_thread = new List<Thread>();
        private static Queue<CompressionTask> m_tasks = new Queue<CompressionTask>();
        private static SortedDictionary<int, CompressionTask> m_completedTask = new SortedDictionary<int, CompressionTask>();

        static CustomFixedThreadPool()
        {
            for (int i = 0; i < Environment.ProcessorCount; ++i)
            {
                Thread newThread = new Thread(dataProcessing);
                newThread.IsBackground = true;
                m_thread.Add(newThread);
                newThread.Start();
            }
        }

        public static CompressionTask GetCompletedTask(int index)
        {
            CompressionTask task = null;
            lock (m_completedTask)
            {
                if (m_completedTask.ContainsKey(index))
                {
                    if (m_completedTask.TryGetValue(index, out task))
                    {
                        m_completedTask.Remove(index);
                    }
                }
            }
            if (task != null)
            {
                lock (m_tasks)
                {
                    Monitor.Pulse(m_tasks);
                    return task;
                }
            }
            return null;
        }

        private static void dataProcessing()
        {
            while(true)
            {
                CompressionTask task = null;
                lock(m_tasks)
                {
                    while (m_tasks.Count == 0)
                    {
                        Monitor.Wait(m_tasks);
                    }
                    task = m_tasks.Dequeue();
                }
                task.Execute();
                addCompletedTask(task);
            }
        }

        private static void addCompletedTask(CompressionTask task)
        {
            lock (m_completedTask)
            {
                m_completedTask.Add(task.Id, task);
            }
        }

        public static void ExecuteTask(CompressionTask task)
        {
            lock(m_tasks)
            {
                m_tasks.Enqueue(task);
                Monitor.Pulse(m_tasks);
            }
        }
    }
}
