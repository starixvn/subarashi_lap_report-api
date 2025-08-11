namespace Fanex.Athena.Report.BySite.UnitTests.TestHelpers
{
    using System.Collections.Generic;
    using AutoFixture;
    using BySite.MetaModels;

    public static class MetaModelHelper
    {
        private static Fixture fixture = new Fixture();

        public static DailyBetsMetaModel CreateDailyBetsMetaModel()
            => fixture.Build<DailyBetsMetaModel>()
                .With(_ => _.CurrencyIdList, "1,2,3")
                .With(_ => _.CurrencyList, new Dictionary<int, string>
                { { 1, "USD" }, { 2, "EUR" }, { 3, "UUS" } })
                .Create();
    }
}