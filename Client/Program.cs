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
        private static DirectoryInfo LocalStorage = new System.IO.DirectoryInfo(@"C:\temp\TestSyncDir");
        private static Guid accountId = Guid.Parse("{FC948776-0FA5-4ABC-A2F3-E8AC8005DFCA}"); 
        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.NotifyIcon tray;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        FormWindowState lastState = new FormWindowState();
        FileSystemWatcher watcher = new FileSystemWatcher();
        private ToolStripMenuItem syncNowToolStripMenuItem;
        SyncServer.Sync _sync = new SyncServer.Sync();

        static void Main() 
        {
            Program p = new Program();
            p.InitializeComponent();
            p.SyncWithServer();
            Application.Run(p);
        }        
                
        private void SyncWithServer()
        {
            _sync.Timeout = 600000;
            //sync once on start up
            SyncNow();

            //monitor local files and sync with server on changes
            watcher.Path = @"C:\temp\TestSyncDir";
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastWrite
                | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";
            watcher.EnableRaisingEvents = true;

            // Add event handlers
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
        }

        private void SyncNow()
        {
            SyncDir local = Util.AuditTree(LocalStorage);
            SyncDir server = Util.FromXml<SyncDir>(_sync.GetServerSyncDir(accountId, local.Name));
            Util.RebuildParentRelationships(server);
            ResolveDifferencesWithLocal(server.GetNewerFiles(local), LocalStorage);
            ResolveDifferencesWithServer(local.GetNewerFiles(server), LocalStorage);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            SyncNow();
            Thread.Sleep(10000);
        }

        private void OnRenamed(object sender, FileSystemEventArgs e)
        {
            SyncNow();
            Thread.Sleep(10000);
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
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.syncNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tray = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.syncNowToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(128, 70);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // syncNowToolStripMenuItem
            // 
            this.syncNowToolStripMenuItem.Name = "syncNowToolStripMenuItem";
            this.syncNowToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.syncNowToolStripMenuItem.Text = "Sync Now";
            this.syncNowToolStripMenuItem.Click += new System.EventHandler(this.syncNowToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // tray
            // 
            this.tray.BalloonTipText = "SuperSimpleSync";
            this.tray.ContextMenuStrip = this.trayMenu;
            this.tray.Icon = ((System.Drawing.Icon)(resources.GetObject("tray.Icon")));
            this.tray.Text = "SuperSimpleSync";
            this.tray.Visible = true;
            this.tray.DoubleClick += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // Program
            // 
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.Name = "Program";
            this.ShowIcon = false;
            this.Text = "SuperSimpleSync";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Program_FormClosed);
            this.Resize += new System.EventHandler(this.Program_Resize);
            this.trayMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void Program_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.tray.Visible = false;
        }

        private void Program_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
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

        private void syncNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SyncNow();
        }
    }
}
