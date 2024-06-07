namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    public interface IDataPermissionService
    {
        T ClearDataByLimitPermission<T>(T data);
    }
}
