namespace Fanex.Athena.Report.Common.Services.Interfaces
{
    public interface INameService
    {
        string GetBetTypeName(int betTypeId, bool isHideSaba = false, string lang = null);

        string GetSportName(int sportTypeId, bool isHideSaba = false, string lang = null);
    }
}