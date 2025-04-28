namespace Fanex.Athena.Report.Core.Repositories.Abstraction;

using System;
using System.Collections.Generic;
using Models.DTOs;

public interface IIpRepository
{
    Tuple<IList<IPV4RangeEntity>, IList<IPV6RangeEntity>> GetVNIPs();

    Location GetIpLocation(string IPAddress);
}