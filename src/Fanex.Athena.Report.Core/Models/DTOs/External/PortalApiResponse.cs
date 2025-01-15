namespace Fanex.Athena.Report.Core.Models.DTOs.External
{
    public class PortalApiResponse
    {
        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }
    }

    public class PortalApiResponse<T> : PortalApiResponse
    {
        public T Data { get; set; }
    }
}