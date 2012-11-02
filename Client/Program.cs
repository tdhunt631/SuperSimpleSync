using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using Common;
using System.Threading;
using System.Windows.Forms;

namespace SuperSimpleSync 
{
    class Program : System.Windows.Forms.Form
    {
        private static String directoryPath = "C:/temp/TestSyncDir";
        private static DirectoryInfo LocalStorage = new System.IO.DirectoryInfo(directoryPath);
        private static Guid accountId = Guid.Parse("{FC948776-0FA5-4ABC-A2F3-E8AC8005DFCA}"); 
        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.NotifyIcon tray;
        private Label label1;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        FormWindowState lastState = new FormWindowState();
        private ToolStripMenuItem setLocalDirectoryToolStripMenuItem;
        private FolderBrowserDialog folderBrowserDialog1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem setLocalDirectoryToolStripMenuItem1;
        SyncServer.Sync _sync = new SyncServer.Sync();

[STAThread]
        static void Main() 
        {
            Program p = new Program();
            p.InitializeComponent();

            Directory.CreateDirectory(directoryPath);

            Thread syncThread = new Thread(new ThreadStart(p.SyncWithServer));
            syncThread.Name = "syncThread";
            syncThread.IsBackground = true;
            syncThread.Start();

            Application.Run(p);
        }
                
        private void SyncWithServer()
        {
            _sync.Timeout = 600000;

            while (true)
            {

                SyncDir local = Util.AuditTree(LocalStorage);
                SyncDir server = Util.FromXml<SyncDir>(_sync.GetServerSyncDir(accountId, local.Name));
                Util.RebuildParentRelationships(server);
                ResolveDifferencesWithLocal(server.GetNewerFiles(local), LocalStorage);
                ResolveDifferencesWithServer(local.GetNewerFiles(server), LocalStorage);
                Thread.Sleep(5000);
            }
        }

        private void ResolveDifferencesWithServer(SyncDir diff, DirectoryInfo dir)
        {
            if (diff.Files != null)
            {
                foreach (SyncFile f in diff.Files)
                {
                    byte[] buffer = File.ReadAllBytes(dir.FullName + Path.DirectorySeparatorChar + f.Name);
                    _sync.SendFileToServer(accountId, f.BuildPath(), buffer);
                }
            }
            if (diff.Directories != null)
            {
                foreach (SyncDir d in diff.Directories)
                {
                    DirectoryInfo sub = new DirectoryInfo(dir.FullName + Path.DirectorySeparatorChar + d.Name);
                    if (!sub.Exists)
                        sub.Create();
                    ResolveDifferencesWithServer(d, sub);
                }
            }
        }
        
        private void ResolveDifferencesWithLocal(SyncDir diff, DirectoryInfo dir)
        {
            if (diff.Files != null)
            {
                foreach (SyncFile f in diff.Files)
                {
                    byte[] buffer = _sync.GetFileFromServer(accountId, f.BuildPath());
                    File.WriteAllBytes(dir.FullName + Path.DirectorySeparatorChar + f.Name, buffer);
                }
            }
            if (diff.Directories != null)
            {
                foreach (SyncDir d in diff.Directories)
                {
                    DirectoryInfo sub = new DirectoryInfo(dir.FullName + Path.DirectorySeparatorChar + d.Name);
                    if (!sub.Exists)
                        sub.Create();
                    ResolveDifferencesWithLocal(d, sub);
                }
            }
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Program));
            this.label1 = new System.Windows.Forms.Label();
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setLocalDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tray = new System.Windows.Forms.NotifyIcon(this.components);
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setLocalDirectoryToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.trayMenu.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(145, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Interface needs development";
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.setLocalDirectoryToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(173, 70);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // setLocalDirectoryToolStripMenuItem
            // 
            this.setLocalDirectoryToolStripMenuItem.Name = "setLocalDirectoryToolStripMenuItem";
            this.setLocalDirectoryToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.setLocalDirectoryToolStripMenuItem.Text = "Set Local Directory";
            this.setLocalDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openFolderBrowser);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // tray
            // 
            this.tray.BalloonTipText = "SuperSimpleSync is still running!";
            this.tray.ContextMenuStrip = this.trayMenu;
            this.tray.Icon = ((System.Drawing.Icon)(resources.GetObject("tray.Icon")));
            this.tray.Text = "SuperSimpleSync";
            this.tray.DoubleClick += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setLocalDirectoryToolStripMenuItem1});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(173, 26);
            this.contextMenuStrip1.Text = "ContextMenu";
            // 
            // setLocalDirectoryToolStripMenuItem1
            // 
            this.setLocalDirectoryToolStripMenuItem1.Name = "setLocalDirectoryToolStripMenuItem1";
            this.setLocalDirectoryToolStripMenuItem1.Size = new System.Drawing.Size(172, 22);
            this.setLocalDirectoryToolStripMenuItem1.Text = "Set Local Directory";
            this.setLocalDirectoryToolStripMenuItem1.Click += new System.EventHandler(this.openFolderBrowser);
            // 
            // Program
            // 
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.Name = "Program";
            this.ShowInTaskbar = false;
            this.Text = "SuperSimpleSync";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Program_FormClosed);
            this.Resize += new System.EventHandler(this.Program_Resize);
            this.trayMenu.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void Program_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.tray.Visible = false;
        }

        private void Program_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                this.tray.Visible = true;
                this.tray.ShowBalloonTip(250);
                this.Hide();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = this.lastState;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
            Environment.Exit(1);
        }

        private void openFolderBrowser(object sender, EventArgs e)
        {
            //
            // This event handler was created by double-clicking the window in the designer.
            // It runs on the program's startup routine.
            //
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                directoryPath = folderBrowserDialog1.SelectedPath;
                Directory.CreateDirectory(directoryPath);
                LocalStorage = new System.IO.DirectoryInfo(directoryPath);
            }
        }
    }
}
