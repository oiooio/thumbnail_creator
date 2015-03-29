using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ThumbnailCreator
{
    
    class Log
    {
        private string logPath;
        public Log(string applicationPath) 
        {
            logPath = applicationPath + @"\output.log";

        }
        public void AppendMessage(string message)
        {
            File.AppendAllText(logPath, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString()+"."+DateTime.Now.Millisecond + ": " + message + Environment.NewLine);            
        }
    }
}
