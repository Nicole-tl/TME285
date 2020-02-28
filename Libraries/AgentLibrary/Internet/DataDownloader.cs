using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuxiliaryLibrary;
using AgentLibrary.EventArgsClasses;
using AgentLibrary.Memories;

namespace AgentLibrary.Internet
{
    public abstract class DataDownloader
    {
        private const int DEFAULT_MILLISECOND_INTERVAL = 10000;

        protected Thread downloadThread = null;
        protected Boolean runRepeatedly = false;
        protected double interval = (double)DEFAULT_MILLISECOND_INTERVAL / 1000;
        protected Boolean running = false;

        public event EventHandler<DataItemListEventArgs> DownloadComplete = null;

        private void OnDownloadComplete(List<DataItem> dataItemList)
        {
            if (DownloadComplete != null)
            {
                EventHandler<DataItemListEventArgs> handler = DownloadComplete;
                DataItemListEventArgs e = new DataItemListEventArgs(dataItemList);
                handler(this, e);
            }
        }

        public void Start()
        {
            if (!running)
            {
                running = true;
                downloadThread = new Thread(new ThreadStart(() => DownloadLoop()));
                downloadThread.Start();
            }
        }

        // 20200104
        public void Stop()
        {
            running = false;
            downloadThread.Interrupt(); // This is ugly, but OK ...
        }

        // Note to students: Write this method to handle the download and process the results,
        // returning a single string.
        protected abstract List<DataItem> ProcessDownload();

        private void DownloadLoop()
        {
            int millisecondInterval = (int)Math.Round(1000 * interval);
            while (running)
            {
                List<DataItem> result = ProcessDownload();
                OnDownloadComplete(result);
                if (!runRepeatedly) { running = false; }
                else { Thread.Sleep(millisecondInterval); }
            }
        }

        public Boolean RunRepeatedly
        {
            get { return runRepeatedly; }
            set { runRepeatedly = value; }
        }

        public double Interval
        {
            get { return interval; }
            set { interval = value; }
        }
    }
}
