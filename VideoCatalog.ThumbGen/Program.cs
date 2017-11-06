using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CommandLine.Utility;

namespace VideoCatalog.ThumbGen
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Arguments cmdLine = new Arguments(args);
                bool hasErrors = false;

                string path = cmdLine["path"];

                if (String.IsNullOrEmpty(path))
                {
                    hasErrors = true;
                    Console.WriteLine("Path argument is missing.");
                }

                bool recursive = cmdLine["recursive"] != null || cmdLine["r"] != null;
                bool verbose = cmdLine["verbose"] != null || cmdLine["v"] != null;
                bool forceRecreate = cmdLine["force"] != null || cmdLine["f"] != null;

                if (!String.IsNullOrEmpty(cmdLine["help"]) || hasErrors)
                {
                    Console.WriteLine("Usage:\r\n" +
                                      "-path=\"C:\\My_videos\"   Main directory for thumbnail processing.\r\n" +
                                      "-recursive or -r        Recursive processing option (Default: false).\r\n" +
                                      "-force or -f            Force recreation of all thumbs (Default: false).\r\n" +
                                      "-verbose or -v          Outputs information messages (Default: false).");
                    Console.ReadLine();
                }

                if (!hasErrors)
                {
                    Stopwatch swGeral = new Stopwatch();
                    swGeral.Start();
                    VideoHelper videoHelper = new VideoHelper();
                    videoHelper.ProcessDirectory(path, recursive, verbose, forceRecreate);
                    swGeral.Stop();
                    if (verbose)
                    {
                        Console.WriteLine("Total time: " + swGeral.ElapsedMilliseconds + "ms");
                    }
                    Console.WriteLine("Finished!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.Message);
            }
        }
    }
}
