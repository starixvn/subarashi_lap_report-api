using System;

namespace Fanex.Athena.Report.Core.Models
{
    public interface IBalanceUpAndMoveBetMetaModel
    {
        DateTime MoveBetsDay { get; set; }

        DateTime BalanceUpDay { get; set; }
    }
}