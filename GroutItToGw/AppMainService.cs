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

        public event EventHandler Progress;
        public string ProgressMessage {get; private set;}

        private AppSettings appSettings;

        private CancellationTokenSource cTokenSource;

        //Constructors---------------------------------------------------------------------------------------------------------//
        public AppMainService(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        //Methods--------------------------------------------------------------------------------------------------------------//

        //public method to fire up FilesScanningAsync()
        public void StartFilesScanning()
        {
            cTokenSource = new CancellationTokenSource();
            FilesScanningAsync(cTokenSource.Token, OnProgress);
        }

        public void StopFilesScanning()
        {
            if (cTokenSource != null && !cTokenSource.IsCancellationRequested)
            {
                cTokenSource.Cancel();
            }
        }

        
        //main async method running infinite loop to scan files
        private Task FilesScanningAsync(CancellationToken token, Func<string,string> progressCallback)
        {
            return Task.Factory.StartNew(() =>
            {
                OnProgress("Scanning for files started...");
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

                OnProgress("Scanning for files stopped.");
            });
        }

        //write log entry to the file GroutItToGwLog.txt
        protected virtual void WriteToLog(string logEntry)
        {
            using (var logWriter = File.AppendText("GroutItToGwLog.txt"))
            {
                logWriter.WriteLine(String.Format("{0:yyyy-MM-dd HH:mm:ss} : {1}", DateTime.Now, logEntry));
            }
        }

        // trigger Progress event and write to log
        protected virtual string OnProgress(string progressMessage)
        {
            this.ProgressMessage = progressMessage;
            WriteToLog(progressMessage);
            if (Progress != null) { Progress(this, EventArgs.Empty); }
            return progressMessage;
        }

        //Helpers--------------------------------------------------------------------------------------------------------------//
        #region Helpers

        




        #endregion
                
    }
}

