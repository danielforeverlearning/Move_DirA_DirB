using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Security;

namespace Move_DirA_DirB
{
    class Program
    {
        static void ScanDirectory(ref List<string> source_fullpath, ref List<string> source_basename, ref List<string> source_dirname, string searchpath)
        {
            try
            {
                DirectoryInfo mydirinfo = new DirectoryInfo(searchpath);
                FileInfo[] folderfiles = mydirinfo.GetFiles();
                for (int ii = 0; ii < folderfiles.Length; ii++)
                {
                    string fullpath = folderfiles[ii].FullName;
                    source_fullpath.Add(fullpath);

                    string basename = folderfiles[ii].Name;
                    source_basename.Add(basename);

                    string dirname = folderfiles[ii].DirectoryName;
                    source_dirname.Add(dirname);
                }
                
                DirectoryInfo[] folderdirs = mydirinfo.GetDirectories();
                for (int ii = 0; ii < folderdirs.Length; ii++)
                {
                    ScanDirectory(ref source_fullpath, ref source_basename, ref source_dirname, folderdirs[ii].FullName);
                }
            }
            catch (Exception ex)
            {
                string errorstr = "Move_DirA_DirB: ScanDirectory caught exception!";
                Console.WriteLine(errorstr);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                string logstr = string.Format("{0} MSG={1} STACK={2}", errorstr, ex.Message, ex.StackTrace);
                EventLog mylog = new EventLog();
                mylog.Source = "Move_DirA_DirB";
                mylog.WriteEntry(logstr, EventLogEntryType.Error);
                System.Environment.FailFast("Move_DirA_DirB ScanDirectory FailFast");
            }
        }//ScanDirectory

        static void Main(string[] args)
        {
            try
            {
                //If this program throws SecurityException it is because of trying to do EventLog.SourceExists or EventLog.CreateEventSource
                //Possible solutions are:
                //(1) Control Panel-->System and Security-->Administrative Tools-->Local Security Policy-->Local Policies-->User Rights Assignment-->Manage auditing and security Log-->(add user or group)
                //(2) Run this program with elevated privileges such as "run as administrator"
                //(3) When running this program in task schedular make sure it is run with elevated privileges
                bool sourceexists = false;
                sourceexists = EventLog.SourceExists("Move_DirA_DirB");
                if (sourceexists == false)
                {
                    EventLog.CreateEventSource("Move_DirA_DirB", "Application");
                    Console.WriteLine("Creating Event Source in Application Log Move_DirA_DirB.");
                }
                else
                    Console.WriteLine("Event Source in Application Log Move_DirA_DirB already created OK.");


                string sourcefolder = ConfigurationManager.AppSettings["MySourceFolder"];
                string destfolder = ConfigurationManager.AppSettings["MyDestinationFolder"];
                string overwritestr = ConfigurationManager.AppSettings["MyOverwriteFlag"];
                bool overwriteflag = false;
                if (overwritestr.ToLower() == "true" || overwritestr == "1")
                    overwriteflag = true;

                Console.WriteLine("SOURCE = " + sourcefolder);
                Console.WriteLine("DEST = " + destfolder);

                List<string> source_fullpath = new List<string>();
                List<string> source_basename = new List<string>();
                List<string> source_dirname = new List<string>();
                List<string> dest_dirname = new List<string>();
                ScanDirectory(ref source_fullpath, ref source_basename, ref source_dirname, sourcefolder);

                foreach (string str in source_dirname)
                {
                    string poststr = str.Substring(sourcefolder.Length, str.Length - sourcefolder.Length);
                    string deststr = destfolder + poststr;
                    dest_dirname.Add(deststr);
                }

                //Console.WriteLine("********** fullpath *************");
                //foreach (string str in source_fullpath)
                //    Console.WriteLine(str);

                //Console.WriteLine("********** basename *************");
                //foreach (string str in source_basename)
                //    Console.WriteLine(str);

                //Console.WriteLine("********** dirname *************");
                //foreach (string str in source_dirname)
                //    Console.WriteLine(str);

                //Console.WriteLine("********** destdir *************");
                //foreach (string str in dest_dirname)
                //    Console.WriteLine(str);

                //check to make sure each destination folder exists, if not exists, then create it
                foreach (string dir in dest_dirname)
                {
                    bool exists = Directory.Exists(dir);
                    if (exists == false)
                        Directory.CreateDirectory(dir);
                }

                //do move files
                for (int ii = 0; ii < dest_dirname.Count; ii++)
                {
                    string destpath = dest_dirname[ii] + "\\" + source_basename[ii];
                    Console.WriteLine(destpath);

                    //check to see if file already exists first
                    bool alreadyexists = File.Exists(destpath);
                    if (alreadyexists)
                    {
                        if (overwriteflag)
                        {
                            File.SetAttributes(source_fullpath[ii], FileAttributes.Normal);
                            File.SetAttributes(destpath, FileAttributes.Normal);
                            File.Copy(source_fullpath[ii], destpath, true);
                            File.Delete(source_fullpath[ii]);
                        }
                    }
                    else
                        File.Move(source_fullpath[ii], destpath);
                }
            }
            catch (Exception ex)
            {
                string errorstr = "Move_DirA_DirB: Main caught exception!";
                Console.WriteLine(errorstr);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                string logstr = string.Format("{0} MSG={1} STACK={2}", errorstr, ex.Message, ex.StackTrace);
                EventLog mylog = new EventLog();
                mylog.Source = "Move_DirA_DirB";
                mylog.WriteEntry(logstr, EventLogEntryType.Error);
                System.Environment.FailFast("Move_DirA_DirB Main FailFast");
            }

            //success log entry
            EventLog goodlog = new EventLog();
            goodlog.Source = "Move_DirA_DirB";
            goodlog.WriteEntry("Move_DirA_DirB Ran successfully");
        }//Main
    }//program
}//namespace
