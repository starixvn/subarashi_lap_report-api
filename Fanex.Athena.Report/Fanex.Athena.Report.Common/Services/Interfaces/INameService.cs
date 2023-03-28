namespace Fanex.Athena.Report.Common.Services.Interfaces
{
    public interface INameService
    {
        string GetBetTypeName(int betTypeId, string lang = null);

        string GetSportName(int sportTypeId, string lang = null);
    }
}
