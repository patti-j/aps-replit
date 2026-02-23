using System;

namespace PT.ServerManagerSharedLib.Definitions
{
    public class DownloadStateProcessing
    {
        public DateTime DownloadStartedTime;
        public DateTime DownloadProgressUpdateTime;
        public string UserState;
        public long BytesReceived;
        public long TotalBytesToReceive;
        public int ProgressPercentage;
        public bool Downloaded;
    }
}
