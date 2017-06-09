using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ValidatorArguments valArg = new ValidatorArguments();
                if (valArg.check(args))
                {
                    new ArchiveFile().archive(valArg.CompressionMode, valArg.InputFileName, valArg.OutputFileName);
                    Console.WriteLine($"Result: 1");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Result: 0, \n {exception.StackTrace} \n {exception.Source} \n { exception.Message}");
            }
        }
    }
}
