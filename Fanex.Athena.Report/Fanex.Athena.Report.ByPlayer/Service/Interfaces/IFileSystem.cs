namespace Fanex.Athena.Report.ByPlayer.Service.Interfaces
{
    public interface IFileSystem
    {
        void Delete(string path);

        byte[] ReadAllBytes(string path);

        string ReadString(string path);

        void Save(string path, string content);
    }
}