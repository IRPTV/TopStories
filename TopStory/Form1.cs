using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TopStory
{
    public partial class Form1 : Form
    {
        int _ImagesCount = int.Parse(ConfigurationSettings.AppSettings["ImagesCount"].ToString().Trim());
        int _ImageIndex = 1;
        WebBrowser wb = new WebBrowser();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            button1.ForeColor = Color.White;
            button1.Text = "Started";
            button1.BackColor = Color.Red;

            string[] FilesList = Directory.GetFiles(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim());
            foreach (string item in FilesList)
            {
                try
                {
                    if (File.GetLastAccessTime(item) < DateTime.Now.AddHours(-48))
                    {
                        File.Delete(item);
                        richTextBox1.Text += (item) + " *Deleted* \n";
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                        Application.DoEvents();
                    }
                }
                catch (Exception Exp)
                {
                    richTextBox1.Text += (Exp) + " \n";
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                    Application.DoEvents();
                }

            }
            GenerateScreenshot(ConfigurationSettings.AppSettings["WebUrl"].ToString().Trim());

           
        }
        public void GenerateScreenshot(string url)
        {
            richTextBox1.Text = "";
            timer1.Enabled = false;
            timer2.Enabled = false;
            timer3.Enabled = false;
            wb.NewWindow += wb_NewWindow;
            wb.ScrollBarsEnabled = false;
            wb.ScriptErrorsSuppressed = true;
            wb.Navigate(url);
            while (wb.ReadyState != WebBrowserReadyState.Complete) { Application.DoEvents(); }
            richTextBox1.Text += "Web Site Loaded" + " \n";
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
            Application.DoEvents();
            groupBox3.Controls.Clear();
            groupBox3.Controls.Add(wb);
            wb.Width = 1010;
            wb.Height = 700;
            _ImageIndex = 1;
            timer3.Enabled = true;
        }
        void wb_NewWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            int StartMinute = int.Parse(ConfigurationSettings.AppSettings["RenderIntervalMin"].ToString().Trim());
            if (DateTime.Now.Minute >= StartMinute && DateTime.Now.Minute <= StartMinute + 1)
            {
                timer1.Enabled = false;
                button1_Click(new object(), new EventArgs());
            }

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            timer2.Interval = int.Parse(ConfigurationSettings.AppSettings["WaitForBetweenImagesSec"].ToString().Trim()) * 1000;
          //  timer1.Interval = int.Parse(ConfigurationSettings.AppSettings["RenderIntervalMin"].ToString().Trim()) * 60 * 1000;
            timer3.Interval = int.Parse(ConfigurationSettings.AppSettings["WaitForSiteLoadSec"].ToString().Trim()) * 1000;
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Enabled = false;
            if (_ImageIndex <= _ImagesCount)
            {
                Bitmap bitmap = new Bitmap(wb.Width, wb.Height);
                wb.DrawToBitmap(bitmap, new Rectangle(0, 0, wb.Width, wb.Height));
                Bitmap scaled = new Bitmap(1920, 1080);
                using (Graphics graphics = Graphics.FromImage(scaled))
                {
                    graphics.DrawImage(bitmap, new Rectangle(0, 0, scaled.Width, scaled.Height));
                }
                scaled.Save(ConfigurationSettings.AppSettings["ImagePath"].ToString().Trim() + _ImageIndex.ToString() + ".png");
                richTextBox1.Text += ConfigurationSettings.AppSettings["ImagePath"].ToString().Trim() + _ImageIndex.ToString() + ".png" + " \n";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                Application.DoEvents();

                if (_ImageIndex == _ImagesCount)
                {
                    #region Renderer
                    // D:\Program Files\Adobe\Adobe After Effects CS6\Support Files>aerender.exe -project "e:\AEProjs\iFILM website. 8 collect folder\Web Edit
                    //22.8 collect.aep" -comp "web" -output  e:\2.avi

                    Process proc = new Process();
                    proc.StartInfo.FileName = "\"" + ConfigurationSettings.AppSettings["AeRenderPath"].ToString().Trim() + "\"";




                    //string FilePath = dataGridView1.Rows[_Index].Cells[1].Value.ToString();

                    //    string sss = Path.GetDirectoryName(FilePath.Replace("\\\\", "\\"));
                    //    //    DirectoryInfo Dir = new DirectoryInfo);
                    //    string[] Paths = FilePath.Replace(textBox1.Text.Trim(), "").Split('\\');
                    //    string NewPath = "";
                    //    for (int i = 0; i < Paths.Length - 1; i++)
                    //    {
                    //        NewPath += "\\" + Paths[i];
                    //    }
                    string DateTimeStr = string.Format("{0:0000}", DateTime.Now.Year) + "-" + string.Format("{0:00}", DateTime.Now.Month) + "-" + string.Format("{0:00}", DateTime.Now.Day) + "_" + string.Format("{0:00}", DateTime.Now.Hour) + "-" + string.Format("{0:00}", DateTime.Now.Minute) + "-" + string.Format("{0:00}", DateTime.Now.Second);

                    DirectoryInfo Dir = new DirectoryInfo(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim());

                    if (!Dir.Exists)
                    {
                        Dir.Create();
                    }


                    proc.StartInfo.Arguments = " -project " + "\"" + ConfigurationSettings.AppSettings["AeProjectFile"].ToString().Trim() + "\"" + "   -comp   \"" + ConfigurationSettings.AppSettings["Composition"].ToString().Trim() + "\" -output " + "\"" + ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim() + ConfigurationSettings.AppSettings["OutPutFileName"].ToString().Trim() + "_" + DateTimeStr + ".mp4" + "\"";
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.EnableRaisingEvents = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;

                    if (!proc.Start())
                    {


                        return;
                    }


                    proc.PriorityClass = ProcessPriorityClass.Normal;
                    StreamReader reader = proc.StandardOutput;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        richTextBox1.Text += (line) + " \n";
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                        Application.DoEvents();
                    }
                    proc.Close();






                    try
                    {
                        string StaticDestFileName = ConfigurationSettings.AppSettings["ScheduleDestFileName"].ToString().Trim();
                        // File.Delete(StaticDestFileName);
                        File.Copy(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim() + ConfigurationSettings.AppSettings["OutPutFileName"].ToString().Trim() + "_" + DateTimeStr + ".mp4", StaticDestFileName, true);
                        richTextBox1.Text += "COPY FINAL:" + StaticDestFileName + " \n";
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                        Application.DoEvents();
                    }
                    catch (Exception Ex)
                    {
                        richTextBox1.Text += Ex.Message + " \n";
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                        Application.DoEvents();
                    }















                    #endregion
                    timer1.Enabled = true;
                    timer2.Enabled = false;
                    timer3.Enabled = false;
                    this.Text = "TopStory V1.5 20140130::" + DateTime.Now.ToString();
                    button1.ForeColor = Color.White;
                    button1.Text = "Start";
                    button1.BackColor = Color.Navy;

                }
                timer2.Enabled = true;
                _ImageIndex++;
            }
        }
        private void timer3_Tick(object sender, EventArgs e)
        {
            timer3.Enabled = false;
            timer2_Tick(new Object(), new EventArgs());
            timer2.Enabled = true;
        }
    }
}
