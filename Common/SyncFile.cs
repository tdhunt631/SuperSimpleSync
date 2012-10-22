using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace Common
{
    [Serializable]
    public class SyncFile : IEquatable<SyncFile>
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public DateTime DateCreated { get; set; }
        [XmlAttribute]
        public DateTime DateModified { get; set; }
        [XmlAttribute]
        public string Hash { get; set; }
        [XmlIgnore]
        public SyncDir Directory { get; set; }

        public bool Equals(SyncFile other)
        {
            return other != null && other.Name == Name && other.Hash == Hash;
        }

        public override string ToString()
        {
            return Name;
        }

        public string BuildPath()
        {
            return Directory.BuildPath() + Path.DirectorySeparatorChar + Name;
        }
    }
}
