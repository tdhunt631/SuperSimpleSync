using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using Common;
using System.Threading;

namespace SuperSimpleSync
{
    class Program
    {
        private DirectoryInfo LocalStorage = new System.IO.DirectoryInfo(@"C:\temp\TestSyncDir");
        private Guid accountId = Guid.Parse("{FC948776-0FA5-4ABC-A2F3-E8AC8005DFCA}");
        SyncServer.Sync _sync = new SyncServer.Sync();

        static void Main(string[] args)
        {
            Program p = new Program();
            try
            {
                p.SyncWithServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error syncing: " + ex.Message);
                Environment.Exit(1);
            }
            Environment.Exit(0);
        }

        private void SyncWithServer()
        {
            SyncDir local = Util.AuditTree(LocalStorage);
            SyncDir server = Util.FromXml<SyncDir>(_sync.GetServerSyncDir(accountId, local.Name));
            Util.RebuildParentRelationships(server);
            ResolveDifferencesWithLocal(server.GetNewerFiles(local), LocalStorage);
            ResolveDifferencesWithServer(local.GetNewerFiles(server), LocalStorage);
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
    }
}
