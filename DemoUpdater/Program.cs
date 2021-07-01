using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

namespace DemoUpdater
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
        class FileHash
        {
            public FileInfo File { get; set; }
            public byte[] Hash { get; set; }
        }

        private static void CreatePatchFile(string gameVersion, string workPath, string patchPath)
        {
            var directory = new DirectoryInfo(workPath);

            IEnumerable<FileHash> data = GetCurrentFileHash(directory);

            XDocument xDocSave = CreateXmlDocument(gameVersion, data);

            using (StreamWriter writer = new StreamWriter(patchPath))
            {
                xDocSave.Document.Save(writer);
            }
        }

        private static XDocument CreateXmlDocument(string gameVersion, IEnumerable<FileHash> listFiles)
        {
            var declaration = new XDeclaration("1.0", "UTF-8", null);

            var data = new XElement("data", listFiles.Select(x =>
                        new XElement("file",
                            new XAttribute("path", x.File.FullName),
                            new XAttribute("length", x.File.Length),
                            new XAttribute("hash", FormatHash(x.Hash)))
                            ));

            var gameVerison = new XElement("game", new XAttribute("version", gameVersion), new XElement(data));

            return new XDocument(declaration, gameVerison);
        }

        private static string FormatHash(byte[] hashArray)
        {
            return string.Join("", hashArray.Select(b => b.ToString("X2")).ToArray());
        }

        private static IEnumerable<FileHash> GetCurrentFileHash(DirectoryInfo directory)
        {
            return from file in directory.EnumerateFiles("*", SearchOption.AllDirectories)
                   let hash = GetHash(file)
                   select new FileHash { File = file, Hash = hash };
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

                                int index = attrPath.Value.LastIndexOf("WorkPlace");
                                string path = attrPath.Value[(index + "WorkPlace".Length)..];

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

                                int index = attrPath.Value.LastIndexOf("WorkPlace");
                                string path = attrPath.Value.Substring(index + "WorkPlace".Length);

                                webClient.DownloadFile(@"https://raw.githubusercontent.com/kirN-dev/WorkPlace/master" + "/" + path, attrPath.Value);
                            }
                            catch (Exception)
                            {

                                throw;
                            }
                        }
                        var hash = GetHash(fileInfo);
                        if (attrHash.Value != FormatHash(hash))
                        {
                            try
                            {
                                WebClient webClient = new WebClient();

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