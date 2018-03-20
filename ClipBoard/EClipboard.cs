using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
//using Google.Apis.Services;
//using Google.Apis.Util.Store;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.IO;
using System.Threading;

namespace ClipBoard
{
    
    class EClipboard
    {
        #region data members
        
        private DriveService service;
        private Thread worker;
        public static readonly EClipboard cb = new EClipboard();


        #endregion

        #region constructors

        private EClipboard()
        {
            this.LocalFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.LocalFilesPath += "\\E-Clipboard";
            this.LocalTokensPath = LocalFilesPath + "\\tokens";
            this.LocalDatafilesPath = LocalFilesPath + "\\dataFiles";
            this.Client_id = "506666529089-eqplt2bdlnvulfpshcj9p8l2g2u1laaj.apps.googleusercontent.com";
            this.Client_secret = "TTPz5MM8eVaVjm3SgztjxiJ4";
        }

        private EClipboard(String localPath, String client_id, String client_pass)
        {
            this.LocalFilesPath = localPath;
            this.LocalFilesPath += "\\E-Clipboard";
            this.LocalTokensPath = LocalFilesPath + "\\tokens";
            this.Client_id = client_id;
            this.Client_secret = client_pass;
        }

        #endregion

        #region public methods

        public bool signIn()
        {
            this.service = Authentication.AuthenticateOauth(this.Client_id, this.Client_secret, Environment.UserName);
           
            return this.service != null;
        }

        public Google.Apis.Drive.v2.Data.File fileExists(String title)
        {
            //String Q = "'" + this.DriveFolderId + "' in parents and title = '" + title + "' and mimeType = 'application/vnd.google-apps.file'";
            String Q1 = "'" + this.DriveFolderId + "' in parents";
            IList<Google.Apis.Drive.v2.Data.File> _files = DaimtoGoogleDriveHelper.GetFiles(this.service, Q1);

            if (_files.Count != 0)
            {
                foreach (var item in _files)
                {
                    if (item.Title.Contains(title) && !item.ExplicitlyTrashed.Value)
                    {
                        return item;
                        
                    }
                }
            }
            return null;
        }

        public void uploadFile(String nameWithExtension) 
        {
            this.worker = new Thread(() => DaimtoGoogleDriveHelper.uploadFile(this.service, this.LocalDatafilesPath + "\\" + nameWithExtension, this.DriveFolderId));
            this.worker.Start();
        }

        public void updateFile(String nameWithExtension, String id)
        {
            this.worker = new Thread(() => DaimtoGoogleDriveHelper.updateFile(this.service, this.LocalDatafilesPath + "\\" + nameWithExtension, this.DriveFolderId, id));
            this.worker.Start();
        }

        public void downloadFile(Google.Apis.Drive.v2.Data.File f)
        {
            this.worker = new Thread(() => DaimtoGoogleDriveHelper.downloadFile(this.service, f, this.LocalDatafilesPath + "\\" + f.Title));
            this.worker.Start();
            this.worker.Join();
        }

        public String getSystemMonthYearString()
        {
            return System.DateTime.Now.Month + "_" + System.DateTime.Now.Year;
        }

        public void initialize()
        {
            
            String Q = "title = 'eclipboard' and mimeType = 'application/vnd.google-apps.folder'";           
            IList<Google.Apis.Drive.v2.Data.File> _files = DaimtoGoogleDriveHelper.GetFiles(this.service, Q);
            
            if (_files.Count != 0)
            {
                if (!_files[0].ExplicitlyTrashed.Value)
                {
                    this.DriveFolderId = _files[0].Id;
                }
                else
                {
                    this.DriveFolderId = DaimtoGoogleDriveHelper.createDirectory(this.service, "eclipboard", "contains clipboard data files", "root").Id;
                }
               
            }
            else
            {
                this.DriveFolderId = DaimtoGoogleDriveHelper.createDirectory(this.service, "eclipboard", "contains clipboard data files", "root").Id;
            }


            Google.Apis.Drive.v2.Data.File f;
            StreamWriter f1;
            try
            {
                if (!(Directory.Exists(this.LocalFilesPath)))
                {
                    MessageBox.Show("IF called");
                    Directory.CreateDirectory(this.LocalFilesPath);
                    Directory.CreateDirectory(this.LocalDatafilesPath);
                    Directory.CreateDirectory(this.LocalTokensPath);

                    if ((f = this.fileExists(this.getSystemMonthYearString())) != null)
                    {
                        DaimtoGoogleDriveHelper.downloadFile(this.service, f, this.LocalDatafilesPath + "\\" + f.Title);
                    }
                    else
                    {
                        File.CreateText(this.LocalDatafilesPath + "\\" + this.getSystemMonthYearString() + ".json").Close();
                    }
                }
                else
                {
                    //MessageBox.Show("ELSE called");
                    if (!(Directory.Exists(this.LocalDatafilesPath)))
                    {
                        Directory.CreateDirectory(this.LocalDatafilesPath);
                    }

                    if ((f = EClipboard.cb.fileExists(this.getSystemMonthYearString())) != null)
                    {
                        this.downloadFile(f);
                        this.worker.Join();
                    }
                    else
                    {
                        f1 = File.CreateText(EClipboard.cb.LocalDatafilesPath + "\\" + this.getSystemMonthYearString() + ".json");
                        f1.WriteLine("[");
                        f1.Write("]");
                        f1.Close();
                       

                    }
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine("The process failed: {0}", e1.ToString());
            }

        }

        #endregion

        #region properties

        public String LocalFilesPath { get; set; }
        public String LocalTokensPath { get; set; }
        public String LocalDatafilesPath { get; set; }
        public String Client_id { get; set; }
        public String Client_secret { get; set; }
        public String DriveFolderId { get; set; }

        #endregion

        
    }
}
