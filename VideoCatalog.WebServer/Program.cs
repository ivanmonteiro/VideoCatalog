using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CassiniDev;
using System.Diagnostics;

namespace VideoCatalog.WebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //string serverPath = Path.Combine(dirInfo.Parent.Parent.Parent.FullName, "VideoGif.Web");
            string serverPath = Path.Combine(Environment.CurrentDirectory, "Web");
            if (!Directory.Exists(serverPath))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(Environment.CurrentDirectory);
                serverPath = Path.Combine(dirInfo.Parent.Parent.Parent.FullName, "VideoGif.Web");
            }
            Console.WriteLine("Server path: " + serverPath);
            Console.WriteLine("Starting server...");
            //CassiniDevServer server = new CassiniDevServer();
            System.Net.IPAddress ip = System.Net.IPAddress.Parse("127.0.0.1");
            Server server = new Server(8000, "/", serverPath, ip, "localhost", 7200000);
            server.Start();
            string defaultUrl = server.RootUrl;
            Console.WriteLine("Server started: " + defaultUrl);
            Process.Start(defaultUrl);
            Console.WriteLine("Press S to shutdown the server.");
            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.S)
                {
                    Console.WriteLine("Shutting down server...");
                    server.ShutDown();
                    break;
                }
            }
        }
    }
}
