using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DokanNet;

namespace FileSystem
{
    public partial class fInfo : Form
    {
        Thread thread;

        public fInfo()
        {
            InitializeComponent();
        }

        private void fInfo_Load(object sender, EventArgs e)
        {
            // 初始化挂载点
            List<String> availablePoint = new List<String>();
            for (var ch = 'A'; ch <= 'Z'; ++ch)
            {
                availablePoint.Add(new string(ch, 1));
            }

            var allDrives = System.IO.DriveInfo.GetDrives();
            foreach (var drive in allDrives)
            {
                availablePoint.Remove(drive.Name.Substring(0, 1));
            }

            foreach (var point in availablePoint)
            {
                comboMountPoint.Items.Add(point);
            }

            comboMountPoint.SelectedIndex = comboMountPoint.Items.Count / 2;
        }

        private void comboMountPoint_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int sizeMB;
            if (!int.TryParse(txtSize.Text, out sizeMB))
            {
                MessageBox.Show("请输入有效的文件系统容量");
                return;
            }
            if (sizeMB < 10 || sizeMB > 200)
            {
                MessageBox.Show("容量必须介于 10 MB 和 200 MB 之间");
                return;
            }

            int sizeByte = sizeMB * 1024 * 1024;
            Disk.Format(sizeByte, (int)(sizeByte / 16384));

            button1.Enabled = false;
            comboMountPoint.Enabled = false;
            txtSize.Enabled = false;

            String mountPoint = comboMountPoint.Text;
            
            thread = new Thread(new ThreadStart(delegate
            {
                try
                {
                    Dokan.Mount(new FSDrive(), mountPoint,
                    DokanOptions.KeepAlive | DokanOptions.RemovableDrive);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("挂载失败：" + ex.Message, "MyFS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Invoke(new Action(delegate
                    {
                        button1.Enabled = true;
                        comboMountPoint.Enabled = true;
                        txtSize.Enabled = true;
                        button2.Enabled = false;
                    }));
                }
            }));

            thread.Start();
            button2.Enabled = true;
        }

        private void fInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (button2.Enabled == true)
            {
                button2_Click(null, null);
                thread.Abort();
            }
            System.Environment.Exit(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Dokan.Unmount(comboMountPoint.Text.ElementAt(0));
            MessageBox.Show("卸载成功");
            button1.Enabled = true;
            comboMountPoint.Enabled = true;
            txtSize.Enabled = true;
            button2.Enabled = false;
            this.Close();
        }
    }
}
