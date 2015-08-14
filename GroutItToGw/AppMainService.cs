using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GroutItToGw
{
    public class AppMainService
    {
        //Fields and Properties------------------------------------------------------------------------------------------------//
        
        public bool ScanIsRunning { get; private set; }
        
        public delegate void ProgressEventHandler(object sender, AppProgressEventArgs args);
        public event ProgressEventHandler ScanProgress;
        public event EventHandler ScanCancelled;
        
        private AppSettings appSettings;
        private CancellationTokenSource cTokenSource;

        //Constructors---------------------------------------------------------------------------------------------------------//
        public AppMainService(AppSettings appSettings)
        {
            this.ScanIsRunning = false;
            this.appSettings = appSettings;
            this.appSettings.ReadFromXML();
        }

        //Methods--------------------------------------------------------------------------------------------------------------//

        //main async method running infinite loop to scan files
        private Task FilesScanAsync(CancellationToken token, Action<string> progressCallback, Action cancelCallback)
        {
            return Task.Factory.StartNew(() =>
            {
                progressCallback("Scanning for files started.");
                ScanIsRunning = true;
                var timeStamp = new DateTime();

                while (!token.IsCancellationRequested)
                {
                    timeStamp = DateTime.Now;
                    
                    if ((timeStamp.Hour * 3600 + timeStamp.Minute * 60 + timeStamp.Second) % appSettings.FolderScanSeconds != 0 )
                    {
                        Thread.Sleep(600);
                        continue; 
                    }

                    progressCallback("testMessage");

                    Thread.Sleep(1000);
                }
                ScanIsRunning = false;
                cancelCallback();
            });
        }

        //public method to fire up FilesScanningAsync()
        public void StartFilesScan()
        {
            cTokenSource = new CancellationTokenSource();
            FilesScanAsync(cTokenSource.Token, OnScanProgress, OnScanCancelled);
        }

        //public method to stop FilesScanningAsync()
        public void StopFilesScan()
        {
            if (cTokenSource != null && !cTokenSource.IsCancellationRequested)
            {
                cTokenSource.Cancel();
            }
        }

        // trigger ScanProgress event and write to log
        protected virtual void OnScanProgress(string progressMessage)
        {
            WriteToLog(progressMessage);
            if (ScanProgress != null) { 
                ScanProgress(this, new AppProgressEventArgs(progressMessage));
            }
        }

        // trigger ScanProgress event, trigger ScanCancelled event and write to log
        protected virtual void OnScanCancelled()
        {
            var cancelMessage = "Scanning for files stopped.";
            WriteToLog(cancelMessage);
            if (ScanProgress != null) { ScanProgress(this, new AppProgressEventArgs(cancelMessage)); }
            if (ScanCancelled != null) { ScanCancelled(this, EventArgs.Empty); }
        }

        //write log entry to the file GroutItToGwLog.txt
        protected virtual void WriteToLog(string logEntry)
        {
            using (var logWriter = File.AppendText("GroutItToGwLog.txt"))
            {
                logWriter.WriteLine(String.Format("{0:yyyy-MM-dd HH:mm:ss} : {1}", DateTime.Now, logEntry));
            }
        }

        //Helpers--------------------------------------------------------------------------------------------------------------//
        #region Helpers

        




        #endregion
                
    }
}

