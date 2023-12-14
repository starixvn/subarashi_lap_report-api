namespace Fanex.Athena.Report.Core.Utility.Interfaces
{
    public interface IFileSystem
    {
        bool Exists(string path);

        string ReadFile(string path);
    }
}
