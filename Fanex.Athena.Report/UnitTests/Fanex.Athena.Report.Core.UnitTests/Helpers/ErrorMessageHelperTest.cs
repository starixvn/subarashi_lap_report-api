using System;
using Fanex.Athena.Report.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Helpers
{
    public class ErrorMessageHelperTest
    {
        [Fact]
        public void GenerateMessage_WebException_BuildMessage()
        {
            var context = new DefaultHttpContext();
            var exception = new NullReferenceException();

            var result = ErrorMessageHelper.GenerateMessage(context.HttpContext, exception);

            Assert.Equal($"ErrMsg: {exception.Message}", result.Substring(0, result.IndexOf(Environment.NewLine)));
        }
    }
}