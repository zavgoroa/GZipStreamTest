using System;
using System.Threading;
using System.IO.Compression;
using System.IO;

namespace GZipTest
{
    class CompressionFile
    {
        private readonly long m_sizeBlock = 1024 * 1024;
        private readonly int m_sizeBlockSize = 4;

        public void Execute(CompressionMode mode, string sourceFileName, string targetFileName)
        {
            CompressionTaskResultHandler taskResultHandler = null;
            Exception exception = null;
            try
            {
                using (FileStream inputStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read))
                {
                    long offsetBytes = 0;
                    int countBytes = 0;
                    int countTask = 0;
                    for (countTask = 0; ; ++countTask)
                    {
                        if (inputStream.Position >= inputStream.Length) break;
                        if (mode == CompressionMode.Compress)
                        {
                            countBytes = checked((int)calculateSizeBlock(inputStream.Length - offsetBytes));
                        }
                        else
                        {
                            byte[] sizeBlock = new byte[m_sizeBlockSize];
                            inputStream.Read(sizeBlock, 0, m_sizeBlockSize);
                            offsetBytes += m_sizeBlockSize;
                            countBytes = BitConverter.ToInt32(sizeBlock, 0);
                        }

                        CompressionTask task = new CompressionTask(countTask, mode, sourceFileName, countBytes, offsetBytes);
                        CustomFixedThreadPool.ExecuteTask(task);
                        //Console.WriteLine($"process part {i} bytes from {offsetBytes} to {offsetBytes + countBytes} ({countBytes}), all {inputStream.Length} bytes");
                        offsetBytes += countBytes;
                        inputStream.Seek(countBytes, SeekOrigin.Current);
                    }
                    //Console.WriteLine($"pos:{inputStream.Length}, length: {inputStream.Length}");

                    taskResultHandler = new CompressionTaskResultHandler(targetFileName);
                    int completedTask = 0;
                    while (completedTask != countTask)
                    {
                        CompressionTask task = CustomFixedThreadPool.GetCompletedTask(completedTask);
                        if (task == null)
                        {
                            Thread.Sleep(5);
                        }
                        else
                        {
                            taskResultHandler.Handle(task);
                            task.Dispose();
                            completedTask++;
                        }
                    }
                    Console.WriteLine($"Size source file: {inputStream.Length} bytes");
                    Console.WriteLine($"Size target file: {taskResultHandler.NumberOutputBytes} bytes");
                    Console.WriteLine($"Start task: {countTask}, completed task: {completedTask}");
                }
            }
            catch (Exception externException)
            {
                exception = externException;
            }
            finally
            {
                if (taskResultHandler != null)
                {
                    taskResultHandler.Dispose();
                }
                if (exception != null)
                {
                    throw exception;
                }
            }
        }

        private long calculateSizeBlock(long countBytes)
        {
            if (countBytes > m_sizeBlock)
            {
                return m_sizeBlock;
            }
            else
            {
                return countBytes;
            }
        }
    }
}
