using System.IO;
using System.Net;

namespace DemoUpdater
{
    class Downloader
    {
        private static readonly WebClient _webClient = new WebClient();

        public static void DownloadFile(string adress, string savePath)
        {
            _webClient.DownloadFile(adress, savePath);
        }

        public static void DownloadUpdateFile(string root, string savePath, string serverPath)
        {
            string source = Configuration.ServerPath + root;
            _webClient.DownloadFile(source, savePath);
        }

        private static string GetSourceFile(string adress, string savePath)
        {
            //TODO: Исправить
            int index = savePath.LastIndexOf("WorkPlace");
            string path = savePath[(index + "WorkPlace".Length)..];
            return adress + path;
        }
    }
}
