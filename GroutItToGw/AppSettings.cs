﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GroutItToGw
{
    public class AppSettings
    {
        //Fields and Properties------------------------------------------------------------------------------------------------//

        public string InputFolder { get; set; }
        public string OutputFolder { get; set; }

        private int folderScanSeconds;
        public int FolderScanSeconds 
        {
            get { return this.folderScanSeconds; }
            set { this.folderScanSeconds = (value > 1 && value <= 3600) ? value : folderScanSeconds; }
        }

        public string ExtCLICommand { get; set; }
        public string ExtCLICommandArgs { get; set; }

        //Constructors---------------------------------------------------------------------------------------------------------//

        public AppSettings()
        {
            this.InputFolder = @"C:\_temp\inputFolder";
            this.InputFolder = @"C:\_temp\outputFolder";
            this.folderScanSeconds = 10;

        }

    }
}
