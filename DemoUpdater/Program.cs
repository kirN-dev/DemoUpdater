using System;

namespace DemoUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            Updater updater = new Updater();

            updater.UpdateFiles();
            
            //updater.CreatePatchFile();

        }
    }
}