﻿using System;
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
        private GroutItToGwService fileConvertService;
        
        private CancellationTokenSource cTokenSource;

        //Constructors---------------------------------------------------------------------------------------------------------//
        public AppMainService(AppSettings appSettings, AppUtilities appUtilities, GroutItToGwService fileConvertService)
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
            var checkFoldersMessage = CheckFoldersIfExist();
            if (!String.IsNullOrEmpty(checkFoldersMessage))
            {
                OnScanCancelled(checkFoldersMessage);
                return;
            }
            cTokenSource = new CancellationTokenSource();
            FilesScanAsync();
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
      
        //main async method running infinite loop to scan files
        protected virtual Task FilesScanAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                OnScanProgress("Scanning for files started.");
                ScanIsRunning = true;
                var timeStamp = new DateTime();

                while (!cTokenSource.Token.IsCancellationRequested)
                {
                    timeStamp = DateTime.Now;
                    if ((timeStamp.Hour * 3600 + timeStamp.Minute * 60 + timeStamp.Second) % appSettings.FolderScanSeconds != 0)
                    {
                        Thread.Sleep(600);
                        continue;
                    }
                    DoWorkOnInterval();
                    Thread.Sleep(1000);
                }

                ScanIsRunning = false;
                OnScanCancelled("");
            });
        }

        //the actual task to be run on an interval
        protected virtual void DoWorkOnInterval()
        {
            var checkFoldersMessage = CheckFoldersIfExist();
            if (!String.IsNullOrEmpty(checkFoldersMessage))
            {
                OnScanProgress(checkFoldersMessage);
                return;
            }

            IEnumerable<FileInfo> inputFileInfoList;
            try
            {
                inputFileInfoList = new DirectoryInfo(appSettings.InputFolder)
                    .EnumerateFiles("*.csv", SearchOption.TopDirectoryOnly);
            }
            catch (Exception exception)
            {
                OnScanProgress("Error scanning folder: " + exception.GetBaseException().Message);
                return;
            }
            foreach (var inputFileInfo in inputFileInfoList)
            {
                var inputFilePath = appSettings.InputFolder + @"\" + inputFileInfo.Name;
                var outputFilePath = (appSettings.OutputFolder + @"\" + 
                    inputFileInfo.Name.Remove(inputFileInfo.Name.Length - 3) + "txt");
                var processedFilePath = appSettings.ProcessedFolder + @"\" + inputFileInfo.Name;
                var errorFilePath = appSettings.ErrorFolder + @"\" + inputFileInfo.Name;

                string[] inputFileRows;
                try
                {
                    inputFileRows = File.ReadAllLines(inputFilePath);
                }
                catch (Exception exception)
                {
                    OnScanProgress("Error reading file: " + exception.GetBaseException().Message);
                    continue;
                }

                OnScanProgress("Processing file " + inputFileInfo.Name);
                bool processSucceeded = false;
                try
                { 
                    var outputFileData = fileConvertService.ConvertGroutItToGw(inputFileInfo.Name, inputFileRows);
                    File.WriteAllLines(outputFilePath, outputFileData);

                    if (File.Exists(processedFilePath)) { File.Delete(processedFilePath); }
                    File.Move(inputFilePath, processedFilePath);

                    processSucceeded = true;
                }
                catch (Exception exception)
                {
                    OnScanProgress("Error processing file " + inputFileInfo.Name +
                        " :" + exception.GetBaseException().Message);
                }

                if (!processSucceeded)
                {
                    try
                    {
                        if (File.Exists(errorFilePath)) { File.Delete(errorFilePath); }
                        File.Move(inputFilePath, errorFilePath);
                    }
                    catch { }    
                }
            }

        }

        //check if folders exist
        protected virtual string CheckFoldersIfExist()
        {
            if (!Directory.Exists(appSettings.InputFolder))
                { return "'Input' folder '" + appSettings.InputFolder + "' not found"; }
            if (!Directory.Exists(appSettings.ProcessedFolder))
                { return "'Processed' folder '" + appSettings.ProcessedFolder + "' not found"; }
            if (!Directory.Exists(appSettings.ErrorFolder)) 
                { return "'Error' folder '" + appSettings.ErrorFolder + "' not found"; }
            if (!Directory.Exists(appSettings.OutputFolder))
                { return "'Output' folder '" + appSettings.OutputFolder + "' not found"; } 
            return String.Empty;
        }

        // trigger ScanProgress event and write to log
        protected virtual void OnScanProgress(string progressMessage)
        {
            appUtilities.WriteToLog(progressMessage);
            if (ScanProgress != null) { ScanProgress(this, new AppProgressEventArgs(progressMessage));}
        }

        // trigger ScanProgress event, trigger ScanCancelled event and write to log
        protected virtual void OnScanCancelled(string cancelMessage)
        {
            cancelMessage = (!String.IsNullOrEmpty(cancelMessage)) ? cancelMessage : "Scanning for files stopped.";
            appUtilities.WriteToLog(cancelMessage);
            if (ScanCancelled != null) { ScanCancelled(this, new AppProgressEventArgs(cancelMessage)); }
        }
        

        #endregion
                
    }
}

