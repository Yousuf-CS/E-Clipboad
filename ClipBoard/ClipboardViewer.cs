using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Google.Apis.Drive.v2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace ClipBoard
{
    public partial class ClipboardViewer : Form
    {

        #region custom data members

        private bool _hotkey;
        public IntPtr _clipboardViewerNext;
        bool _lock;

        #endregion

        public ClipboardViewer()
        {
            InitializeComponent();
            this._lock = false;
        }

        #region private methods

        // Register this form as a Clipboard Viewer application
        private void registerClipboardViewer(IntPtr handle)
        {
            this._clipboardViewerNext = ClipboardViewer_Hotkey.SetClipboardViewer(handle);

        }

        // Remove this form from the Clipboard Viewer list
        private void unregisterClipboardViewer(IntPtr handle)
        {
            ClipboardViewer_Hotkey.ChangeClipboardChain(handle, this._clipboardViewerNext);
        }

        private void registerHotkey(IntPtr handle)
        {
            int id = 0;     // The id of the hotkey. 
            _hotkey = ClipboardViewer_Hotkey.RegisterHotKey(handle, id, (int)KeyModifier.Control, Keys.X.GetHashCode());
        }

        private void unregisterHotkey(IntPtr handle)
        {
            ClipboardViewer_Hotkey.UnregisterHotKey(handle, 0);
        }

        private String getUIMonthYearString()
        {
            return dateTimePicker1.Value.Month + "_" + dateTimePicker1.Value.Year;
            
        }

        private void populateUI(string fileName)
        {
            listView1.Items.Clear();
            string m = "";
            Google.Apis.Drive.v2.Data.File f;
            if (File.Exists(EClipboard.cb.LocalDatafilesPath + "\\" + fileName + ".json"))
            {
                m = File.ReadAllText(EClipboard.cb.LocalDatafilesPath + "\\" + fileName + ".json");
            }
            else
            {
                if ((f = EClipboard.cb.fileExists(fileName)) != null)
                {
                    EClipboard.cb.downloadFile(f);
                    m = File.ReadAllText(EClipboard.cb.LocalDatafilesPath + "\\" + fileName + ".json");
                }
                else
                {
                    MessageBox.Show("No such file exists");
                    dateTimePicker1.ResetText();
                    return;
                }
            }
                
            
            
            JArray j = JArray.Parse(m);
            
            
            if (textBox1.Text.Length > 0)
            {


                var resStudents = (from s in j
                                   where s["TextData"].ToString().ToLower().Contains(textBox1.Text.ToLower()) || s["OS"].ToString().ToLower().Contains(textBox1.Text.ToLower()) || s["Date"].ToString().ToLower().Contains(textBox1.Text.ToLower()) || s["Time"].ToString().ToLower().Contains(textBox1.Text.ToLower())
                                   select new { text = s["TextData"], date = s["Date"], tim = s["Time"], os = s["OS"] }).ToList();


                //Console.WriteLine("Only Student Names");
                foreach (var item in resStudents)
                {


                    ListViewItem lv1 = new ListViewItem(item.text.ToString());
                    lv1.SubItems.Add(item.date.ToString());
                    lv1.SubItems.Add(item.tim.ToString());
                    lv1.SubItems.Add(item.os.ToString());
                    listView1.Items.Add(lv1);


                }
            }
            else
            {
                //string m = File.ReadAllText(EClipboard.cb.LocalDatafilesPath + "\\" + this.getMonthYearString() + ".json");



                var resStudents = (from s in j
                                   // where s["TextData"].ToString().Contains("gama")
                                   select new { text = s["TextData"], date = s["Date"], tim = s["Time"], os = s["OS"] }).ToList();



                //Console.WriteLine("Only Student Names");
                foreach (var item in resStudents)
                {


                    ListViewItem lv1 = new ListViewItem(item.text.ToString());
                    lv1.SubItems.Add(item.date.ToString());
                    lv1.SubItems.Add(item.tim.ToString());
                    lv1.SubItems.Add(item.os.ToString());
                    listView1.Items.Add(lv1);


                }
            }
        }

        #endregion

        // overrided so that the main form can respond to window messages from the operating system
        protected override void WndProc(ref Message m)
        {
            switch ((Msgs)m.Msg)
            {
                //
                // The WM_DRAWCLIPBOARD message is sent to the first window 
                // in the clipboard viewer chain when the content of the 
                // clipboard changes. This enables a clipboard viewer 
                // window to display the new content of the clipboard. 
                //
                case Msgs.WM_DRAWCLIPBOARD:
                    //MessageBox.Show(this._lock.ToString());
                    if (!this._lock)
                    {
                        //Console.WriteLine("WindowProc DRAWCLIPBOARD: " + m.Msg, "WndProc");
                        if (Clipboard.ContainsText())
                        {
                            ClipboardObject c = new ClipboardObject(Clipboard.GetText());
                            //File.AppendAllText(EClipboard.cb.LocalDatafilesPath + "\\new.json", JsonConvert.SerializeObject(c));
                            if (Directory.Exists(EClipboard.cb.LocalDatafilesPath))
                            {
                                #region need to be considered later
                                
                                String[] lines = File.ReadAllLines(EClipboard.cb.LocalDatafilesPath + "\\" + EClipboard.cb.getSystemMonthYearString() + ".json");
                                int count = lines.Length - 1;
                                
                                for (int i = 0; i < count; i++)
                                {
                                    File.AppendAllText(EClipboard.cb.LocalDatafilesPath + "\\temp.json", lines[i] + Environment.NewLine);
                                }
                                
                                #endregion


                                using (StreamWriter file = File.AppendText(EClipboard.cb.LocalDatafilesPath + "\\temp.json"))
                                {
                                    JsonSerializer serializer = new JsonSerializer();
                                    if (count == 1)
                                    {
                                        serializer.Serialize(file, c);
                                        file.WriteLine();
                                        file.Write("]");
                                    }
                                    else
                                    {
                                        file.Write(",");
                                        serializer.Serialize(file, c);
                                        file.WriteLine();
                                        file.Write("]");

                                    }
                                }

                                File.Delete(EClipboard.cb.LocalDatafilesPath + "\\" + EClipboard.cb.getSystemMonthYearString() + ".json");
                                File.Move(EClipboard.cb.LocalDatafilesPath + "\\temp.json", EClipboard.cb.LocalDatafilesPath + "\\" + EClipboard.cb.getSystemMonthYearString() + ".json");

                                Google.Apis.Drive.v2.Data.File f; ;
                                if ((f = EClipboard.cb.fileExists(EClipboard.cb.getSystemMonthYearString())) != null)
                                {
                                    Console.WriteLine("File id: " + f.Id);
                                    EClipboard.cb.updateFile(EClipboard.cb.getSystemMonthYearString() + ".json", f.Id);
                                }
                                else
                                {
                                    Console.WriteLine("File not found");
                                    EClipboard.cb.uploadFile(EClipboard.cb.getSystemMonthYearString() + ".json");

                                }
                                //EClipboard.cb.uploadFile();



                            }
                            else
                            {
                                Console.WriteLine("directory not found");
                            }
                        }
                        dateTimePicker1.ResetText();
                        //this.populateUI(this.getUIMonthYearString());
                        ClipboardViewer_Hotkey.SendMessage(this._clipboardViewerNext, m.Msg, m.WParam, m.LParam);
                    }
                    
                    
                    //
                    // Each window that receives the WM_DRAWCLIPBOARD message 
                    // must call the SendMessage function to pass the message 
                    // on to the next window in the clipboard viewer chain.
                    //
                    
                    break;


                //
                // The WM_CHANGECBCHAIN message is sent to the first window 
                // in the clipboard viewer chain when a window is being 
                // removed from the chain. 
                //
                case Msgs.WM_CHANGECBCHAIN:
                    Console.WriteLine("WM_CHANGECBCHAIN: lParam: " + m.LParam, "WndProc");

                    // When a clipboard viewer window receives the WM_CHANGECBCHAIN message, 
                    // it should call the SendMessage function to pass the message to the 
                    // next window in the chain, unless the next window is the window 
                    // being removed. In this case, the clipboard viewer should save 
                    // the handle specified by the lParam parameter as the next window in the chain. 

                    //
                    // wParam is the Handle to the window being removed from 
                    // the clipboard viewer chain 
                    // lParam is the Handle to the next window in the chain 
                    // following the window being removed. 
                    if (m.WParam == this._clipboardViewerNext)
                    {
                        //
                        // If wParam is the next clipboard viewer then it
                        // is being removed so update pointer to the next
                        // window in the clipboard chain
                        //
                        this._clipboardViewerNext = m.LParam;
                    }
                    else
                    {
                        ClipboardViewer_Hotkey.SendMessage(this._clipboardViewerNext, m.Msg, m.WParam, m.LParam);
                    }
                    break;

                case Msgs.WM_HOTKEY:
                    //MessageBox.Show(new Form() { TopMost = true }, "Ctrl+X presses");
                    MessageBox.Show("Ctrl+X pressed", "Important", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button3, MessageBoxOptions.ServiceNotification);
                    break;

                default:
                    //
                    // Let the form process the messages that we are
                    // not interested in
                    //
                    base.WndProc(ref m);
                    break;

            }

        }

        #region event handlers

        private void button2_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(EClipboard.cb.LocalTokensPath))
            {
                Directory.Delete(EClipboard.cb.LocalTokensPath, true);
                ClipBoard.Program.f.Show();
                this.Hide();
            }
        }

        private void ClipboardViewer_Load(object sender, EventArgs e)
        {
            this.registerClipboardViewer(this.Handle);
            this.registerHotkey(this.Handle);
            
        }

        private void ClipboardViewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.unregisterClipboardViewer(this.Handle);
            this.unregisterHotkey(this.Handle);
            Application.Exit();
        }

        private void ClipboardViewer_Resize(object sender, EventArgs e)
        {

            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Icon = this.Icon;
                notifyIcon1.Visible = true;
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
        
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }


        #endregion

        private void ClipboardViewer_Shown(object sender, EventArgs e)
        {
            //Thread t = new Thread(new ThreadStart(EClipboard.cb.initialize));
            //EClipboard.cb.initialize();
            this.populateUI(this.getUIMonthYearString());
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                pictureBox1.Visible = true;
            }
            else
            {
                pictureBox1.Visible = false;
            }
            this.populateUI(this.getUIMonthYearString());
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            richTextBox1.Text = e.Item.SubItems[0].Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this._lock = true;
            Clipboard.SetText(richTextBox1.Text);
            this._lock = false;
            MessageBox.Show("Copied to clipboard");

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            this.populateUI(this.getUIMonthYearString());
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if (dateTimePicker2.Value.Month.ToString().Length > 1)
            {
                textBox1.Text = dateTimePicker2.Value.Day.ToString() + "-" + dateTimePicker2.Value.Month.ToString() + "-" + dateTimePicker2.Value.Year.ToString();
            }
            else
            {
                textBox1.Text = dateTimePicker2.Value.Day.ToString() + "-0" + dateTimePicker2.Value.Month.ToString() + "-" + dateTimePicker2.Value.Year.ToString();
            }
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

       
    }
}
