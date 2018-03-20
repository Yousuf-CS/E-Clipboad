using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ClipBoard
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        public static Form1 f;
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(f = new Form1());
        }
    }
}
