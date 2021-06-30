using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

namespace HashDirectory
{
    class Program
    {
        static void Main(string[] args)
        {
            string version = "0.1";
            string workPath = @"C:\Users\Silvan\Desktop\TestProject\WorkPlace";
            string patchPath = AppDomain.CurrentDomain.BaseDirectory + "\\patch.xml";

            WebClient webClient = new WebClient();

            webClient.DownloadFile(@"https://raw.githubusercontent.com/kirN-dev/WorkPlace/master/patch.xml", patchPath);

            CheckFiles(patchPath);

            //CreatePatchFile(version, workPath, patchPath);

        }
        private static byte[] GetHash(FileInfo fileInfo)
        {
            using (var hasher = SHA256.Create())
            {
                using (var stream = fileInfo.OpenRead())
                {
                    return hasher.ComputeHash(stream);
                };
            }
        }

        private static void CreatePatchFile(string version, string workPath, string patchPath)
        {
            var directory = new DirectoryInfo(workPath);

            

            var infos = from file in directory.EnumerateFiles("*", SearchOption.AllDirectories)
                        let hash = GetHash(file)
                        select new { File = file, Hash = hash };

            var declaration = new XDeclaration("1.0", "UTF-8", null);

            var data = new XElement("data", infos.Select(x =>
                        new XElement("file",
                            new XAttribute("path", x.File.FullName),
                            new XAttribute("length", x.File.Length),
                            new XAttribute("hash", string.Join("", x.Hash.Select(b => b.ToString("X2")).ToArray())))
                            ));

            var gameVerison = new XElement("game", new XAttribute("version", version), new XElement(data));

            XDocument xDocSave = new XDocument(declaration, gameVerison);

            using (StreamWriter writer = new StreamWriter(patchPath))
            {
                xDocSave.Document.Save(writer);
            }
        }

        private static void CheckFiles(string patchPath)
        {
            XmlDocument xDocLoad = new XmlDocument();
            xDocLoad.Load(patchPath);
            // получим корневой элемент
            XmlElement xRoot = xDocLoad.DocumentElement;
            // обход всех узлов в корневом элементе
            foreach (XmlNode xnode in xRoot)
            {
                foreach (XmlNode childNode in xnode.ChildNodes)
                {
                    if (childNode.Attributes.Count > 0)
                    {
                        XmlNode attrPath = childNode.Attributes.GetNamedItem("path");

                        if (attrPath == null)
                            return;

                        if (!File.Exists(attrPath.Value))
                        {
                            try
                            {
                                WebClient webClient = new WebClient();

                                string directoryName = Path.GetDirectoryName(attrPath.Value);
                                Directory.CreateDirectory(directoryName);
                                string fileName = Path.GetFileName(attrPath.Value);

                                int index = attrPath.Value.LastIndexOf("WorkPlace");
                                string path = attrPath.Value.Substring(index + "WorkPlace".Length);

                                webClient.DownloadFile(@"https://raw.githubusercontent.com/kirN-dev/WorkPlace/master" + "/" + path, attrPath.Value);
                            }
                            catch (Exception)
                            {

                                throw;
                            }

                        }

                        XmlNode attrLength = childNode.Attributes.GetNamedItem("length");
                        XmlNode attrHash = childNode.Attributes.GetNamedItem("hash");
                        FileInfo fileInfo = new FileInfo(attrPath.Value);

                        if (attrLength.Value != fileInfo.Length.ToString())
                        {
                            try
                            {
                                WebClient webClient = new WebClient();

                                string directoryName = Path.GetDirectoryName(attrPath.Value);
                                Directory.CreateDirectory(directoryName);
                                string fileName = Path.GetFileName(attrPath.Value);

                                int index = attrPath.Value.LastIndexOf("WorkPlace");
                                string path = attrPath.Value.Substring(index + "WorkPlace".Length);

                                webClient.DownloadFile(@"https://raw.githubusercontent.com/kirN-dev/WorkPlace/master" + "/" + path, attrPath.Value);
                            }
                            catch (Exception)
                            {

                                throw;
                            }
                        }
                        var p = GetHash(fileInfo).Select(b => b.ToString("X2")).ToArray();
                        if (attrHash.Value != string.Join("", p))
                        {
                            try
                            {
                                WebClient webClient = new WebClient();

                                string directoryName = Path.GetDirectoryName(attrPath.Value);
                                Directory.CreateDirectory(directoryName);
                                string fileName = Path.GetFileName(attrPath.Value);

                                int index = attrPath.Value.LastIndexOf("WorkPlace");
                                string path = attrPath.Value.Substring(index + "WorkPlace".Length);

                                webClient.DownloadFile(@"https://raw.githubusercontent.com/kirN-dev/WorkPlace/master" + "/" + path, attrPath.Value);
                            }
                            catch (Exception)
                            {

                                throw;
                            }
                        }
                    }
                }
            }
        }
    }
}