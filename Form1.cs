using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FileCompare
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void PopulateListView(ListView lv, string folderPath, string comparePath = null)
        {
            lv.BeginUpdate();
            lv.Items.Clear();
            try
            {
                Dictionary<string, FileInfo> compareFiles = new Dictionary<string, FileInfo>();
                if (!string.IsNullOrEmpty(comparePath) && Directory.Exists(comparePath))
                {
                    compareFiles = new DirectoryInfo(comparePath).GetFiles()
                                   .ToDictionary(f => f.Name, f => f);
                }

                var dirs = Directory.EnumerateDirectories(folderPath)
                        .Select(p => new DirectoryInfo(p))
                        .OrderBy(d => d.Name);

                foreach (var d in dirs)
                {
                    var item = new ListViewItem(d.Name);
                    item.SubItems.Add("<DIR>");
                    item.SubItems.Add(d.LastWriteTime.ToString("g"));
                    lv.Items.Add(item);
                }

                var files = Directory.EnumerateFiles(folderPath)
                    .Select(p => new FileInfo(p))
                    .OrderBy(f => f.Name);

                foreach (var f in files)
                {
                    compareFiles.TryGetValue(f.Name, out FileInfo rf);

                    var item = new ListViewItem(f.Name);
                    item.SubItems.Add(f.Length.ToString("N0") + " 바이트");
                    item.SubItems.Add(f.LastWriteTime.ToString("g"));

                    if (rf != null)
                    {
                        if (f.LastWriteTime == rf.LastWriteTime)
                        {
                            item.ForeColor = Color.Black;
                        }
                        else if (f.LastWriteTime > rf.LastWriteTime)
                        {
                            item.ForeColor = Color.Red;
                        }
                        else
                        {
                            item.ForeColor = Color.Gray;
                        }
                    }
                    else
                    {
                        item.ForeColor = Color.Purple;
                    }

                    lv.Items.Add(item);
                }

                for (int i = 0; i < lv.Columns.Count; i++)
                {
                    if (i == lv.Columns.Count - 1)
                    {
                        lv.Columns[i].Width = -2;
                    }
                    else
                    {
                        lv.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show(this, "폴더를 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show(this, "입출력 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lv.EndUpdate();
            }
        }

        private void btnLeftDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를 선택하세요.";
                if (!string.IsNullOrWhiteSpace(txtLeftDir.Text) && Directory.Exists(txtLeftDir.Text))
                {
                    dlg.SelectedPath = txtLeftDir.Text;
                }

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtLeftDir.Text = dlg.SelectedPath;
                    PopulateListView(lvwLeftDir, txtLeftDir.Text, txtRightDir.Text);
                    if (!string.IsNullOrEmpty(txtRightDir.Text))
                    {
                        PopulateListView(lvwRightDir, txtRightDir.Text, txtLeftDir.Text);
                    }
                }
            }
        }

        private void btnRightDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를 선택하세요.";
                if (!string.IsNullOrWhiteSpace(txtRightDir.Text) && Directory.Exists(txtRightDir.Text))
                {
                    dlg.SelectedPath = txtRightDir.Text;
                }

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtRightDir.Text = dlg.SelectedPath;
                    PopulateListView(lvwRightDir, txtRightDir.Text, txtLeftDir.Text);
                    if (!string.IsNullOrEmpty(txtLeftDir.Text))
                    {
                        PopulateListView(lvwLeftDir, txtLeftDir.Text, txtRightDir.Text);
                    }
                }
            }
        }
    }
}