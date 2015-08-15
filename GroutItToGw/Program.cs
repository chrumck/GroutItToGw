﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GroutItToGw
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var appSettings = new AppSettings();
            var appUtilities = new AppUtilities(appSettings);
            var fileConvertService = new FileConvertService(appSettings, appUtilities);
            var appMainService = new AppMainService(appSettings, appUtilities, fileConvertService);
            Application.Run(new FormMain(appMainService));
        }
    }
}
