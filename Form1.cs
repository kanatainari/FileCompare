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

        private void CopyFiles(ListView srcListView, string srcDirPath, string destDirPath)
        {
            if (!Directory.Exists(srcDirPath) || !Directory.Exists(destDirPath)) return;
            if (srcListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("복사할 파일을 선택해주세요.");
                return;
            }

            try
            {
                foreach (ListViewItem item in srcListView.SelectedItems)
                {
                    if (item.SubItems[1].Text == "<DIR>") continue;

                    string fileName = item.Text;
                    string srcFile = Path.Combine(srcDirPath, fileName);
                    string destFile = Path.Combine(destDirPath, fileName);

                    if (File.Exists(destFile))
                    {
                        DateTime srcTime = File.GetLastWriteTime(srcFile);
                        DateTime destTime = File.GetLastWriteTime(destFile);

                        // 원본(src)이 대상(dest)보다 오래된 경우 복사 중단
                        if (srcTime < destTime)
                        {
                            string msg = $"[{fileName}]\n\n원본 파일이 대상 폴더의 파일보다 오래되었습니다.\n최신 파일을 보호하기 위해 복사를 취소합니다.";
                            MessageBox.Show(msg, "복사 거부", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            continue; // 다음 파일로 넘어감
                        }
                    }

                    File.Copy(srcFile, destFile, true);
                }

                PopulateListView(lvwLeftDir, txtLeftDir.Text, txtRightDir.Text);
                PopulateListView(lvwRightDir, txtRightDir.Text, txtLeftDir.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("복사 중 오류 발생: " + ex.Message);
            }
        }

        private void btnCopyFromLeft_Click(object sender, EventArgs e)
        {
            CopyFiles(lvwLeftDir, txtLeftDir.Text, txtRightDir.Text);
        }

        private void btnCopyFromRight_Click(object sender, EventArgs e)
        {
            CopyFiles(lvwRightDir, txtRightDir.Text, txtLeftDir.Text);
        }
    }
}