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

        public static void DownloadUpdateFile(string root, string savePath)
        {
            string server = Configuration.ServerPath;
            string source = server + root;
            _webClient.DownloadFile(source, savePath);
        }
    }
}
