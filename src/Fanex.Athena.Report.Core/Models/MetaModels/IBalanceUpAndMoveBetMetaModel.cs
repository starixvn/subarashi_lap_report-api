namespace Fanex.Athena.Report.Core.Models.MetaModels
{
    using System;

    public interface IBalanceUpAndMoveBetMetaModel
    {
        DateTime MoveBetsDay { get; set; }

        DateTime BalanceUpDay { get; set; }
    }
}