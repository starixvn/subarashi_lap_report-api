namespace Fanex.Athena.Report.Core.Services.Interfaces;

using Models.DTOs;

public interface IIpService
{
    bool IsVNIP(string ipAddress);

    bool IsAlwaysIpvn(string ipAddress);

    Location GetIpLocation(string ipAddress, bool isShowIpvn = false);

    string GetIpAddress(string ipAddress, bool isShowIpvn = false);
}