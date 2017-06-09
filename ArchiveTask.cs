using System;
using System.IO.Compression;
using System.IO;

namespace GZipTest
{

    class ArchiveTask: IDisposable
    {
        private CompressionMode m_mode;
        private readonly string m_sourceFileName;
        private int m_countBytes;
        private long m_offsetBytes;
        private byte[] m_resultData;
        private int m_id;
        private Exception m_exception;

        public ArchiveTask(int id, CompressionMode mode, string sourceFileName, int countBytes, long offsetBytes)
        {
            m_id = id;
            m_mode = mode;
            m_sourceFileName = sourceFileName;
            m_countBytes = countBytes;
            m_offsetBytes = offsetBytes;
        }

        public byte[] ResultData
        {
            get { return m_resultData; }
            private set { m_resultData = value; }
        }

        public int Id
        {
            get { return m_id; }
            private set { m_id = value; }
        }

        public CompressionMode CompressionMode
        {
            get { return m_mode; }
            private set { m_mode = value; }
        }

        public Exception Exception
        {
            get { return m_exception; }
            private set { m_exception = value; }
        }

        public void execute()
        {
            try
            {
                using (FileStream inputStream = new FileStream(m_sourceFileName, FileMode.Open, FileAccess.Read))
                {
                    inputStream.Seek(m_offsetBytes, SeekOrigin.Begin);
                    byte[] rawData = new byte[m_countBytes];
                    inputStream.Read(rawData, 0, m_countBytes);

                    using (MemoryStream rawDataStream = new MemoryStream(rawData))
                    {
                        if (m_mode == CompressionMode.Compress)
                        {
                            using (MemoryStream compressDataStream = new MemoryStream())
                            {
                                using (GZipStream zipStream = new GZipStream(compressDataStream, m_mode))
                                {
                                    zipStream.Write(rawDataStream.ToArray(), 0, rawDataStream.ToArray().Length);
                                }
                                byte[] compressData = compressDataStream.ToArray();
                                m_resultData = new byte[compressData.Length + 4];
                                Array.Copy(BitConverter.GetBytes(compressData.Length), 0, m_resultData, 0, 4);
                                Array.Copy(compressData, 0, m_resultData, 4, compressData.Length);
                            }
                        }
                        else
                        {
                            using (MemoryStream decompressDataStream = new MemoryStream())
                            {
                                using (GZipStream zipStream = new GZipStream(rawDataStream, m_mode))
                                {
                                    zipStream.CopyTo(decompressDataStream);
                                }
                                m_resultData = decompressDataStream.ToArray();
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                m_exception = exception;
            }
        }

        public void Dispose()
        {
            if (m_resultData != null)
            {
                Array.Clear(m_resultData, 0, m_resultData.Length);
            }
        }
    }
}
