using System;
using System.Diagnostics;

namespace Playarr.Test.Dummy
{
    public class DummyApp
    {
        public const string DUMMY_PROCCESS_NAME = "Playarr.Test.Dummy";

        private static void Main(string[] args)
        {
            var process = Process.GetCurrentProcess();

            Console.WriteLine("Dummy process. ID:{0} Name:{1} Path:{2}", process.Id, process.ProcessName, process.MainModule.FileName);
            Console.ReadLine();
        }
    }
}
