using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoCatalog.Core;

namespace VideoCatalog.ThumbGen
{
    public class VideoHelper
    {
        private string _BasePath;
        private string _ffMpegPath;
        private int _thumbsCount = 8;
        private string _thumbsDirName = ".VideoCache";

        public VideoHelper()
        {
            _BasePath = AppDomain.CurrentDomain.BaseDirectory;
            _ffMpegPath = Path.Combine(_BasePath, "ffmpeg.exe");
        }

        public void GetVideoThumbnailSeek(string inputFile, List<float> thumbnailsTimes, List<string> thumbnailsFilesPaths)
        {
            try
            {
                for (int i = 0; i < thumbnailsTimes.Count; i++)
                {
                    //string arguments = string.Format("-y -threads 0 -loglevel error -nostdin -ss {0} -i \"{1}\" -t 1 -f mjpeg -vframes 1 \"{2}\"", thumbnailsTimes[i].ToString(CultureInfo.InvariantCulture), inputFile, thumbnailsFilesPaths[i]);
                    //string arguments = string.Format("-y -loglevel error -nostdin -ss {0} -i \"{1}\" -vframes 1 -s 320x240 \"{2}\" ", thumbnailsTimes[i].ToString(CultureInfo.InvariantCulture), inputFile, thumbnailsFilesPaths[i]);
                    string arguments = string.Format("-threads 0 -y -loglevel fatal -nostdin -ss {0} -i \"{1}\" -vframes 1 -vf scale=320:-1 \"{2}\"",
                            thumbnailsTimes[i].ToString(CultureInfo.InvariantCulture), inputFile,
                            thumbnailsFilesPaths[i]);

                    Process process = RunFFMpeg(arguments);
                    string error = process.StandardError.ReadToEnd();
                    string standard = process.StandardOutput.ReadToEnd();
                    if (process.ExitCode != 0)
                    {
                        throw new Exception("ExitCode do processo diferente de zero - " + error + " " + standard);
                    }
                    process.Close();
                }
                
            }
            catch (Exception ex)
            {
                throw new Exception("Erro processando thumbnais para " + inputFile, ex);
            }
        }

        public float GetDuration(string inputFile)
        {
            try
            {
                string arguments = string.Format("-threads 0 -nostdin -i \"{0}\"", new object[] { inputFile });
                Process process = RunFFMpeg(arguments);
                string info = process.StandardError.ReadToEnd();
                process.Close();

                if (!String.IsNullOrEmpty(info))
                {
                    var match = Regex.Match(info, @"Duration: (\d*):(\d*):(\d*).(\d*)", RegexOptions.Multiline);
                    if (match.Success)
                    {
                        string test = match.Groups[0].Value;
                        int hours = Int32.Parse(match.Groups[1].Value);
                        int minutes = Int32.Parse(match.Groups[2].Value);
                        int seconds = Int32.Parse(match.Groups[3].Value);
                        int fractionsOfSecond = Int32.Parse(match.Groups[4].Value);
                        //double duration = seconds + minutes*60 + hours*60*60;
                        TimeSpan duration = new TimeSpan(0, hours, minutes, seconds, fractionsOfSecond * 10);
                        return (float)duration.TotalSeconds;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting file duration for: " + inputFile, ex);
            }
        }

        private Process RunFFMpeg(string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(_ffMpegPath, arguments);
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = Path.GetDirectoryName(_BasePath);
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.StandardOutputEncoding = Encoding.Default;
            Process process = Process.Start(startInfo);
            //wait 30 seconds
            process.WaitForExit(30000);
            return process;
        }

        public void ProcessDirectory(string path, bool recursive, bool verbose, bool forceRecreateAllThumbs)
        {
            bool generateAllThumbs = false;
            //var files = Directory.EnumerateFiles(path).Where(f => fileTypes.Any(ft => f.EndsWith(ft, true, CultureInfo.InvariantCulture)));
            var files = Directory.EnumerateFiles(path).Where(f => Helper.HasSupportedExtension(f));
            string thumbsDirectory = Path.Combine(path, _thumbsDirName);
            if (forceRecreateAllThumbs && Directory.Exists(thumbsDirectory))
            {
                Directory.Delete(thumbsDirectory, true);
            }
            if (files.Any() && !Directory.Exists(thumbsDirectory))
            {
                Directory.CreateDirectory(thumbsDirectory);
                generateAllThumbs = true;
            }
            
            var actions = new List<Action>();

            if (files.Any())
            {
                foreach (var file in files)
                {
                    string fileHash = String.Format("{0:X}", Path.GetFileName(file).GetHashCode());

                    if (!generateAllThumbs)
                    {
                        var thumbsDirectoryFiles = Directory.EnumerateFiles(thumbsDirectory);
                        if (thumbsDirectoryFiles.Any(x => x.EndsWith(fileHash + "_1.jpg")))
                        {
                            continue;
                        }
                    }
                    actions.Add(() =>
                        {
                            GetThumbnailsTask(verbose, file, thumbsDirectory, fileHash);
                        });
                }
            }

            if (actions.Count > 0)
            {
                ParallelOptions options = new ParallelOptions();
                options.MaxDegreeOfParallelism = 4;
                Parallel.Invoke(options, actions.ToArray());
            }

            if (recursive)
            {
                var directories = Directory.EnumerateDirectories(path);

                foreach (var directory in directories)
                {
                    if (!directory.EndsWith(_thumbsDirName))
                    {
                        ProcessDirectory(directory, recursive, verbose, forceRecreateAllThumbs);
                    }
                }
            }
        }

        private void GetThumbnailsTask(bool verbose, string file, string thumbsDirectory, string fileHash)
        {
            try
            {
                Stopwatch swThumbnails = new Stopwatch();

                if (verbose)
                {
                    swThumbnails.Start();
                }

                float duration = this.GetDuration(file);

                if (verbose)
                {
                    swThumbnails.Stop();
                    Console.WriteLine(swThumbnails.ElapsedMilliseconds + "ms - Info - " + Path.GetFileName(file));
                    swThumbnails.Restart();
                }

                if (duration > 0)
                {
                    var thumbnailsTimes = new List<float>();
                    var thumbnailsFilePaths = new List<string>();

                    for (int i = 0; i < _thumbsCount; i++)
                    {
                        thumbnailsTimes.Add((duration/(_thumbsCount + 1))*(i + 1));
                        thumbnailsFilePaths.Add(Path.Combine(thumbsDirectory, fileHash + "_" + i + ".jpg"));
                    }

                    this.GetVideoThumbnailSeek(file, thumbnailsTimes, thumbnailsFilePaths);


                    if (verbose)
                    {
                        swThumbnails.Stop();
                        Console.WriteLine(swThumbnails.ElapsedMilliseconds + "ms - Thumbnails - " +  Path.GetFileName(file));
                    }
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
