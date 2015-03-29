using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThumbnailCreator
{
    public partial class FormMain : Form
    {
        Settings settings;
        Log log;
        List<string> selectedImagePaths;
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            selectedImagePaths = new List<string>();
            log = new Log(Application.StartupPath);
            log.AppendMessage("Application loading...");
            settings = new Settings(Application.StartupPath, log);
            settings.Load();
            textBoxWorkingDirectory.Text = settings.LastWorkingDirectory;
            splitContainer2.SplitterDistance = settings.SplitContainerImagePosition;
            this.Width = settings.WindowWidth;
            this.Height = settings.WindowHeight;
            numericUpDownWidth.Value = settings.ThumbnailWidth;
            numericUpDownHeight.Value = settings.ThumbnailHeight;
            checkBoxKeepRatio.Checked = settings.KeepRatio;
            comboBoxDimension.Text = settings.Dimension;
            log.AppendMessage("Loaded application successfully.");
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            settings.LastWorkingDirectory = textBoxWorkingDirectory.Text;
            settings.SplitContainerImagePosition = splitContainer2.SplitterDistance;
            settings.WindowWidth = this.Width;
            settings.WindowHeight = this.Height;
            settings.ThumbnailWidth = Convert.ToInt32(numericUpDownWidth.Value);
            settings.ThumbnailHeight = Convert.ToInt32(numericUpDownHeight.Value);
            settings.KeepRatio = checkBoxKeepRatio.Checked;
            settings.Dimension = comboBoxDimension.Text;
            settings.Save();
            log.AppendMessage("Application closed.");
        }

        private void buttonWorkingDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxWorkingDirectory.Text = fbd.SelectedPath;
            }
            log.AppendMessage("New directory: "+ textBoxWorkingDirectory.Text);
        }

        private void textBoxWorkingDirectory_TextChanged(object sender, EventArgs e)
        {
            listView.Items.Clear();
            if (!Directory.Exists(textBoxWorkingDirectory.Text)) { textBoxWorkingDirectory.Text = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures); }
            foreach (string s in Directory.GetFiles(textBoxWorkingDirectory.Text))
            {
                if (settings.PictureExtensions.Contains(Path.GetExtension(s).ToLower()))
                {
                    Image i = Image.FromFile(s);
                    listView.Items.Add(new ListViewItem(new string[] { s, i.Width.ToString(), i.Height.ToString() ,GetFileSizeString(s)}));
                    i.Dispose();
                    i = null;
                    GC.Collect();
                }
            }
            try
            {
                listView.SelectedIndices.Add(0);
                listView.Focus();
            }
            catch
            {
                pictureBox.Image = null;
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedImagePaths.Clear();
            try
            {
                foreach (ListViewItem lvi in listView.SelectedItems) 
                {
                    selectedImagePaths.Add(lvi.SubItems[0].Text);
                }
            }
            catch { }
            try
            {
                pictureBox.Image = Image.FromFile(selectedImagePaths[0]);
            }
            catch { }
        }

        private void buttonCreateThumbnail_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0) return;
            foreach (string selectedImagePath in selectedImagePaths)
            {
                try
                {
                    string newFileName = Path.GetDirectoryName(selectedImagePath) + "\\" + Path.GetFileNameWithoutExtension(selectedImagePath) + settings.Suffix + Path.GetExtension(selectedImagePath);
                    if (comboBoxDimension.Text == "Beides")
                    {
                        Thumbnail.Create(Image.FromFile(selectedImagePath), Convert.ToInt32(numericUpDownWidth.Value), Convert.ToInt32(numericUpDownHeight.Value), checkBoxKeepRatio.Checked, settings.Quality).Save(newFileName);
                    }
                    else if (comboBoxDimension.Text == "Breite")
                    {
                        Thumbnail.Create(Image.FromFile(selectedImagePath), Convert.ToInt32(numericUpDownWidth.Value), Thumbnail.Dimension.Width, checkBoxKeepRatio.Checked, settings.Quality).Save(newFileName);
                    }
                    else if (comboBoxDimension.Text == "Höhe")
                    {
                        Thumbnail.Create(Image.FromFile(selectedImagePath), Convert.ToInt32(numericUpDownHeight.Value), Thumbnail.Dimension.Height, checkBoxKeepRatio.Checked, settings.Quality).Save(newFileName);
                    }
                    log.AppendMessage("Created thumbnail \""+newFileName+"\" from file \""+selectedImagePath+"\".");
                }
                catch (Exception ex)
                {
                    log.AppendMessage("An error occured while creating a thumbnail for \"" + selectedImagePath + "\". Error message: " + ex.Message);
                }
            }
            textBoxWorkingDirectory_TextChanged(sender, e);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                Process.Start(selectedImagePaths[0]);
                log.AppendMessage("Opening " + selectedImagePaths[0]);
            }
            catch (Exception ex)
            {
                log.AppendMessage("Could not open " + selectedImagePaths[0] + " Error message: " + ex.Message);
            }
        }

        private void löschenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox.Image = null;
                GC.Collect();
                System.Threading.Thread.Sleep(100);
                File.Delete(selectedImagePaths[0]);
                log.AppendMessage("Deleted \"" + selectedImagePaths[0] + "\"");
                textBoxWorkingDirectory_TextChanged(sender, e);
            }
            catch (Exception ex)
            {
                log.AppendMessage("Could not delete \"" + selectedImagePaths[0] + "\". Error message: " + ex.Message);
            }

        }

        private void thumbnailErstellenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonCreateThumbnail_Click(sender, e);
        }

        private void comboBoxDimension_SelectedIndexChanged(object sender, EventArgs e)
        {
            numericUpDownHeight.Enabled = comboBoxDimension.Text == "Beides" || comboBoxDimension.Text == "Höhe";
            numericUpDownWidth.Enabled = comboBoxDimension.Text == "Beides" || comboBoxDimension.Text == "Breite";
        }
  
        private string GetFileSizeString(string path)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB" };
            double len = new FileInfo(path).Length;
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }
    }
}
