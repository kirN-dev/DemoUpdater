using System;

namespace DemoUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            Updater updater = new Updater();

            updater.UpdateProgressChanged += Updater_UpdateProgressChanged;

            updater.UpdateFiles();
            
            //updater.CreatePatchFile();

        }

        private static void Updater_UpdateProgressChanged(object sender, UpdateFileEventArgs e)
        {
            Console.Clear();
            Console.WriteLine("{0}/{1}",e.CureentProgress, e.TotalCount);
        }
    }
}