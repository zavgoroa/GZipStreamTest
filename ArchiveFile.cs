using System;
using System.Threading;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;

namespace GZipTest
{
    class ArchiveFile
    {
        private static long m_sizeBlock = 1024 * 1024;

        public void archive(CompressionMode mode, string sourceFileName, string targetFileName)
        {
            using (FileStream inputStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read))
            {
                Console.WriteLine($"Size source file: {inputStream.Length} bytes");
                long offsetBytes = 0;
                int countBytes = 0;
                int countTask = 0;
                for (int i = 0; ; ++i, ++countTask)
                {
                    if (inputStream.Position >= inputStream.Length) break;
                    if (mode == CompressionMode.Compress)
                    {
                        countBytes = (int)sizeBlock(inputStream.Length - offsetBytes);
                    }
                    else
                    {
                        byte[] sizeBlock = new byte[4];
                        inputStream.Read(sizeBlock, 0, 4);
                        offsetBytes += 4;
                        countBytes = BitConverter.ToInt32(sizeBlock, 0);
                    }

                    ArchiveTask task = new ArchiveTask(i, mode, sourceFileName, countBytes, offsetBytes);
                    CustomFixedThreadPool.executeTask(task);
                    //Console.WriteLine($"process part {i} bytes from {offsetBytes} to {offsetBytes + countBytes} ({countBytes}), all {inputStream.Length} bytes");
                    offsetBytes += countBytes;
                    inputStream.Seek(countBytes, SeekOrigin.Current);
                }
                //Console.WriteLine($"pos:{inputStream.Length}, length: {inputStream.Length}");

                ArchiveTaskResultHandler taskResultHandler = new ArchiveTaskResultHandler(targetFileName);
                int completedTask = 0;
                while (completedTask != countTask)
                {
                    ArchiveTask task = CustomFixedThreadPool.getCompletedTask(completedTask);
                    if (task == null)
                    {
                        Thread.Sleep(5);
                    } else
                    {
                        taskResultHandler.processResultTask(task);
                        completedTask++;
                    }
                }
                taskResultHandler.Dispose();
                Console.WriteLine($"Size out file in bytes: {taskResultHandler.summ}");
                Console.WriteLine($"Start task: {countTask}, completed task: {completedTask}");
            }
        }

        public static long sizeBlock(long countBytes)
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

        public static long GetFreeMemory()
        {
            PerformanceCounter pc = new PerformanceCounter("Memory", "Available Bytes");
            long freeMemory = Convert.ToInt64(pc.RawValue);
            return freeMemory;
        }
    }
}
