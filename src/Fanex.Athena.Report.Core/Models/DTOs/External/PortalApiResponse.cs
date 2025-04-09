namespace Fanex.Athena.Report.Core.Models.DTOs.External
{
    public class PortalApiResponse
    {
        public int ErrorCode { get; set; }

        public string ErrorMsg { get; set; }
    }

    public class PortalApiResponse<T> : PortalApiResponse
    {
        public T Data { get; set; }
    }
}
