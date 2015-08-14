using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GroutItToGw
{
    public partial class FormMain : Form
    {
        //Fields and Properties------------------------------------------------------------------------------------------------//

        private AppMainService appMainService;

        //Constructors---------------------------------------------------------------------------------------------------------//
        public FormMain(AppMainService appMainService)
        {
            this.appMainService = appMainService;
            
            InitializeComponent();
        }

        //Methods--------------------------------------------------------------------------------------------------------------//

        private void BtnStart_Click(object sender, EventArgs e)
        {
            BtnStart.Enabled = false;
            BtnStop.Enabled = true;

            appMainService.StartFilesScanning();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            BtnStart.Enabled = true;
            BtnStop.Enabled = false;

            appMainService.StopFilesScanning();
        }


        //Helpers--------------------------------------------------------------------------------------------------------------//
        #region Helpers

        #endregion
        
    }
}
