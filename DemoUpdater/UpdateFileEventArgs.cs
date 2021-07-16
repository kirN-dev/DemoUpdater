using System;
using System.Collections.Generic;
using System.Text;

namespace DemoUpdater
{
    public class UpdateFileEventArgs : EventArgs
    {
        public UpdateFileEventArgs(int totalCount, int currentProgress)
        {
            TotalCount = totalCount;
            CureentProgress = currentProgress;
        }

        public int TotalCount { get; private set; }
        public int CureentProgress { get; private set; }
    }
}
