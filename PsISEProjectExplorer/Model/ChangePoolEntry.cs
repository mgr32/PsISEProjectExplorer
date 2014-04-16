using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Model
{
    public class ChangePoolEntry
    {
        public string PathChanged { get; set; }

        public string RootPath { get; set; }

        public ChangePoolEntry(string pathChanged, string rootPath)
        {
            this.PathChanged = pathChanged;
            this.RootPath = rootPath;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ChangePoolEntry))
            {
                return false;
            }
            var other = (ChangePoolEntry)obj;
            return (other.PathChanged == this.PathChanged && other.RootPath == this.RootPath);
        }

        public override int GetHashCode()
        {
            return ((this.PathChanged ?? "") + (this.RootPath ?? "")).GetHashCode();
        }
    }
}
