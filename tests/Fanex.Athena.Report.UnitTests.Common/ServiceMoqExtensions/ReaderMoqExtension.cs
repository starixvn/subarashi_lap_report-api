using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Moq;

namespace Fanex.Athena.Report.UnitTests.Common.ServiceMoqExtensions
{
    public static class ReaderMoqExtension
    {
        public static void SetupToggleRead(this Mock<IDataReader> reader)
        {
            var toggle = true;

            reader.Setup(x => x.Read())
                  .Returns(() => toggle)
                  .Callback(() => toggle = false);
        }
    }
}
