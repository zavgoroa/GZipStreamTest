using System;
using System.IO;

namespace GZipTest
{
    class CompressionTaskResultHandler: IDisposable
    {
        private FileStream m_outputStream;
        public long m_numberOutputBytes = 0;

        public long NumberOutputBytes
        {
            get { return m_numberOutputBytes; }
            private set { m_numberOutputBytes = value; }
        }

        public CompressionTaskResultHandler(string outputFileName)
        {
            m_outputStream = new FileStream(outputFileName, FileMode.OpenOrCreate, FileAccess.Write);
        }

        public virtual void Handle(CompressionTask task)
        {
            if (task.Exception == null)
            {
                m_numberOutputBytes += task.ResultData.Length;
                //Console.WriteLine($"completed task: {task.Id}, block: {task.ResultData.Length}");
                m_outputStream.Write(task.ResultData, 0, task.ResultData.Length);
                m_outputStream.Flush();
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
