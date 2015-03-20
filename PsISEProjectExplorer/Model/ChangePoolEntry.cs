namespace PsISEProjectExplorer.Model
{
	public class ChangePoolEntry
    {
        public string PathChanged { get; set; }

        public string PathAfterRename { get; set; }

        public string RootPath { get; set; }

        public ChangePoolEntry(string pathChanged, string rootPath, string pathAfterRename)
        {
			PathChanged = pathChanged;
			RootPath = rootPath;
			PathAfterRename = pathAfterRename;
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
            return (other.PathChanged == PathChanged && other.RootPath == RootPath && other.PathAfterRename == PathAfterRename);
        }

        public override int GetHashCode()
        {
            return ((PathChanged ?? "") + (RootPath ?? "") + (PathAfterRename ?? "")).GetHashCode();
        }
    }
}
