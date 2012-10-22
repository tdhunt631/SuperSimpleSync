using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

namespace Common
{
    public static class Util
    {
        public static SyncDir AuditTree(DirectoryInfo currentDir, SyncDir parentDir = null)
        {
            currentDir.Refresh();
            if (!currentDir.Exists)
                return null;
            SyncDir dir = new SyncDir() { Name = currentDir.Name, Parent = parentDir };
            List<SyncFile> files = new List<SyncFile>();
            foreach (FileInfo f in currentDir.GetFiles())
            {
                files.Add(new SyncFile() 
                { 
                    DateCreated = f.CreationTime, 
                    DateModified = f.LastWriteTime, 
                    Name = f.Name,
                    Hash = GetMD5HashFromFile(f),
                    Directory = dir
                });
            }
            dir.Files = files.ToArray();
            List<SyncDir> dirs = new List<SyncDir>();
            foreach (DirectoryInfo d in currentDir.GetDirectories())
            {
                dirs.Add(AuditTree(d, parentDir: dir));
            }
            dir.Directories = dirs.ToArray();
            return dir;
        }

        public static string ToXml<T>(T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        public static T FromXml<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return default(T);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (StringReader stringReader = new StringReader(xml))
            {
                using (XmlTextReader xmlReader = new XmlTextReader(stringReader))
                {
                    return (T)xmlSerializer.Deserialize(xmlReader);
                }
            }
        }

        public static void RebuildParentRelationships(SyncDir parent)
        {
            if (parent.Files != null)
            {
                foreach (SyncFile file in parent.Files)
                {
                    file.Directory = parent;
                }
            }
            if (parent.Directories != null)
            {
                foreach (SyncDir child in parent.Directories)
                {
                    child.Parent = parent;
                    RebuildParentRelationships(child);
                }
            }
        }

        public static string GetMD5HashFromFile(FileInfo f)
        {
            byte[] retVal;
            using (FileStream file = f.OpenRead())
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                retVal = md5.ComputeHash(file);
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
