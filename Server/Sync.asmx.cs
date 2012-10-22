using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using Common;

namespace Server
{
    /// <summary>
    /// Summary description for Sync
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Sync : System.Web.Services.WebService
    {
        private DirectoryInfo StorageDir = new DirectoryInfo(@"G:\Temp\Test\Sync A");

        public DirectoryInfo GetAccountStorageDir(Guid accountId)
        {
            DirectoryInfo storageDir = new DirectoryInfo(StorageDir.FullName + Path.DirectorySeparatorChar + accountId.ToString());
            if (!storageDir.Exists)
                storageDir.Create();
            storageDir.Refresh();
            return storageDir;
        }

        [WebMethod]
        public byte[] GetFileFromServer(Guid accountId, string path)
        {
            DirectoryInfo ServerStorage = GetAccountStorageDir(accountId);
            return File.ReadAllBytes(ServerStorage.FullName + path);
        }

        [WebMethod]
        public void SendFileToServer(Guid accountId, string path, byte[] buffer)
        { 
            DirectoryInfo ServerStorage = GetAccountStorageDir(accountId);
            EnsureDirectoryStructure(accountId, path, ServerStorage);
            File.WriteAllBytes(ServerStorage.FullName + path, buffer);
        }

        private void EnsureDirectoryStructure(Guid accountId, string path, DirectoryInfo baseDir)
        {
            path = path.Replace(Path.GetFileName(path), string.Empty);
            string[] parts = path.Split(Path.DirectorySeparatorChar);
            DirectoryInfo dir = baseDir;
            foreach (string part in parts)
            {
                DirectoryInfo sub = new DirectoryInfo(dir.FullName + Path.DirectorySeparatorChar + part);
                if (!sub.Exists)
                {
                    sub.Create();
                    sub.Refresh();
                }
                dir = sub;
            }
        }

        [WebMethod]
        public string GetServerSyncDir(Guid accountId, string rootDir)
        {
            DirectoryInfo ServerStorage = GetAccountStorageDir(accountId);
            if (!ServerStorage.Exists)
                ServerStorage.Create();
            DirectoryInfo root = new DirectoryInfo(ServerStorage.FullName + Path.DirectorySeparatorChar + rootDir);
            if (!root.Exists)
                root.Create();
            root.Refresh();
            try
            {
                SyncDir dir = Util.AuditTree(root);
                return Util.ToXml<SyncDir>(dir);
            }
            catch (Exception)
            { }
            return null;
        }

    }
}
