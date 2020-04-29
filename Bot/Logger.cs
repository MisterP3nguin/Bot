using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace Bot
{
    public class Logger
    {
        public static void Log (string text)
        {
            StreamWriter Logfile = new StreamWriter(Directory.GetCurrentDirectory() + "/logfile.log",true);
            Logfile.WriteLine(text);
            Logfile.Close();
            Console.WriteLine(text);
        }
    }
}
