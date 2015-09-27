namespace PsISEProjectExplorer.Model
{
    public class ChangePoolEntry
    {
        public string PathChanged { get; set; }

        public string PathAfterRename { get; set; }

        public string RootPath { get; set; }

        public ChangePoolEntry(string pathChanged, string rootPath, string pathAfterRename)
        {
            this.PathChanged = pathChanged;
            this.RootPath = rootPath;
            this.PathAfterRename = pathAfterRename;
        }

        public ChangePoolEntry(string pathChanged, string rootPath)
            : this(pathChanged, rootPath, null)
        {
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ChangePoolEntry))
            {
                return false;
            }
            var other = (ChangePoolEntry)obj;
            return (other.PathChanged == this.PathChanged && other.RootPath == this.RootPath && other.PathAfterRename == this.PathAfterRename);
        }

        public override int GetHashCode()
        {
            return ((this.PathChanged ?? "") + (this.RootPath ?? "") + (this.PathAfterRename ?? "")).GetHashCode();
        }
    }
}
