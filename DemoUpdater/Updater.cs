﻿using System;
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

        public Updater()
        {
            GameVersion = Configuration.GameVersion;
            GamePath = Configuration.GamePath;
            PatchPath = Path.Combine(GamePath, "patch.xml");
            ServerPath = Configuration.ServerPath;

            if (!File.Exists(PatchPath))
                Directory.CreateDirectory(GamePath);

            Downloader.DownloadFile(Path.Combine(ServerPath, "patch.xml"), PatchPath);
        }

        public void UpdateFiles()
        {
            XmlDocument xDocLoad = new XmlDocument();
            xDocLoad.Load(PatchPath);

            XmlElement xRoot = xDocLoad.DocumentElement;

            if (CheckVersion(xRoot))
            {
                return;
            }

            //TODO: Реализовать при сохранении, от сюда убрать
            string GetSourcePath(string savePath)
            {
                //TODO: Исправить
                int index = savePath.LastIndexOf("WorkPlace");
                return savePath[(index + "WorkPlace".Length)..];
            }

            foreach (XmlNode xnode in xRoot)
            {
                foreach (XmlNode childNode in xnode.ChildNodes)
                {
                    if (!AttributeExists(childNode))
                        continue;

                    //TODO:
                    #region Поправить часть кода
                    XmlNode attrPath = childNode.Attributes.GetNamedItem("path");

                    if (attrPath == null)
                        continue;

                    //Исправить
                    var temp2 = GamePath + GetSourcePath(attrPath.Value);
                    var temp1 = temp2.Replace(Path.GetFileName(attrPath.Value), "");
                    if (!File.Exists(temp2))
                    {
                        Directory.CreateDirectory(temp1);

                        Downloader.DownloadUpdateFile(ServerPath, temp2);
                        continue;
                    }

                    XmlNode attrLenght = childNode.Attributes.GetNamedItem("lenght");

                    if (attrLenght == null)
                        continue;

                    FileInfo fileInfo = new FileInfo(attrPath.Value);

                    if (attrLenght.Value != fileInfo.Length.ToString())
                    {
                        Downloader.DownloadUpdateFile(ServerPath, attrPath.Value);
                        continue;
                    }

                    XmlNode attrHash = childNode.Attributes.GetNamedItem("hash");

                    if (attrHash == null)
                        continue;

                    if (attrHash.Value != FormatHash(GetHash(fileInfo)))
                    {
                        Downloader.DownloadUpdateFile(ServerPath, attrPath.Value);
                    }
                    #endregion
                }
            }
        }

        private bool CheckVersion(XmlElement xRoot)
        {
            return xRoot.GetAttribute("version") != GameVersion;
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