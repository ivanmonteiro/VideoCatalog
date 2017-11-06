using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VideoCatalog.Web.Models
{
    public class DirectoryItemModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string ThumbnailPath { get; set; }
    }
}