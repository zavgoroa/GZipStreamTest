using System;
using System.Collections.Generic;
using System.Threading;

namespace GZipTest
{
    static class CustomFixedThreadPool
    {
        private static List<Thread> m_thread = new List<Thread>();
        private static Queue<ArchiveTask> m_tasks = new Queue<ArchiveTask>();
        private static SortedDictionary<int, ArchiveTask> m_completedTask = new SortedDictionary<int, ArchiveTask>();

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

        public static ArchiveTask getCompletedTask(int index)
        {
            ArchiveTask task = null;
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
                ArchiveTask task = null;
                lock(m_tasks)
                {
                    while (m_tasks.Count == 0)
                    {
                        Monitor.Wait(m_tasks);
                    }
                    task = m_tasks.Dequeue();
                }
                task.execute();
                addCompletedTask(task);
            }
        }

        private static void addCompletedTask(ArchiveTask task)
        {
            lock (m_completedTask)
            {
                m_completedTask.Add(task.Id, task);
            }
        }

        public static void executeTask(ArchiveTask task)
        {
            lock(m_tasks)
            {
                m_tasks.Enqueue(task);
                Monitor.Pulse(m_tasks);
            }
        }
    }
}
