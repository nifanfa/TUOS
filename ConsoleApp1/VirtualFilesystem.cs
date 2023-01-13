namespace ConsoleApp1
{
    public abstract class VirtualFilesystem
    {
        public string[] GetFiles(string path) => GetFilesOrDirectories(path, false);

        public string[] GetDirectories(string path) => GetFilesOrDirectories(path, true);

        public abstract string[] GetFilesOrDirectories(string _path, bool directory);

        public abstract byte[] ReadAllBytes(string _path);

        public abstract void WriteAllBytes(string fullname, byte[] buffer);

        public abstract void CreateDirectory(string fullname);
    }
}
