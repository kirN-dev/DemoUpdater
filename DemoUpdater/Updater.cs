using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

namespace DemoUpdater
{
    public class Updater
    {
        public string GameVersion { get; }
        public string GamePath { get; }
        public string PatchPath { get; }
        public string ServerPath { get; }
        public string UpdateRoot { get; }

        public event EventHandler<UpdateFileEventArgs> UpdateProgressChanged;

        public Updater()
        {
            GameVersion = Configuration.GameVersion;
            GamePath = Configuration.GamePath;
            PatchPath = Path.Combine(GamePath, "patch.xml");
            ServerPath = Configuration.ServerPath;
            UpdateRoot = Configuration.UpdateRoot;

            if (!File.Exists(PatchPath))
                Directory.CreateDirectory(GamePath);

            Downloader.DownloadFile(Path.Combine(ServerPath, "patch.xml"), PatchPath);
        }

        public void UpdateFiles()
        {
            XmlDocument xDocLoad = new XmlDocument();
            xDocLoad.Load(PatchPath);

            XmlElement xRoot = xDocLoad.DocumentElement;

            ChangeGameVersion(xRoot);


            string GetSourcePath(string patchSavePath)
            {
                int index = patchSavePath.LastIndexOf(UpdateRoot);
                return patchSavePath[(index + UpdateRoot.Length)..];
            }

            int currentProgress = 0;
            foreach (XmlNode xnode in xRoot)
            {
                foreach (XmlNode childNode in xnode.ChildNodes)
                {
                    UpdateProgressChanged?.Invoke(this, new UpdateFileEventArgs(xnode.ChildNodes.Count, currentProgress));

                    if (!AttributeExists(childNode))
                        continue;

                    XmlNode attrPath = childNode.Attributes.GetNamedItem("path");
                    XmlNode attrLenght = childNode.Attributes.GetNamedItem("length");
                    XmlNode attrHash = childNode.Attributes.GetNamedItem("hash");

                    string rootPatchPath = GetSourcePath(attrPath.Value);
                    string localFilePath = GamePath + rootPatchPath;
                    string localDirectoryPath = Directory.GetParent(localFilePath).FullName;

                    FileInfo fileInfo = new FileInfo(localFilePath);

                    if (!File.Exists(localFilePath) ||
                        attrLenght.Value != fileInfo.Length.ToString() ||
                        attrHash.Value != FormatHash(GetHash(fileInfo)))
                    {
                        Directory.CreateDirectory(localDirectoryPath);

                        Downloader.DownloadUpdateFile(rootPatchPath, localFilePath);
                    }

                    currentProgress++;
                }
            }
        }

        private static void ChangeGameVersion(XmlElement xRoot)
        {
            string newVersion = xRoot.GetAttribute("version");
            string settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            string json;

            using (StreamReader reader = new StreamReader(settingsPath))
            {
                json = reader.ReadToEnd().Replace(Configuration.GameVersion, newVersion);
            }

            using (StreamWriter writer = new StreamWriter(settingsPath))
            {
                writer.WriteLine(json);
            }
        }

        private bool CheckVersion(XmlElement xRoot)
        {
            return xRoot.GetAttribute("version") == GameVersion;
        }

        private bool AttributeExists(XmlNode childNode)
        {
            return childNode.Attributes.Count > 0;
        }

        private byte[] GetHash(FileInfo fileInfo)
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

        public void CreatePatchFile()
        {
            var directory = new DirectoryInfo(GamePath);

            IEnumerable<FileHash> data = GetCurrentFileHash(directory);

            if (!float.TryParse(GameVersion, out float gameVerision))
            {
                throw new FormatException();
            }

            string newGameVersion = (gameVerision + .1f).ToString();

            XDocument xDocSave = CreateXmlDocument(newGameVersion, data);

            using (StreamWriter writer = new StreamWriter(PatchPath))
            {
                xDocSave.Document.Save(writer);
            }
        }

        private XDocument CreateXmlDocument(string gameVersion, IEnumerable<FileHash> listFiles)
        {
            var declaration = new XDeclaration("1.0", "UTF-8", null);

            var data = new XElement("data", listFiles.Select(x =>
                        new XElement("file",
                            new XAttribute("path", x.File.FullName),
                            new XAttribute("length", x.File.Length),
                            new XAttribute("hash", FormatHash(x.Hash)))
                            ));

            var updateData = new XElement("game", new XAttribute("version", gameVersion), new XElement(data));

            return new XDocument(declaration, updateData);
        }

        private string FormatHash(byte[] hashArray)
        {
            return string.Join("", hashArray.Select(b => b.ToString("X2")).ToArray());
        }

        private IEnumerable<FileHash> GetCurrentFileHash(DirectoryInfo directory)
        {
            return from file in directory.EnumerateFiles("*", SearchOption.AllDirectories)
                   let hash = GetHash(file)
                   select new FileHash { File = file, Hash = hash };
        }
    }
}
