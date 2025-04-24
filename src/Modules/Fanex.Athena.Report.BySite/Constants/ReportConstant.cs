using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.Constants
{
    public static class ReportConstant
    {
        public const string ALL = "ALL";

        public static class LiveStatusTypes
        {
            public const int Mix = 2;
            public const int NonLive = 0;
            public const int Live = 1;
            public const int All = 3;
        }

        public static readonly List<int> LiveReportNoDetailLinkSports = new() {
            162, 251, 252, 253, 202, 203, 208, 209, 210, 211, 212, 220, 248, 165, 247, 245, 222, 223, 243,
            200, 244, 232, 234, 238, 233, 239, 171, 167, 172, 174, 176, 237, 158, 159, 155, 156, 228, 227,
            216, 230, 146, 229, 138, 148, 145, 144, 143, 130, 131, 139, 124, 129, 123
        };
    }
}