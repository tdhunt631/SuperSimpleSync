using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace Common
{
    [Serializable]
    [XmlInclude(typeof(SyncDir))]
    [XmlInclude(typeof(SyncFile))]
    public class SyncDir : IEquatable<SyncDir>
    {
        [XmlAttribute]
        public string Name { get; set; }
        public SyncDir[] Directories { get; set; }
        public SyncFile[] Files { get; set; }
        [XmlIgnore]
        public SyncDir Parent { get; set; }

        public bool Equals(SyncDir other)
        {
            if (other == null || Name != other.Name)
                return false;
            foreach (SyncFile f in Files)
            {
                if (!f.Equals(other.GetFile(f.Name)))
                    return false;
            }
            foreach (SyncDir d in Directories)
            {
                if (!d.Equals(other.GetSubDirectory(d.Name)))
                    return false;
            }
            return true;
        }

        public SyncFile GetFile(string name)
        {
            foreach (SyncFile f in Files)
            {
                if (f.Name == name)
                    return f;
            }
            return null;
        }

        public SyncDir GetSubDirectory(string name)
        {
            foreach (SyncDir d in Directories)
            {
                if (d.Name == name)
                    return d;
            }
            return null;
        }

        public override string ToString()
        {
            return Name;
        }

        public SyncDir GetNewerFiles(SyncDir other)
        {
            if (other == null)
                return this.Copy();
            //Assume this is Authoritative.
            SyncDir dir = new SyncDir() { Name = Name };
            List<SyncFile> files = new List<SyncFile>();
            foreach (SyncFile f in this.Files)
            {
                SyncFile theirs = other.GetFile(f.Name);
                if (theirs == null || f.DateModified > theirs.DateModified)
                    files.Add(f);
            }
            dir.Files = files.ToArray();
            List<SyncDir> dirs = new List<SyncDir>();
            foreach (SyncDir d in Directories)
            {
                SyncDir theirs = other.GetSubDirectory(d.Name);
                dirs.Add(d.GetNewerFiles(theirs));
            }
            dir.Directories = dirs.ToArray();
            return dir;
        } 

        public SyncDir Copy()
        {
            SyncDir dir = new SyncDir() { Name = this.Name };
            List<SyncFile> files = new List<SyncFile>();
            foreach (SyncFile f in Files)
            {
                files.Add(new SyncFile() { 
                    DateCreated = f.DateCreated, 
                    DateModified = f.DateModified, 
                    Directory = f.Directory, 
                    Hash = f.Hash, 
                    Name = f.Name });
            }
            dir.Files = files.ToArray();
            List<SyncDir> dirs = new List<SyncDir>();
            foreach (SyncDir d in Directories)
            {
                dirs.Add(d.Copy());
            }
            Directories = dirs.ToArray();
            return dir;
        }

        public string BuildPath()
        {
            string path = string.Empty;
            SyncDir dir = this;
            do
            {
                path = Path.DirectorySeparatorChar + dir.Name + path;
                dir = dir.Parent;
            } while (dir != null);
            return path;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
    }
}
