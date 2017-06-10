using System;
using System.IO;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ValidatorArguments valArg = null;
            try
            {
                valArg = new ValidatorArguments();
                if (valArg.Check(args))
                {
                    Console.WriteLine("Executing...");
                    new CompressionFile().Execute(valArg.CompressionMode, valArg.InputFileName, valArg.OutputFileName);
                    Console.WriteLine($"Result: 1");
                }
            }
            catch (Exception exception)
            {
                try
                {
                    if (valArg != null && valArg.IsChecked)
                    {
                        File.Delete(valArg.OutputFileName);
                    }
                }
                catch { }
                finally
                {
                    Console.WriteLine($"Result: 0, \n stackTrace:{exception.StackTrace} \n Message:{ exception.Message}");
                }
            }
        }
    }
}
