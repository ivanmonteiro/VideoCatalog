using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Xml;
using VideoCatalog.Core;
using VideoCatalog.Web.Models;

namespace VideoCatalog.Web.Controllers
{
    public class HomeController : Controller
    {
        private static string _baseDir;
        private static string _thumbsDirName = ".VideoCache";

        static HomeController()
        {
            string appFolder = AppDomain.CurrentDomain.BaseDirectory;
            var info = new DirectoryInfo(appFolder);
            string configFile = Path.Combine(info.Parent.FullName, "config.xml");
            if (System.IO.File.Exists(configFile))
            {
                ReadConfig(configFile);
            }
            else
            {
                ReadConfig(Path.Combine(appFolder, "config.xml"));
            }
        }

        private static void ReadConfig(string config_file)
        {
            using (var xmlFileStream = new FileStream(config_file, FileMode.Open))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlFileStream);
                XmlNodeList nodes = doc.GetElementsByTagName("folder");
                string pasta = nodes.Item(0).InnerText;
                _baseDir = pasta.Trim();
            }
        }

        public HomeController() { }

        public ActionResult Index()
        {
            return RedirectToAction("Catalog");
        }
        
        public ActionResult Catalog(string dir)
        {
            var model = CreateModel(dir);
            return View("Catalog", model);
        }

        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }
        
        private CatalogModel CreateModel(string relative_dir)
        {
            CatalogModel model = new CatalogModel();

            if (relative_dir == null)
                relative_dir = "";

            if (!Directory.Exists(Path.Combine(_baseDir, relative_dir)))
            {
                model.HasError = true;
                model.Error = "Directory not found or access forbidden.";
                return model;
            }

            var files = Directory.EnumerateFiles(Path.Combine(_baseDir, relative_dir));
            //string relative_dir = MakeRelativePath(_baseDir, dir);

            foreach (var file in files)
            {
                if (Helper.HasSupportedExtension(file))
                {
                    string fileName = Path.GetFileName(file);
                    string fileHash = String.Format("{0:X}", Path.GetFileName(file).GetHashCode());

                    model.Items.Add(new DirectoryItemModel()
                        {
                            Name = fileName,
                            Path = Path.Combine(relative_dir, Path.GetFileName(file)),
                            ThumbnailPath = Path.Combine(relative_dir, _thumbsDirName, fileHash)
                        });
                }
            }

            var directories = Directory.EnumerateDirectories(Path.Combine(_baseDir, relative_dir));

            string parent = Directory.GetParent(Path.Combine(_baseDir, relative_dir)).FullName;

            if ((parent).Contains(_baseDir.TrimEnd('\\')) && !String.IsNullOrEmpty(relative_dir))
            {
                //string relative_parent = MakeRelativePath(_baseDir, parent);
                string relative_parent = parent.Replace(_baseDir.TrimEnd('\\'), "").TrimStart('\\');
                model.Directories.Add(new DirectoryItemModel() { Name = "..", Path = relative_parent });    
            }

            foreach (var directory in directories)
            {
                if (!directory.EndsWith(_thumbsDirName))
                {
                    var currentChildDir = new DirectoryInfo(directory);
                    var childThumbsFolders = currentChildDir.GetDirectories(_thumbsDirName, SearchOption.AllDirectories);
                    if (currentChildDir != null && childThumbsFolders.Length > 0)
                    {
                        model.Directories.Add(new DirectoryItemModel()
                        {
                            Name = Path.GetFileName(directory),
                            Path = Path.Combine(relative_dir, Path.GetFileName(directory))
                        });    
                    }
                }
            }

            return model;
        }

        public ActionResult GetImage(string path)
        {
            string server_path = Path.Combine(_baseDir, path + ".jpg");
            return base.File(server_path, "image/jpeg");
        }

        [HttpPost]
        public ActionResult OpenVideo(string path)
        {
            string decodedPath = System.Uri.UnescapeDataString(path);
            string fullPath = Path.Combine(_baseDir, decodedPath);
            Process.Start(fullPath);
            return new EmptyResult();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
