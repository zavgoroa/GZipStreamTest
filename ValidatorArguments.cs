using System.IO;
using System.IO.Compression;

namespace GZipTest
{
    class ValidatorArguments
    {
        private string m_inputFileName;
        private string m_outputFileName;
        private CompressionMode m_compressionMode;
        private readonly int m_countArgument = 3;
        private bool m_isChecked = false;

        public bool IsChecked
        {
            get { return m_isChecked; }
            private set { m_isChecked = value; }
        }
        public string InputFileName
        {
            get { return m_inputFileName; }
            private set { m_inputFileName = value; }
        }

        public string OutputFileName
        {
            get { return m_outputFileName; }
            private set { m_outputFileName = value; }
        }

        public CompressionMode CompressionMode
        {
            get { return m_compressionMode; }
            private set { m_compressionMode = value; }
        }

        private bool checkFile(string inputFileName, string outputFileName)
        {
            FileInfo inputFileInfo = new FileInfo(inputFileName);
            if (!inputFileInfo.Exists || inputFileInfo.Length == 0)
            {
                throw new System.ArgumentException("Bad input file.");
            }

            FileInfo outputFileInfo = new FileInfo(outputFileName);
            if (outputFileInfo.Exists)
            {
                throw new System.ArgumentException("Output file already exists.");
            }
            return true;
        }

        private bool checkArguments(string [] args)
        {
            if (args.Length != m_countArgument)
            {
                throw new System.ArgumentException("Invalid count arguments.");
            }

            bool isCompressMode = args[0].Equals("compress");
            bool isDecompressMode = args[0].Equals("decompress");
            if (!isCompressMode && !isDecompressMode)
            {
                throw new System.ArgumentException($"invalid parameter: { args[0] } (must be compress or decompress).");
            } else {
                m_compressionMode = isCompressMode ? CompressionMode.Compress : CompressionMode.Decompress;
            }

            if (args[1].Length == 0 || args[2].Length == 0)
            {
                throw new System.ArgumentException("Empty input or output file name.");
            } else {
                InputFileName = args[1];
                OutputFileName = args[2];
            }
            return true;
        }

        public bool Check(string[] args)
        {
            return (IsChecked = checkArguments(args) && checkFile(InputFileName, OutputFileName));
        }
    }

}