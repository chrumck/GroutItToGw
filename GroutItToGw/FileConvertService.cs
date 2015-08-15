using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace GroutItToGw
{
    public class FileConvertService
    {
        //Fields and Properties------------------------------------------------------------------------------------------------//

        private AppSettings appSettings;
        private AppUtilities appUtilities;

        private struct Ty_Rec
        {
            public DateTime RDate;
            public string VOL;
            public string PR;
            public string BEB;
        }

        private struct Ty_IF
        {
            public DateTime FDate;
            public string Phase;
            public string Tran;
            public string Fora;
            public string HTranche;
            public IList<Ty_Rec> Recs;
        }

        private Ty_IF myFileObject;

        //Constructors---------------------------------------------------------------------------------------------------------//

        public FileConvertService(AppSettings appSettings, AppUtilities appUtilities)
        {
            this.appSettings = appSettings;
            this.appUtilities = appUtilities;
        }

        //Methods--------------------------------------------------------------------------------------------------------------//

        //GroutToGw - converts file from GroutIt format to GW
        public IList<string> ConvertGroutItToGw(string inputFileName) 
        {
            //reading from file and checking if there is any data
            string[] inputFileRows = File.ReadAllLines(appSettings.InputFolder + @"\" + inputFileName);
            if (inputFileRows.Length < 4) 
                { throw new ArgumentException("File does not seem to contain any data"); }

            //converting file data to 2-dimensional array[i,j] of strings
            var dataArrayWidth = 20;
            string[,] inputFileData = new string[inputFileRows.Length, dataArrayWidth];
            for (int i = 0; i < inputFileRows.Length; i++)
            {
                string[] inputFileColumns = inputFileRows[i].Split(new Char[] { ';' });
                for (int j = 0; j < inputFileColumns.Length; j++)
                {
                    inputFileData[i, j] = inputFileColumns[j];
                }
            }

            //checking if format of inputFileData correct
            checkInpuFileData(inputFileData);
                        
            // Initializing MyFile and setting constant (common) parameters 
            myFileObject = new Ty_IF();
            myFileObject.Recs = new List<Ty_Rec>();
            myFileObject.Phase = inputFileName.Remove(inputFileName.Length - 4).Split(new Char[] { '_' }).Last();
            myFileObject.Fora = inputFileData[1, 2];
            myFileObject.Tran = inputFileData[1, 5];
            myFileObject.HTranche = inputFileData[1, 18];
            myFileObject.FDate = DateTime.ParseExact(inputFileData[1, 12] + " " + inputFileData[1, 13],
                    "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            //Reading records from inputFileData and adding to MyFile.Recs list
            var incTime = 0;
            for (int i = 3; i < inputFileRows.Length; i++)
            {
                if (String.IsNullOrEmpty(inputFileData[i,0])) { continue; }
                var rowSeconds = Convert.ToInt32(double.Parse(inputFileData[i, 0]));

                if (i != 3 && i != inputFileRows.Length - 1 && Math.Abs(rowSeconds - incTime) > 1 && (rowSeconds - incTime) < 0)
                    { continue; }

                myFileObject.Recs.Add(new Ty_Rec()
                {
                    RDate = myFileObject.FDate.AddSeconds(rowSeconds),
                    BEB = inputFileData[i, 3],
                    PR = inputFileData[i, 5],
                    VOL = inputFileData[i, 7]
                });
                incTime += appSettings.OutputFileIntervalMinutes * 60;
            }

            //Creating and writing output file
            var outputFileData = new List<string>();
            foreach (var rec in myFileObject.Recs)
            {
                outputFileData.Add(encodeOutputFileRow(myFileObject.Fora + "_Stage", rec.RDate, myFileObject.Tran, "0"));
                outputFileData.Add(encodeOutputFileRow(myFileObject.Fora + "_Phase", rec.RDate, myFileObject.Phase, "0"));
                outputFileData.Add(encodeOutputFileRow(myFileObject.Fora + "_Vol", rec.RDate, rec.VOL, "0.00"));
                outputFileData.Add(encodeOutputFileRow(myFileObject.Fora + "_Press", rec.RDate, rec.PR, "0.00"));
                outputFileData.Add(encodeOutputFileRow(myFileObject.Fora + "_FlRate", rec.RDate, rec.BEB, "0.00"));
                outputFileData.Add(encodeOutputFileRow(myFileObject.Fora + "_Height", rec.RDate, myFileObject.HTranche, "0"));
            }
            return outputFileData;
        }


        //Helpers--------------------------------------------------------------------------------------------------------------//
        #region Helpers

        //checkInpuFileData
        private void checkInpuFileData(string[,] inputFileData)
        {
            if (inputFileData[0, 2] != "FORA")
            { throw new ArgumentException("Input format not as expected (FORA)"); }
            if (inputFileData[0, 5] != "TRAN")
            { throw new ArgumentException("Input format not as expected (TRAN)"); }
            if (inputFileData[0, 18] != "HTRANCHE[ft]")
            { throw new ArgumentException("Input format not as expected (HTRANCHE)"); }
            if (inputFileData[0, 12] != "DATE")
            { throw new ArgumentException("Input format not as expected (HTRANCHE - DATE)"); }
            if (inputFileData[0, 13] != "TIME")
            { throw new ArgumentException("Input format not as expected (HTRANCHE - TIME)"); }
            if (inputFileData[2, 0] != "TPS[s]")
            { throw new ArgumentException("Input format not as expected (TPS)"); }
            if (inputFileData[2, 3] != "DEB[US gal/min]")
            { throw new ArgumentException("Input format not as expected (DEB)"); }
            if (inputFileData[2, 5] != "PR[psi]")
            { throw new ArgumentException("Input format not as expected (PR)"); }
            if (inputFileData[2, 7] != "VOL[US gal]")
            { throw new ArgumentException("Input format not as expected (VOL)"); }
        }

        //encodeOutputFileRow
        private string encodeOutputFileRow(string MPName, DateTime dateTime, string outputValueString, string valueFormat)
        {
            double outputValue;
            outputValue = double.TryParse(outputValueString, out outputValue) ? outputValue : 9999;

            return MPName + "\t" +
                   dateTime.ToString("dd'/'MM'/'yyyy HH:mm:ss") + "\t" +
                   outputValue.ToString(valueFormat, CultureInfo.InvariantCulture) + 
                   "\t0";
        }


        #endregion
    }
}
