using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VideoCatalog.Web.Models
{
    public class CatalogModel
    {
        public CatalogModel()
        {
            Items = new List<DirectoryItemModel>();
            Directories = new List<DirectoryItemModel>();
        }

        public bool HasError { get; set; }

        public string Error { get; set; }
        
        public List<DirectoryItemModel> Directories { get; set; }

        public List<DirectoryItemModel> Items { get; set; }
    }
}