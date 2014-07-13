using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SQLCDCIDLoader
{
 
    public class LogRoller
    {
        // The amount of numbered file logs
        private int numberedFileMax = 10;
        private int maxFileSizeInMB = 1;
        public string logpath { get; set; }
        //private string logpath = "";
        private string logDirectoryWithSlash = "";
        private string logFileName = "";
        private string logFileExtensionWithDot = "";
       
        public LogRoller()
        {
            
        }
        public LogRoller(string logpath)
        {
            int slashIndex = logpath.LastIndexOf(@"\");
            if (slashIndex == -1)
                slashIndex = logpath.LastIndexOf(@"/");
            if (slashIndex == -1)
                throw new ApplicationException(@"The fullFilePath must have at least one backslash ('\') or slash ('/')");

            int dotIndex = logpath.LastIndexOf(@".");
            if (dotIndex == -1 || dotIndex >= logpath.Length - 1)
                throw new ApplicationException(@"The fullFilePath must have a dot before its extension");

            this.logpath = logpath;
            this.logDirectoryWithSlash = logpath.Substring(0, slashIndex + 1);
            this.logFileName = logpath.Substring(slashIndex + 1, dotIndex - slashIndex - 1);
            this.logFileExtensionWithDot = logpath.Substring(dotIndex);

        }

        public void LogMessage(string message)
        {
            FileInfo logFileInfo = new FileInfo(this.logpath);

            if (!logFileInfo.Exists)
            {
                if (!Directory.Exists(logFileInfo.DirectoryName))
                    Directory.CreateDirectory(logFileInfo.DirectoryName);
            }
            else
            {
                // The log file exists, see if we need to roll
                if (logFileInfo.Length / 1024 / 1024 > this.maxFileSizeInMB)
                {
                    // We need to roll the files
                    string currentlogpath = "";
                    string previousEachlogpath = "";
                    for (int i = this.numberedFileMax - 1; i > 0; i--)
                    {
                        currentlogpath = this.GetLogFullFileNameFromNumber(i);

                        FileInfo errorLog = new FileInfo(currentlogpath);
                        if (errorLog.Exists)
                        {
                            // This will happen at most one time per method call, if any.
                            if (previousEachlogpath == "")
                                previousEachlogpath = this.GetLogFullFileNameFromNumber(i + 1);

                            // You cannot move and do an overwrite. So lets use the copy command, this
                            // one can overwrite.
                            //File.Move(currentlogpath, previousEachlogpath);
                            File.Copy(currentlogpath, previousEachlogpath, true);
                        }

                        previousEachlogpath = currentlogpath;
                    }
                    File.Copy(this.logpath, currentlogpath, true);
                    File.Delete(this.logpath);
                }
            }

            // Write to the log. 
            // 
            // In the future you might want to deal with the scenario where
            // two LogRollers are trying to write to the same file at the same time. I'm not
            // dealing with this right now because this will never happen in my current application
            // and I do not want to hinder performace. 
            //
            // There are two options to deal with this. 1) Making this code be thread safe. 2) Somehow
            // catching the IOException and trying several times after waiting a time interval?
            System.IO.StreamWriter writer = new System.IO.StreamWriter(this.logpath, true);
            
            writer.WriteLine(System.Environment.NewLine + "[" + DateTime.Now + "]: "+ message);
            writer.Close();
        }

        public void LogException(Exception ex)
        {

            this.LogMessage(ex.ToString());
        }
        private string GetLogFullFileNameFromNumber(int number)
        {
            return this.logDirectoryWithSlash + this.logFileName
                + "-" + number.ToString().PadLeft(2,'0') + this.logFileExtensionWithDot;
        }
    }
}