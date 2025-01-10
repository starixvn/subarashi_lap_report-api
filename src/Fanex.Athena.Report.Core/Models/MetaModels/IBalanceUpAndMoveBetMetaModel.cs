using System;

namespace Fanex.Athena.Report.Core.Models.MetaModels
{
    public interface IBalanceUpAndMoveBetMetaModel
    {
        DateTime MoveBetsDay { get; set; }

        DateTime BalanceUpDay { get; set; }
    }
}