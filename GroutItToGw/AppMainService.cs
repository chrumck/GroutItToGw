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
        public event ProgressEventHandler ScanCancelled;
        
        private AppSettings appSettings;
        private AppUtilities appUtilities;
        private FileConvertService fileConvertService;
        
        private CancellationTokenSource cTokenSource;

        //Constructors---------------------------------------------------------------------------------------------------------//
        public AppMainService(AppSettings appSettings, AppUtilities appUtilities, FileConvertService fileConvertService)
        {
            this.appSettings = appSettings;
            this.appUtilities = appUtilities;
            this.fileConvertService = fileConvertService;
            
            this.ScanIsRunning = false;
        }

        //Methods--------------------------------------------------------------------------------------------------------------//

        //public method to load settings from file
        public void ReadAppSettingsFromXml()
        {
            appSettings.ReadFromXML();
        }

        //public method to fire up FilesScanningAsync()
        public void StartFilesScan()
        {
            if (!Directory.Exists(appSettings.InputFolder))
            {
                OnScanCancelled("Input folder '" + appSettings.InputFolder + "' not found");
                return;
            }

            if (!Directory.Exists(appSettings.ProcessedFolder))
            {
                OnScanCancelled("Processed folder '" + appSettings.ProcessedFolder + "' not found");
                return;
            }

            if (!Directory.Exists(appSettings.ErrorFolder))
            {
                OnScanCancelled("Error folder '" + appSettings.ErrorFolder + "' not found");
                return;
            } 

            if (!Directory.Exists(appSettings.OutputFolder))
            {
                OnScanCancelled("Output folder '" + appSettings.OutputFolder + "' not found");
                return;
            } 

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
        

        //Helpers--------------------------------------------------------------------------------------------------------------//
        #region Helpers
        
        // trigger ScanProgress event and write to log
        protected virtual void OnScanProgress(string progressMessage)
        {
            appUtilities.WriteToLog(progressMessage);
            if (ScanProgress != null)
            {
                ScanProgress(this, new AppProgressEventArgs(progressMessage));
            }
        }

        // trigger ScanProgress event, trigger ScanCancelled event and write to log
        protected virtual void OnScanCancelled(string cancelMessage)
        {
            cancelMessage = (!String.IsNullOrEmpty(cancelMessage)) ? cancelMessage : "Scanning for files stopped.";
            appUtilities.WriteToLog(cancelMessage);
            if (ScanCancelled != null) { ScanCancelled(this, new AppProgressEventArgs(cancelMessage)); }
        }

        
        //main async method running infinite loop to scan files
        protected virtual Task FilesScanAsync(CancellationToken token, Action<string> progressCallback, Action<string> cancelCallback)
        {
            return Task.Factory.StartNew(() =>
            {
                progressCallback("Scanning for files started.");
                ScanIsRunning = true;
                var timeStamp = new DateTime();

                while (!token.IsCancellationRequested)
                {
                    timeStamp = DateTime.Now;

                    if ((timeStamp.Hour * 3600 + timeStamp.Minute * 60 + timeStamp.Second) % appSettings.FolderScanSeconds != 0)
                    {
                        Thread.Sleep(600);
                        continue;
                    }
                    var inpuDirectoryInfo = new DirectoryInfo(appSettings.InputFolder);
                    var inputFileInfoList = inpuDirectoryInfo.EnumerateFiles("*.csv", SearchOption.TopDirectoryOnly);
                    foreach (var inputFileInfo in inputFileInfoList)
                    {
                        var inputFilePath = appSettings.InputFolder + @"\" + inputFileInfo.Name;
                        var outputFilePath = (appSettings.OutputFolder + @"\" + inputFileInfo.Name);
                        outputFilePath = outputFilePath.Remove(outputFilePath.Length - 3) + "txt";
                        var processedFilePath = appSettings.ProcessedFolder + @"\" + inputFileInfo.Name;
                        var errorFilePath = appSettings.ErrorFolder + @"\" + inputFileInfo.Name;

                        try
                        {
                            progressCallback("Processing file " + inputFileInfo.Name);
                            var outputFileData = fileConvertService.ConvertGroutItToGw(inputFileInfo.Name);
                            File.WriteAllLines(outputFilePath, outputFileData);

                            if (File.Exists(processedFilePath)) { File.Delete(processedFilePath); }
                            File.Move(inputFilePath, processedFilePath);
                        }
                        catch (Exception exception)
                        {
                            
                            progressCallback("Error processing file " + inputFileInfo.Name + 
                                " :" + exception.GetBaseException().Message);

                            if (File.Exists(errorFilePath)) { File.Delete(errorFilePath); }
                            File.Move(inputFilePath, errorFilePath);
                        }                        
                    }
                    Thread.Sleep(1000);
                }
                ScanIsRunning = false;
                cancelCallback("");
            });
        }
        

        #endregion
                
    }
}

