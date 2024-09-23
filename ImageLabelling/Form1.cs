using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ImageLabelling
{
    public partial class Form1 : Form
    {
        private string[] _files;

        private int _currentFile = -1;

        private bool _min = true;

        public Form1()
        {
            InitializeComponent();
        }

        private void cmdNextImage_Click(object sender, EventArgs e)
        {
            if (_files == null || _files.Length == 0)
            {
                _files = Directory.GetFiles(txtImageDirectory.Text);
                lblImageCount.Text = _files.Length.ToString();
                _currentFile = -1;
            }

            if (_files.Length > 0)
            {
                _currentFile++;
                _currentFile %= _files.Length;
                
                using (var fs = new FileStream(_files[_currentFile], FileMode.Open, FileAccess.Read))
                {
                    var ms = new MemoryStream();
                    fs.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    picCurrent.Image = Image.FromStream(ms);
                }

                lblCurrent.Text = _currentFile.ToString();

                lblXmin.Text = "0.0";
                lblYmin.Text = "0.0";
                lblXmax.Text = "0.0";
                lblYmax.Text = "0.0";
                lblStatus.Text = "-";
                chkHasSign.Checked = false;

                lblFilename.Text = _files[_currentFile];
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.N && e.Control)
            {
                cmdNextImage_Click(null, null);
            }
        }
        private void picCurrent_Click(object sender, EventArgs e)
        {
            var mouseEvent = (MouseEventArgs)e;
            var coordinates = mouseEvent.Location;

            if (_min)
            {
                lblXmin.Text = (coordinates.X / 512.0).ToString("N6");
                lblYmin.Text = (coordinates.Y / 512.0).ToString("N6");
            }
            else
            {
                lblXmax.Text = (coordinates.X / 512.0).ToString("N6");
                lblYmax.Text = (coordinates.Y / 512.0).ToString("N6");
            }

            _min = !_min;
            chkHasSign.Checked = true;
        }

        private void cmdSaveImage_Click(object sender, EventArgs e)
        {
            var filename = Guid.NewGuid() + ".jpg";
            
            File.Move(_files[_currentFile], txtLabelledRoot.Text + "\\images\\" + filename);
            var sb = new StringBuilder();
            sb.Append(filename);
            sb.Append("," + (chkHasSign.Checked ? "1.0" : "0.0"));
            sb.Append("," + lblXmin.Text);
            sb.Append("," + lblYmin.Text);
            sb.Append("," + lblXmax.Text);
            sb.AppendLine("," + lblYmax.Text);
            File.AppendAllText(txtLabelledRoot.Text + "\\annotations.csv", sb.ToString());
            lblStatus.Text = "Saved";
        }


    }
}
