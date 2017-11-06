using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VideoCatalog.Core
{
    public class Helper
    {
        public static List<string> VideoExtensions;

        static Helper()
        {
             VideoExtensions = new List<string>() { ".flv", ".mp4", ".avi", ".wmv", ".m4v", ".mpg", ".mov", ".rm", ".webm", ".3gp" };
        }

        public static bool HasSupportedExtension(string file)
        {
            foreach (var file_format in VideoExtensions)
            {
                if (!String.IsNullOrEmpty(file) && file.ToLower().EndsWith(file_format))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
