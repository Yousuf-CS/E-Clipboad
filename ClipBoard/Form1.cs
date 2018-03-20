using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Management;
using Newtonsoft.Json;
using System.Threading;

namespace ClipBoard
{
    public partial class Form1 : Form
    {

        #region Custom data members

        

        #endregion

        
        public Form1()
        {
            InitializeComponent();
        }


        #region Custom private methods

        private bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("https://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion


        #region Event handlers


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox t = (TextBox)sender;
            if (t.Text.Length > 0)
            {
                signin.Enabled = true;
            }
            else
            {
                signin.Enabled = false;
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://accounts.google.com/SignUp?hl=en");
            Process.Start(sInfo);
            
        }

        private void signin_Click(object sender, EventArgs e)
        {

            if (this.CheckForInternetConnection())
            {
                if (EClipboard.cb.signIn())
                {
                    lb_error.Visible = false;
                    Thread w = new Thread(() => EClipboard.cb.initialize());
                    w.Start();
                    while (!w.IsAlive) ;
                    w.Join();
                    Clipboard.Clear();
                    ClipboardViewer f = new ClipboardViewer();
                    f.Show();
                    this.Hide();

                }
                else
                {
                    lb_error.Visible = true;
                }
            }
            else
            {
                lb_error.Visible = true;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }



        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
