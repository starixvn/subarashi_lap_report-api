namespace Fanex.Athena.Report.BetList.Models
{
    public class BetListResultModel<T>
    {
        public T Data { get; set; }

        public BetListSettingModel Settings { get; set; }
    }
}
