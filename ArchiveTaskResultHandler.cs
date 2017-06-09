using System;
using System.Collections.Generic;
using System.IO;

namespace GZipTest
{
    class ArchiveTaskResultHandler: IDisposable
    {
        private FileStream m_outputStream;
        public long summ = 0;

        public ArchiveTaskResultHandler(string outputFileName)
        {
            m_outputStream = new FileStream(outputFileName, FileMode.OpenOrCreate, FileAccess.Write);
        }

        public virtual void processResultTask(ArchiveTask task)
        {
            if (task.Exception == null)
            {
                summ += task.ResultData.Length;
                //Console.WriteLine($"completed task: {task.Id}, block: {task.ResultData.Length}");
                m_outputStream.Write(task.ResultData, 0, task.ResultData.Length);
                m_outputStream.Flush();
                task.Dispose();
            } else
            {
                throw task.Exception;
            }   
        }

        public void Dispose()
        {
            if (m_outputStream != null)
            {
                m_outputStream.Close();
            }
        }
    }
}
