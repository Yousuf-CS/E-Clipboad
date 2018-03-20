using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;


namespace ClipBoard
{
    class ClipboardObject
    {

        #region constructors

        public ClipboardObject(String textData)
        {
            this.Date = DateTime.Now.ToString("dd-MM-yyyy");
            this.Time = DateTime.Now.ToString("HH:mm:ss");
            this.TextData = textData;

            var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                        select x.GetPropertyValue("Caption")).FirstOrDefault();

            this.OS = name != null ? name.ToString() : "unknown";
        }

        #endregion


        #region properties

        public String Date { get; set; }
        public String Time { get; set; }
        public String TextData { get; set; }
        public String OS { get; set; }

        #endregion


    }
}
